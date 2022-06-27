using System;
using System.ComponentModel.DataAnnotations;

namespace Wallet.Database.Entities
{
    public class TransactionRecord
    {
        [Required]
        public long Id { get; set; }
        [Required]
        public string Symbol { get; set; }
        [Required]
        public string Transaction_Type { get; set; }
        [Required]
        public float Qty { get; set;  }

    }
}
