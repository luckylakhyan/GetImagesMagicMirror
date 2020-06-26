using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace GetImage
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var tservice = services.Where(w => w.ServiceType.FullName == "Microsoft.AspNetCore.Hosting.IWebHostEnvironment").FirstOrDefault();
            IWebHostEnvironment _webHostEnvironment = null;
            if (tservice != null)
            {
                _webHostEnvironment = (IWebHostEnvironment)tservice.ImplementationInstance;
            }
            Action<GetMirrorData> imageData = (opt =>
            {
                opt.SetupTimeStamp = DateTime.Now;
                opt.ApplicationName = _webHostEnvironment != null ? _webHostEnvironment.ApplicationName : "";
                opt.ContentRootPath = _webHostEnvironment != null ? _webHostEnvironment.ContentRootPath : "";
                opt.EnvironmentName = _webHostEnvironment != null ? _webHostEnvironment.EnvironmentName : "";
                opt.WebRootPath = _webHostEnvironment != null ? _webHostEnvironment.WebRootPath : "";
                opt.imageRootPath = Configuration["imagefilefolder"];
                opt.ImageRefreshInterval = int.Parse(Configuration["imagerefreshminutes"]);
               
                opt.ImageNextRefresh = DateTime.Now.AddMinutes(-100);

                opt.GetImagesList().GetAwaiter().GetResult();


            });

            services.Configure(imageData);
            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
            builder =>
            {
                builder.WithOrigins(Configuration["hosturl1"],
                                    Configuration["hosturl2"]);
            });
            });
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<GetMirrorData>>().Value);
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddControllersWithViews();
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors(MyAllowSpecificOrigins);
            app.UseAuthorization();
          
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
