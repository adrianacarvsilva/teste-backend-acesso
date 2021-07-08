using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Processing;
using teste_backend_acesso.Domain.Interfaces;
using teste_backend_acesso.Domain.Services;
using teste_backend_acesso.Repositories;

namespace teste_backend_acesso
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IServiceAccount, ServiceAccount>();
            services.AddSingleton<IRepositoryAccount, RepositoryAccount>();
            services.AddControllers();

           services.AddHostedService<Transactions>();

            services.AddControllers()
               .AddJsonOptions(options => { options.JsonSerializerOptions.IgnoreNullValues = true; });
        }

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
