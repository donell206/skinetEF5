using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.Sqlite;
using Core.Interfaces;
using API.Helpers;
using API.Middleware;
using API.Errors;
using API.Extensions;
using StackExchange.Redis;


namespace API
{
    public class Startup
    {

        private readonly IConfiguration _config;
        public Startup(IConfiguration config)
        {
            _config= config;
        }

       
               // public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            
            services.AddAutoMapper(typeof(MappingProfiles));
            services.AddControllers();


            services.AddDbContext<StoreContext>(X=>X.UseSqlite(_config.GetConnectionString("DefaultConnection")));

  
            services.AddSingleton<IConnectionMultiplexer>(c =>
            {
                var configuration = ConfigurationOptions.Parse(_config.GetConnectionString("Redis"), true);
                return ConnectionMultiplexer.Connect(configuration);
            });

            services.AddAplicationServices();
            services.AddSwaggerDocumentation();
            services.AddCors(opt=>
            {
                opt.AddPolicy("CorsPolicy",policy =>
                {
                    policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200");
                });

            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<ExceptionMiddleware>();
      
            app.UseStatusCodePagesWithReExecute("/errors/{0}");


            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseStaticFiles();
            app.UseCors("CorsPolicy");

            app.UseAuthorization();
            app.UseSwaggerDocumention();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
