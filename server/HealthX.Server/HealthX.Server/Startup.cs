using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HealthX.Server.Database;

using HealthX.Server.Models;
using Braintree;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace HealthX.Server
{
    public class Startup {

        public static DatabaseManager database_manager;
        public static PrivateDatabaseManager private_database_manager;

        public static BraintreeGateway payment_gateway;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {

            // Init DatabaseManager
            services.AddMvc();
            database_manager = new DatabaseManager();
            database_manager.Connect();

            private_database_manager = new PrivateDatabaseManager();
            private_database_manager.Connect();

            // INIT BRAINTREE SERVER
            payment_gateway = new BraintreeGateway {

                Environment = Braintree.Environment.SANDBOX,
                MerchantId = "v4r5z482sdwz3g3d",
                PublicKey = "qbb2zxdvfbtt7dbm",
                PrivateKey = "32e0637133160c66cf6a207959dae903"

            };

            BraintreeTools.PaymentMonitoring.Init();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
