using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApplication.Features.Hubs;
using WebApplication.Services;
using WebApplication.Services.Options;

namespace SignalR
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

            //Add option pattern for databases settings
            services.Configure<MongoDBSettings>(Configuration.GetSection(nameof(MongoDBSettings)));
            services.Configure<RedisDatabaseSettings>(Configuration.GetSection(nameof(RedisDatabaseSettings)));

            services.AddRazorPages().AddRazorRuntimeCompilation();
            services.AddSignalR();

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Configuration.GetSection(nameof(RedisDatabaseSettings))["ConnectionString"];
            });
            //services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect(Configuration.GetSection(nameof(RedisDatabaseSettings))["ConnectionString"]));
            services.AddSingleton<ChatStoreService>();
            services.AddSingleton<IChatStoreService, ChatStoreLoggerService>();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHub<ChatHub>("/chathub");
            });
        }
    }
}
