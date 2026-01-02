using Orama.Domain.Entities.Fiscal;
using Orama.Domain.Entities.Fiscal.NFe;
using System.Text;

namespace Orama.Application.Services.Fiscal.NFe
{
    /// <summary>
    /// Gateway SEFAZ Mock para desenvolvimento e testes
    /// Simula comportamento da SEFAZ sem fazer chamadas reais
    /// </summary>
    public class SefazNFeGatewayMock : ISefazNFeGateway
    {
        public async Task<string> GerarXmlAsync(NFeDocumento nfe)
        {
            await Task.Delay(100); // Simula processamento
            
            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine("<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\">");
            xml.AppendLine("  <infNFe Id=\"NFe" + nfe.ChaveAcesso + "\">");
            xml.AppendLine("    <ide>");
            xml.AppendLine($"      <cUF>{ObterCodigoUF("SP")}</cUF>");
            xml.AppendLine($"      <cNF>{new Random().Next(10000000, 99999999)}</cNF>");
            xml.AppendLine("      <natOp>Venda de mercadoria adquirida ou recebida de terceiros</natOp>");
            xml.AppendLine("      <mod>55</mod>");
            xml.AppendLine($"      <serie>{nfe.Serie}</serie>");
            xml.AppendLine($"      <nNF>{nfe.Numero}</nNF>");
            xml.AppendLine($"      <dhEmi>{nfe.DataEmissao:yyyy-MM-ddTHH:mm:sszzz}</dhEmi>");
            xml.AppendLine($"      <tpNF>1</tpNF>");
            xml.AppendLine("      <idDest>1</idDest>");
            xml.AppendLine("      <cMunFG>3550308</cMunFG>");
            xml.AppendLine($"      <tpImp>1</tpImp>");
            xml.AppendLine("      <tpEmis>1</tpEmis>");
            xml.AppendLine("      <cDV>0</cDV>");
            xml.AppendLine($"      <tpAmb>{(int)nfe.AmbienteFiscal}</tpAmb>");
            xml.AppendLine("      <finNFe>1</finNFe>");
            xml.AppendLine("      <indFinal>1</indFinal>");
            xml.AppendLine("      <indPres>1</indPres>");
            xml.AppendLine("    </ide>");
            
            // Emitente (dados mock)
            xml.AppendLine("    <emit>");
            xml.AppendLine("      <CNPJ>12345678000195</CNPJ>");
            xml.AppendLine("      <xNome>EMPRESA TESTE LTDA</xNome>");
            xml.AppendLine("      <enderEmit>");
            xml.AppendLine("        <xLgr>RUA TESTE</xLgr>");
            xml.AppendLine("        <nro>123</nro>");
            xml.AppendLine("        <xBairro>CENTRO</xBairro>");
            xml.AppendLine("        <cMun>3550308</cMun>");
            xml.AppendLine("        <xMun>SAO PAULO</xMun>");
            xml.AppendLine("        <UF>SP</UF>");
            xml.AppendLine("        <CEP>01000000</CEP>");
            xml.AppendLine("      </enderEmit>");
            xml.AppendLine("      <IE>123456789012</IE>");
            xml.AppendLine("      <CRT>1</CRT>");
            xml.AppendLine("    </emit>");
            
            // Destinatário (dados mock)
            xml.AppendLine("    <dest>");
            xml.AppendLine("      <CPF>12345678901</CPF>");
            xml.AppendLine("      <xNome>CLIENTE TESTE</xNome>");
            xml.AppendLine("      <enderDest>");
            xml.AppendLine("        <xLgr>RUA CLIENTE</xLgr>");
            xml.AppendLine("        <nro>456</nro>");
            xml.AppendLine("        <xBairro>VILA TESTE</xBairro>");
            xml.AppendLine("        <cMun>3550308</cMun>");
            xml.AppendLine("        <xMun>SAO PAULO</xMun>");
            xml.AppendLine("        <UF>SP</UF>");
            xml.AppendLine("        <CEP>02000000</CEP>");
            xml.AppendLine("      </enderDest>");
            xml.AppendLine("      <indIEDest>9</indIEDest>");
            xml.AppendLine("    </dest>");
            
            // Itens
            for (int i = 0; i < nfe.Itens.Count; i++)
            {
                var item = nfe.Itens.ElementAt(i);
                xml.AppendLine($"    <det nItem=\"{i + 1}\">");
                xml.AppendLine("      <prod>");
                xml.AppendLine($"        <cProd>{item.CodigoProduto ?? item.ProdutoId.ToString()}</cProd>");
                xml.AppendLine($"        <cEAN></cEAN>");
                xml.AppendLine($"        <xProd>{item.Descricao}</xProd>");
                xml.AppendLine($"        <NCM>{item.NCM}</NCM>");
                xml.AppendLine($"        <CFOP>{item.CFOP}</CFOP>");
                xml.AppendLine($"        <uCom>{item.Unidade}</uCom>");
                xml.AppendLine($"        <qCom>{item.Quantidade:F4}</qCom>");
                xml.AppendLine($"        <vUnCom>{item.ValorUnitario:F4}</vUnCom>");
                xml.AppendLine($"        <vProd>{item.ValorTotal:F2}</vProd>");
                xml.AppendLine("        <cEANTrib></cEANTrib>");
                xml.AppendLine($"        <uTrib>{item.Unidade}</uTrib>");
                xml.AppendLine($"        <qTrib>{item.Quantidade:F4}</qTrib>");
                xml.AppendLine($"        <vUnTrib>{item.ValorUnitario:F4}</vUnTrib>");
                xml.AppendLine("      </prod>");
                xml.AppendLine("      <imposto>");
                xml.AppendLine("        <ICMS>");
                xml.AppendLine($"          <ICMS{item.CstCsosn}>");
                xml.AppendLine($"            <orig>{item.Origem}</orig>");
                xml.AppendLine($"            <CST>{item.CstCsosn}</CST>");
                if (item.BaseCalculoICMS.HasValue)
                {
                    xml.AppendLine($"            <vBC>{item.BaseCalculoICMS:F2}</vBC>");
                    xml.AppendLine($"            <pICMS>{item.AliquotaICMS:F2}</pICMS>");
                    xml.AppendLine($"            <vICMS>{item.ValorICMS:F2}</vICMS>");
                }
                xml.AppendLine($"          </ICMS{item.CstCsosn}>");
                xml.AppendLine("        </ICMS>");
                xml.AppendLine("      </imposto>");
                xml.AppendLine("    </det>");
            }
            
            // Totais
            var valorTotal = nfe.Itens.Sum(i => i.ValorTotal);
            var valorICMS = nfe.Itens.Sum(i => i.ValorICMS ?? 0);
            
            xml.AppendLine("    <total>");
            xml.AppendLine("      <ICMSTot>");
            xml.AppendLine($"        <vBC>{nfe.Itens.Sum(i => i.BaseCalculoICMS ?? 0):F2}</vBC>");
            xml.AppendLine($"        <vICMS>{valorICMS:F2}</vICMS>");
            xml.AppendLine("        <vICMSDeson>0.00</vICMSDeson>");
            xml.AppendLine("        <vBCST>0.00</vBCST>");
            xml.AppendLine("        <vST>0.00</vST>");
            xml.AppendLine($"        <vProd>{valorTotal:F2}</vProd>");
            xml.AppendLine("        <vFrete>0.00</vFrete>");
            xml.AppendLine("        <vSeg>0.00</vSeg>");
            xml.AppendLine("        <vDesc>0.00</vDesc>");
            xml.AppendLine("        <vII>0.00</vII>");
            xml.AppendLine("        <vIPI>0.00</vIPI>");
            xml.AppendLine("        <vPIS>0.00</vPIS>");
            xml.AppendLine("        <vCOFINS>0.00</vCOFINS>");
            xml.AppendLine("        <vOutro>0.00</vOutro>");
            xml.AppendLine($"        <vNF>{valorTotal:F2}</vNF>");
            xml.AppendLine("      </ICMSTot>");
            xml.AppendLine("    </total>");
            
            xml.AppendLine("    <transp>");
            xml.AppendLine("      <modFrete>9</modFrete>");
            xml.AppendLine("    </transp>");
            
            xml.AppendLine("    <pag>");
            xml.AppendLine("      <detPag>");
            xml.AppendLine("        <tPag>01</tPag>");
            xml.AppendLine($"        <vPag>{valorTotal:F2}</vPag>");
            xml.AppendLine("      </detPag>");
            xml.AppendLine("    </pag>");
            
            xml.AppendLine("  </infNFe>");
            xml.AppendLine("</NFe>");
            
            return xml.ToString();
        }
        
