using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Database;
using Wallet.Database.Entities;
using Wallet.DTO;

namespace Wallet.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WalletController : ControllerBase
    {
        private readonly WalletDbContext _dbContext;
        float XBT = 0;
        float ETHBON = 0;
        float BTHETAT = 0;

        public WalletController(WalletDbContext parm1) 
        
        { 
            _dbContext = parm1; 
        }





        [HttpGet]
        public IActionResult wallet()
        {
            return Ok("Wallet Service is running");
        }



        [HttpGet]
        public async Task<IActionResult> Totals()
        {
            List<TransactionRecord> transactionList = await _dbContext.Transactions.OrderBy(x => x.Symbol).ToListAsync();

            foreach (var item in transactionList)
            {

                if (item.Symbol == ".XBT")
                {
                    XBT = XBT + item.Qty;
                }


            }

            foreach (var item in transactionList)
            {
                
                if (item.Symbol == ".ETHBON")
                {
                    ETHBON = ETHBON + item.Qty;
                }


            }


            foreach (var item in transactionList)
            {

                if (item.Symbol == ".BTHETAT")
                {
                    BTHETAT = BTHETAT + item.Qty;
                }


            }



            return Ok("XBT:" + XBT.ToString() + "\n" +
                      "ETHBON:" + ETHBON.ToString() + "\n" +
                       "BTHETAT:" + BTHETAT.ToString()); 

        }






        [HttpGet]
        
         public async Task<IActionResult> balance() 
          {
           


            List<TransactionRecord> transactionList = await _dbContext.Transactions.OrderBy(x=>x.Symbol).ToListAsync();

           

        

            string BalanceListHTML = CreateTable(transactionList, x => x.Id, x => x.Symbol, x => x.Transaction_Type, x => x.Qty);

            var qry = _dbContext.Transactions.FirstOrDefault();


            static string CreateTable<T>(IEnumerable<T> list, params Func<T, object>[] fxns)
            {
                StringBuilder sb = new StringBuilder();
               sb.Append("---Transactions/Balances---\n");

               

                foreach (var item in list)
               {
                    sb.Append(">\n");
                    foreach (var fxn in fxns)
                  {
                        sb.Append(">");
                      sb.Append(fxn(item));
                       sb.Append(">");
                        
                    }
                     sb.Append(">\n");
              }
                sb.Append("--------------------------------------------------------------------");
              

              
               return sb.ToString();
               
             }

           
           // return Ok(bitcoin.ToString());
            return Ok(BalanceListHTML);
          
          }



        [HttpPost]
        public IActionResult Deposit([FromBody] DepositDto transaction)
        {
           
            // Create connection to RabbitMQ
            string QueueName = "DepositedCurrency";
            var factory = new ConnectionFactory
            {
                // The docker-compose.yml file defined port 5672 to be used
                // to connect to RabbitMQ
                Uri = new Uri("amqp://guest:guest@localhost:5672")
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(
                QueueName,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Serialize the object into a  string
            var serializedData = JsonConvert.SerializeObject(transaction);
            // Encode the serialized  string 
            var utf8Data = Encoding.UTF8.GetBytes(serializedData);
            // Publish message to RabbitMQ
            channel.BasicPublish("", QueueName, null, utf8Data);

            // 2. Return a message to the browser
           
            return Ok("Successfully Deposited!");
        }






        [HttpPost]
        public IActionResult Withdraw([FromBody] DepositDto transaction)
        {
            // 1. Publish the data to RabbitMQ (in a message)
            // ==============================================
            // Create connection to RabbitMQ
            string QueueName = "WithdrawCurrency";
            var factory = new ConnectionFactory
            {
                // The docker-compose.yml file defined port 5672 to be used
                // to connect to RabbitMQ
                Uri = new Uri("amqp://guest:guest@localhost:5672")
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(
                QueueName,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Serialize the cartItem object into a text string
            var serializedData = JsonConvert.SerializeObject(transaction);
            // Encode the serialized text string into a sequence of Unicode-8 bytes
            var utf8Data = Encoding.UTF8.GetBytes(serializedData);
            // Publish message to RabbitMQ
            channel.BasicPublish("", QueueName, null, utf8Data);

            // 2. Return a message to the client (browser/postman)
            // ===================================================
            return Ok("Successfully Withdrawn!");
        }







    }
}
