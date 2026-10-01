using Microsoft.EntityFrameworkCore;
using Orama.Application.Services.Fiscal;
using Orama.Application.Services.Fiscal.NFe;
using Orama.Domain.Entities;
using Orama.Domain.Entities.Fiscal;
using Orama.Domain.Entities.Fiscal.NFe;
using Orama.Infra.Data.Context;
using Xunit;

namespace Orama.Tests.Integration;

public sealed class TenantFiscalIsolationTests
{
    [Fact]
    public async Task Fiscal_company_configuration_cannot_be_updated_or_closed_by_another_company()
    {
        await using var context = CreateContext();
        var config = new EmpresaFiscalConfig
        {
            Id = 9201,
            EmpresaId = 1,
            RegimeTributario = RegimeTributario.SimplesNacional,
            CRT = CRT.SimplesNacional,
            UF = "SP",
            AmbienteFiscal = AmbienteFiscal.Homologacao,
            VersaoFiscal = "2026.1",
            VigenteDe = DateTime.Today.AddDays(-1)
        };
        context.EmpresasFiscaisConfig.Add(config);
        await context.SaveChangesAsync();
        var service = new EmpresaFiscalService(context);

        var forged = new EmpresaFiscalConfig
        {
            Id = config.Id,
            EmpresaId = 2,
            RegimeTributario = config.RegimeTributario,
            CRT = config.CRT,
            UF = config.UF,
            AmbienteFiscal = config.AmbienteFiscal,
            VersaoFiscal = config.VersaoFiscal,
            VigenteDe = config.VigenteDe
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.AtualizarConfiguracaoAsync(forged));
        await Assert.ThrowsAsync<ArgumentException>(() => service.EncerrarVigenciaAsync(config.Id, 2, DateTime.Today));
        Assert.Null((await context.EmpresasFiscaisConfig.FindAsync(config.Id))!.VigenteAte);
    }

    [Fact]
    public async Task Product_tax_configuration_is_scoped_through_the_product_tenant()
    {
        await using var context = CreateContext();
        context.Produtos.Add(new Produto
        {
            Id = 9301,
            EmpresaId = 1,
            Codigo = "FISCAL-1",
            Descricao = "Produto da empresa A",
            Unidade = "UN"
        });
        context.ProdutosFiscaisConfig.Add(new ProdutoFiscalConfig
        {
            Id = 9302,
            ProdutoId = 9301,
            NCM = "12345678",
            Origem = OrigemMercadoria.Nacional,
            CSOSN = "102",
            VigenteDe = DateTime.Today.AddDays(-1)
        });
        await context.SaveChangesAsync();

        var service = new ContextoFiscalService(context);
        var (_, forOwner, _) = await service.ObterConfiguracoesVigentesAsync(1, 9301, TipoOperacaoFiscal.Venda, DateTime.Today);
        var (_, forOtherTenant, _) = await service.ObterConfiguracoesVigentesAsync(2, 9301, TipoOperacaoFiscal.Venda, DateTime.Today);

        Assert.NotNull(forOwner);
        Assert.Null(forOtherTenant);
    }

    [Fact]
    public async Task Nfe_operations_reject_documents_owned_by_another_tenant()
    {
        await using var context = CreateContext();
        context.Vendas.Add(new Venda
        {
            Id = 9401,
            EmpresaId = 1,
            Numero = "V-9401",
            ClienteId = 1,
            DataVenda = DateTime.UtcNow,
            Status = StatusVenda.Faturada,
            Ativo = true
        });
        context.NFeDocumentos.Add(new NFeDocumento
        {
            Id = 9402,
            VendaId = 9401,
            Numero = 1,
            Serie = 1,
            ChaveAcesso = new string('1', 44),
            AmbienteFiscal = AmbienteFiscal.Homologacao,
            Status = NFeStatus.Autorizada,
            DataEmissao = DateTime.Today
        });
        await context.SaveChangesAsync();

        var consulta = new NFeConsultaService(context, null!);
        var cancelamento = new NFeCancelamentoService(context, null!);
        var emissao = new NFeEmissaoService(context, null!, null!, null!);

        await Assert.ThrowsAsync<ArgumentException>(() => consulta.ConsultarSituacaoAsync(9402, 2, 1));
        await Assert.ThrowsAsync<ArgumentException>(() => cancelamento.CancelarNFeAsync(9402, 2, "Justificativa cross tenant", 1));
        Assert.False((await cancelamento.PodeCancelarAsync(9402, 2)).Pode);
        await Assert.ThrowsAsync<ArgumentException>(() => emissao.AssinarEEnviarAsync(9402, 2, 1));
        Assert.Contains("Venda não encontrada", await emissao.ValidarVendaParaNFeAsync(9401, 2));

        Assert.Equal(NFeStatus.Autorizada, (await context.NFeDocumentos.FindAsync(9402))!.Status);
    }

    private static OramaDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OramaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new OramaDbContext(options);
    }
}
