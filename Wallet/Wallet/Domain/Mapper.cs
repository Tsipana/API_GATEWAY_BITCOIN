using Wallet.DTO;
using Wallet.Events.Publishers;
using OrderEntity = Wallet.Database.Entities.TransactionRecord;

namespace Wallet.Domain.Order
{
    public static class Mapper
    {
        public static OrderPlacedEventData ToOrderPlacedEvent(this OrderEntity entity)
            => new OrderPlacedEventData
            {
                Id = entity.Id,
                Symbol = entity.Symbol,
                Transaction_Type = entity.Transaction_Type,
            };

        public static OrderDto ToDto(this OrderEntity entity)
            => new OrderDto
            {
                Id = entity.Id,
                Symbol = entity.Symbol,
                Transaction_Type = entity.Transaction_Type,
                Qty = entity.Qty
            };

        public static OrderEntity ToEntity(this OrderDto dto)
            => new OrderEntity
            {
                Id = dto.Id,
                Symbol = dto.Symbol,
                Transaction_Type = dto.Transaction_Type,
                Qty = dto.Qty
            };
    }
}