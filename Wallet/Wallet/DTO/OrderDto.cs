using System;

namespace Wallet.DTO
{
    public class OrderDto
    {
        public long Id { get; set; }
        public string Symbol { get; set; }
        public string Transaction_Type { get; set; }
        public float Qty { get; set; }

    }
}
