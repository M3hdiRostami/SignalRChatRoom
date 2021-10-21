using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SignalR;
using System;
using WebApplication.Services;
using WebApplication.Services.Options;

namespace XUnitTest.helpers
{
    public class DependencyResolverHelper
    {

        protected IConfiguration Configuration => GetService<IConfiguration>();
        protected IWebHostEnvironment WebHostEnvironment => GetService<IWebHostEnvironment>();
        private readonly WebApplicationFactory<Startup> _webApplicationFactory;

        public DependencyResolverHelper()
        {
           

            _webApplicationFactory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.Configure<RedisDatabaseSettings>(opts =>
                    {
                        opts.ChatCollectionName = "test";
                        opts.DatabaseId = 99;
                    })
                    .Configure<MongoDBSettings>(opts =>
                    {
                        opts.ChatCollectionName = "test";
                        opts.DatabaseName = "Test";
                    });
                });
            });
        }

        public T GetService<T>()
        {
            using (var serviceScope = _webApplicationFactory.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                try
                {
                    var scopedService = services.GetRequiredService<T>();
                    return scopedService;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            };
        }
    }
}

