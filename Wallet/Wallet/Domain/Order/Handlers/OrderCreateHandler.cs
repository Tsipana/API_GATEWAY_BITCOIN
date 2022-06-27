using Microsoft.EntityFrameworkCore;
using Wallet.DTO;
using Wallet.Events.Publishers;

namespace Wallet.Domain.Order.Handlers
{
    public interface IOrderCreateHandler
    {
        void Handle(DbContext dbContext, OrderDto dto);
    }

    public class OrderCreateHandler : IOrderCreateHandler
    {
        private readonly IOrderPlacedPublisher _orderPlacedPublisher;

        public OrderCreateHandler(IOrderPlacedPublisher parmOrderplacedpublisher)
        {
            _orderPlacedPublisher = parmOrderplacedpublisher;
        }

        public void Handle(DbContext dbContext, OrderDto dto)
        {
            var OrderEntity = dto.ToEntity();
            if (OrderEntity != null && OrderEntity.Transaction_Type != null)
            {
                
                dbContext.Add(OrderEntity);
                dbContext.SaveChanges();
            }
           

            var orderPlacedEventData = OrderEntity.ToOrderPlacedEvent();

            _orderPlacedPublisher.Publish(orderPlacedEventData);
        }
    }
}
