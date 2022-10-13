using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
           var host =  CreateHostBuilder(args).Build();
           using var scope = host.Services.CreateScope();
           var service = scope.ServiceProvider;
           try{
                var context = service.GetRequiredService<DataContext>();
                var userManager = service.GetRequiredService<UserManager<AppUser>>();
                await context.Database.MigrateAsync();
                var roleManager = service.GetRequiredService<RoleManager<AppRole>>();
                await Seed.SeedUsers(userManager, roleManager);
           }catch(Exception ex){
                var logger = service.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex.InnerException.Message, "Error occured during migration");
           }

           await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
