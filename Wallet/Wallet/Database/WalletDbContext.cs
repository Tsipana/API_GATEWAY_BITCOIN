using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wallet.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Wallet.Database
{
    public class WalletDbContext : DbContext
    {
        public DbSet<TransactionRecord> Transactions { get; set;  }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(
                "Data Source=localhost,1433;" +
                "Persist Security Info=True;" +
                "User ID=sa;" +
                "Password=Password01;" +
                "Database=WalletDb");
    }
}
