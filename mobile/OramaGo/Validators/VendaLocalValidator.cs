using FluentValidation;
using OramaGo.Models;

namespace OramaGo.Validators;

public class VendaLocalValidator : AbstractValidator<VendaLocal>
{
    public VendaLocalValidator()
    {
        RuleFor(v => v.ClienteId)
            .GreaterThan(0)
            .WithMessage("Selecione um cliente");

        RuleFor(v => v.DataVenda)
            .NotEmpty()
            .WithMessage("Data da venda é obrigatória")
            .LessThanOrEqualTo(DateTime.Today.AddDays(30))
            .WithMessage("Data da venda não pode ser superior a 30 dias no futuro");

        RuleFor(v => v.ValorTotal)
            .GreaterThan(0)
            .WithMessage("Valor total deve ser maior que zero");

        RuleFor(v => v.ValorDesconto)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Valor do desconto não pode ser negativo")
            .LessThanOrEqualTo(v => v.SubTotal)
            .WithMessage("Valor do desconto não pode ser maior que o subtotal");

        RuleFor(v => v.PercentualDesconto)
            .InclusiveBetween(0, 100)
            .WithMessage("Percentual de desconto deve estar entre 0 e 100");

        RuleFor(v => v.ValorFrete)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Valor do frete não pode ser negativo");

        RuleFor(v => v.Parcelas)
            .InclusiveBetween(1, 48)
            .WithMessage("Número de parcelas deve estar entre 1 e 48");

        RuleFor(v => v.Observacoes)
            .MaximumLength(500)
            .WithMessage("Observações não podem exceder 500 caracteres");

        // Validação condicional para parcelas
        RuleFor(v => v.Parcelas)
            .Equal(1)
            .When(v => v.FormaPagamento == FormaPagamentoLocal.AVista)
            .WithMessage("Pagamento à vista deve ter apenas 1 parcela");

        RuleFor(v => v.Parcelas)
            .GreaterThan(1)
            .When(v => v.FormaPagamento == FormaPagamentoLocal.Parcelado)
            .WithMessage("Pagamento parcelado deve ter mais de 1 parcela");
    }
}