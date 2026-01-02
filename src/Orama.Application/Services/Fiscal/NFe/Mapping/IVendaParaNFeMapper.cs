using Orama.Domain.Entities;
using Orama.Domain.Entities.Fiscal.NFe;

namespace Orama.Application.Services.Fiscal.NFe.Mapping
{
    /// <summary>
    /// Interface para mapeamento de Venda para NF-e
    /// </summary>
    public interface IVendaParaNFeMapper
    {
        /// <summary>
        /// Mapeia uma venda para NF-e
        /// </summary>
        Task<NFeDocumento> MapearVendaParaNFeAsync(Venda venda, int usuarioId);
        
        /// <summary>
        /// Mapeia um item de venda para item de NF-e
        /// </summary>
        Task<NFeItem> MapearItemVendaParaNFeAsync(VendaItem vendaItem, int numeroItem);
    }
}