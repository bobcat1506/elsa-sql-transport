using Elsa;
using Elsa.Caching.Rebus.Extensions;
using Elsa.Persistence.EntityFramework.Core.Extensions;
using Elsa.Services;
using ElsaSqlTransport.Workflow;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElsaSqlTransport
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var sqlConnnectionString = Configuration.GetConnectionString("Sql") ?? throw new InvalidOperationException();

            services.AddElsa(elsa =>
            {
                elsa.AddWorkflowsFrom<FirstWorkflow>();
                elsa.AddActivitiesFrom<Step1>();

                // configure for distributed hosting
                // https://elsa-workflows.github.io/elsa-core/docs/hosting/hosting-distributed-hosting

                // elsa persistence
                elsa.UseEntityFrameworkPersistence(ef =>
                {
                    global::Elsa.Persistence.EntityFramework.SqlServer.DbContextOptionsBuilderExtensions.UseSqlServer(ef, sqlConnnectionString);
                });

                // distributed lock
                elsa.ConfigureDistributedLockProvider(options => options.UseSqlServerLockProvider(sqlConnnectionString));

                // rebus
                elsa.UseServiceBus(bus =>
                {
                    // Thrown when routing is commented out:
                    // System.ArgumentException: 'Cannot get destination for message of type Elsa.Services.ExecuteWorkflowDefinitionRequest 
                    // because it has not been mapped! 

                    // Thrown when routing is uncommented:
                    // System.InvalidOperationException: 'Attempted to register primary -> Rebus.Routing.IRouter, but a primary registration
                    // already exists: primary -> Rebus.Routing.IRouter'

                    bus.Configurer
                        .Transport(transport => transport.UseSqlServer(new SqlServerTransportOptions(sqlConnnectionString), bus.QueueName));
                        //.Routing(route =>
                        //{
                        //    route.TypeBased().Map<ExecuteWorkflowDefinitionRequest>("execute-workflow");
                        //});
                });

                // Distributed Cache Signal Provider
                elsa.UseRebusCacheSignal();
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ElsaSqlTransport", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ElsaSqlTransport v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
