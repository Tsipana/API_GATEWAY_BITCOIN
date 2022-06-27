using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wallet.Database;
using Wallet.Domain.Order.Handlers;
using Wallet.Events.Publishers;
using Wallet.Events.Subscribers;
using Wallet.Events;

namespace Wallet
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        //  add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddDbContext<WalletDbContext>();

            services.AddTransient<IOrderCreateHandler, OrderCreateHandler>();
            services.AddTransient<IOrderPlacedPublisher, WalletPublisher>();
            services.AddHostedService<ItemCreatedEventSubscriber>();
            services.AddHostedService<DepositProcessor>();
            services.AddHostedService<WithdrawProcessor>();

        }

        //method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
