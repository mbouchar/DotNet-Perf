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

namespace RestService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
//                .SetBasePath(hostingEnv.ContentRootPath)
//                .AddCommandLine(Program.Args)
                .AddJsonFile("appsettings.json")
//                .AddJsonFile($"appsettings.{hostingEnv.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddMvc();
            services
                .AddMvcCore()
                .AddJsonFormatters()
                //.AddResponseCaching()
                .AddCors();
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
