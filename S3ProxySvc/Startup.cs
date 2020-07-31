using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSwag;

namespace S3ProxySvc
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
            services.AddControllers();

            #region AmazonS3Client

            AmazonS3Config config = new AmazonS3Config();
            config.ServiceURL = Configuration.GetSection("S3")["ServiceURL"];
            Amazon.Runtime.AWSCredentials credentials = new Amazon.Runtime.BasicAWSCredentials(
                        Configuration.GetSection("S3")["accessKey"],
                        Configuration.GetSection("S3")["secretKey"]);
            services.AddTransient<IAmazonS3>(_ =>
                      new AmazonS3Client(credentials, config));
            #endregion

            #region Swagger config

            services.AddOpenApiDocument(document =>
            {
                document.Title = "S3 Proxy Svc API";
                document.Version = "v1";
            });
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwaggerUi3();

            app.UseOpenApi(settings =>
            {
                settings.PostProcess = (document, request) =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "S3 Proxy Svc API";

                    document.Info.License = new OpenApiLicense
                    {
                        Name = "Use under MIT License",
                        Url = "https://github.com/MaxKhlupnov/S3Proxy"
                    };
                };
            });

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
