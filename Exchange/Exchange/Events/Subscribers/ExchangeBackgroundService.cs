using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using System.Collections.Generic;
using Exchange.Events.Publishers;

namespace Exchange.Events.Subscribers
{
    public class ExchangeBackgroundService : BackgroundService
    {
        private readonly IPriceMovedPublisher   _priceMovedPublisher;
        public float bitcoin_previous_price { get; set; } = 0;
        public float ethereum_previous_price { get; set; } = 0;
        public float cardano_previous_price { get; set; } = 0;

        // Constructor:
        // Inject PriceMovedPublisher (defined in Startup.cs to be injected whenever a function-parameter is defined as IPriceMovedPublisher
        public ExchangeBackgroundService(IPriceMovedPublisher parmPriceMovedPublisher)
        {
            _priceMovedPublisher = parmPriceMovedPublisher;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            
            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            // Call ContactExchange() every 5 seconds
            var timer = new Timer(ContactExchange, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        private async void ContactExchange(object state)
        {
            using (var client = new HttpClient())
            {
                // Get the response from the exchange
                HttpResponseMessage response = await client.GetAsync("https://www.bitmex.com/api/v1/trade/bucketed?binSize=1m&partial=true&count=100&reverse=true");

                if (response.IsSuccessStatusCode)
                {
                    // Convert json string response to a List of SymbolDetailsDto objects
                    var cryptoData = JsonConvert.DeserializeObject<List<SymbolDetailsDto>>(await response.Content.ReadAsStringAsync());

                    // Loop through all crypto objects
                    foreach (SymbolDetailsDto symbolDetail in cryptoData)
                    {
                        
                        
                        if ((symbolDetail.symbol == ".XBT") && (symbolDetail.close.HasValue))
                        {

                            
                            if ((symbolDetail.close > bitcoin_previous_price) && (bitcoin_previous_price > 0))
                            {
                                // Send a message to the queue containing the word "up"
                                PriceMovedEventData data = new PriceMovedEventData();
                                data.movement = "up";
                                data.symbol = ".XBT";
                                data.Qty = (float)(symbolDetail.close - bitcoin_previous_price);
                                _priceMovedPublisher.Publish(data);

                                
                                Debug.WriteLine("Bitcoin: Up {0} > {1} Difference = {2}", bitcoin_previous_price, symbolDetail.close, data.Qty);
                            }

                            // Test if the current bitcoin price < previous bitcoin price
                            // Skip the first round where the previous price still has the default value of 0
                            if ((symbolDetail.close < bitcoin_previous_price) && (bitcoin_previous_price > 0))
                            {
                                // Send a message to the queue containing the word "down"
                                PriceMovedEventData data = new PriceMovedEventData();
                                data.movement = "down";
                                data.symbol = ".XBT";
                                data.Qty = (float)(symbolDetail.close - bitcoin_previous_price);
                                _priceMovedPublisher.Publish(data);

                               
                                Debug.WriteLine("Bitcoin: Down {0} > {1} Difference = {2}", bitcoin_previous_price, symbolDetail.close, data.Qty);
                            }

                            
                            bitcoin_previous_price = (float)symbolDetail.close;
                        }
                    
                        
                        if ((symbolDetail.symbol == ".ETHBON") && (symbolDetail.close.HasValue))
                        {

                           
                            if ((symbolDetail.close > ethereum_previous_price) && (ethereum_previous_price > 0))
                            {
                                // Send a message to the queue containing the word "up"
                                PriceMovedEventData data = new PriceMovedEventData();
                                data.movement = "up";
                                data.symbol = ".ETHBON";
                                data.Qty = (float)(symbolDetail.close - ethereum_previous_price);
                                _priceMovedPublisher.Publish(data);

                                Debug.WriteLine("Ethereum: Up {0} > {1} Difference = {2}", ethereum_previous_price, symbolDetail.close, data.Qty);
                            }

                            
                            if ((symbolDetail.close < ethereum_previous_price) && (ethereum_previous_price > 0))
                            {
                                
                                PriceMovedEventData data = new PriceMovedEventData();
                                data.movement = "down";
                                data.symbol = ".ETHBON";
                                data.Qty = (float)(symbolDetail.close - ethereum_previous_price);
                                _priceMovedPublisher.Publish(data);

                                Debug.WriteLine("Ethereum: Down {0} > {1} Difference = {2}", ethereum_previous_price, symbolDetail.close, data.Qty);
                            }

                             
                            ethereum_previous_price = (float)symbolDetail.close;
                        }

                        
                        if ((symbolDetail.symbol == ".BTHETAT") && (symbolDetail.close.HasValue))
                        {

                            
                            if ((symbolDetail.close > cardano_previous_price) && (cardano_previous_price > 0))
                            {
                                // Send a message to the queue containing the word "up"
                                PriceMovedEventData data = new PriceMovedEventData();
                                data.movement = "up";
                                data.symbol = ".BTHETAT";
                                data.Qty = (float)(symbolDetail.close - cardano_previous_price);
                                _priceMovedPublisher.Publish(data);

                                Debug.WriteLine("Cardano: Up {0} > {1} Difference = {2}", cardano_previous_price, symbolDetail.close, data.Qty);
                            }

                            
                            if ((symbolDetail.close < cardano_previous_price) && (cardano_previous_price > 0))
                            {
                                // Send a message to the queue containing the word "down"
                                PriceMovedEventData data = new PriceMovedEventData();
                                data.movement = "down";
                                data.symbol = ".BTHETAT";
                                data.Qty = (float)(symbolDetail.close - cardano_previous_price);
                                _priceMovedPublisher.Publish(data);

                                Debug.WriteLine("Cardano: Down {0} > {1} Difference = {2}", cardano_previous_price, symbolDetail.close, data.Qty);
                            }

                            
                            cardano_previous_price = (float)symbolDetail.close;
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("Invalid Response"); 
                }
            }
        }

        public class SymbolDetailsDto
        {
            public string symbol { get; set; }
            public float? close { get; set; }
        }
    }
}
