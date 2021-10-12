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
        //protected readonly IWebHost _webHost;
        protected IConfiguration _configuration => GetService<IConfiguration>();
        protected IWebHostEnvironment _webHostEnvironment => GetService<IWebHostEnvironment>();
        private readonly WebApplicationFactory<Startup> factory;

        public DependencyResolverHelper()
        {
            //_webHost = WebHost.CreateDefaultBuilder()
            //     .UseStartup<Startup>()
            //     .Build();

            factory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
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
            using (var serviceScope = factory.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                try
                {
                    var scopedService = services.GetRequiredService<T>();
                    return scopedService;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            };
        }
    }
}