        public async Task<string> AssinarXmlAsync(string xml, AmbienteFiscal ambiente)
        {
            await Task.Delay(50); // Simula assinatura
            
            // Mock: adiciona assinatura simulada
            var xmlAssinado = xml.Replace("</NFe>", 
                "  <Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\">\n" +
                "    <!-- Assinatura digital simulada -->\n" +
                "    <SignedInfo>\n" +
                "      <CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" />\n" +
                "      <SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\" />\n" +
                "    </SignedInfo>\n" +
                "    <SignatureValue>MOCK_SIGNATURE_VALUE</SignatureValue>\n" +
                "  </Signature>\n" +
                "</NFe>");
            
            return xmlAssinado;
        }
        
        public async Task<ResultadoEnvioNFe> EnviarNFeAsync(string xmlAssinado, AmbienteFiscal ambiente)
        {
            await Task.Delay(200); // Simula envio
            
            // Mock: simula autorização em homologação, rejeição em produção (segurança)
            if (ambiente == AmbienteFiscal.Producao)
            {
                return new ResultadoEnvioNFe
                {
                    Autorizada = false,
                    CodigoStatus = 999,
                    Mensagem = "MOCK: Emissão em produção bloqueada por segurança",
                    XmlRetorno = "<retEnviNFe><cStat>999</cStat><xMotivo>Produção bloqueada</xMotivo></retEnviNFe>"
                };
            }
            
            // Homologação: simula sucesso
            var protocolo = $"135{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
            
            return new ResultadoEnvioNFe
            {
                Autorizada = true,
                Protocolo = protocolo,
                CodigoStatus = 100,
                Mensagem = "Autorizado o uso da NF-e",
                XmlRetorno = $"<retEnviNFe><cStat>100</cStat><xMotivo>Autorizado o uso da NF-e</xMotivo><protNFe><infProt><tpAmb>{(int)ambiente}</tpAmb><nProt>{protocolo}</nProt><dhRecbto>{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}</dhRecbto><cStat>100</cStat><xMotivo>Autorizado o uso da NF-e</xMotivo></infProt></protNFe></retEnviNFe>"
            };
        }
        
        public async Task<ResultadoConsultaNFe> ConsultarNFeAsync(string chaveAcesso, AmbienteFiscal ambiente)
        {
            await Task.Delay(100); // Simula consulta
            
            return new ResultadoConsultaNFe
            {
                Encontrada = true,
                CodigoStatus = 100,
                Mensagem = "Autorizado o uso da NF-e",
                Protocolo = $"135{DateTime.Now:yyyyMMddHHmmss}0001",
                DataAutorizacao = DateTime.UtcNow.AddMinutes(-5),
                XmlRetorno = $"<retConsSitNFe><cStat>100</cStat><xMotivo>Autorizado o uso da NF-e</xMotivo></retConsSitNFe>"
            };
        }
        
        public async Task<ResultadoEventoNFe> CancelarNFeAsync(string chaveAcesso, string justificativa, AmbienteFiscal ambiente)
        {
            await Task.Delay(150); // Simula cancelamento
            
            if (string.IsNullOrWhiteSpace(justificativa) || justificativa.Length < 15)
            {
                return new ResultadoEventoNFe
                {
                    Sucesso = false,
                    CodigoStatus = 999,
                    Mensagem = "Justificativa deve ter pelo menos 15 caracteres"
                };
            }
            
            var protocolo = $"135{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
            
            return new ResultadoEventoNFe
            {
                Sucesso = true,
                Protocolo = protocolo,
                CodigoStatus = 135,
                Mensagem = "Evento registrado e vinculado a NF-e",
                XmlRetorno = $"<retEventoNFe><cStat>135</cStat><xMotivo>Evento registrado e vinculado a NF-e</xMotivo></retEventoNFe>"
            };
        }
        
        public async Task<bool> VerificarStatusServicoAsync(AmbienteFiscal ambiente, string uf)
        {
            await Task.Delay(50); // Simula verificação
            return true; // Mock sempre retorna serviço disponível
        }
        
        private static string ObterCodigoUF(string uf)
        {
            var codigosUF = new Dictionary<string, string>
            {
                {"SP", "35"}, {"RJ", "33"}, {"MG", "31"}, {"RS", "43"}, {"PR", "41"}
                // Adicionar outros conforme necessário
            };
            
            return codigosUF.TryGetValue(uf.ToUpper(), out var codigo) ? codigo : "35";
        }
    }
}