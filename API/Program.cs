
using API.Data;
using API.Entities;
using API.Extensions;
using API.Middleware;
using API.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
//service container

builder.Services.AddControllers();
builder.Services.AddCors();
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSignalR();

// middleware

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
            
app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(policy => policy
.AllowAnyHeader()
.AllowAnyMethod()
.AllowCredentials()
.WithOrigins("https://localhost:4200"));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");


using var scope = app.Services.CreateScope();
var service = scope.ServiceProvider;
try{
    var context = service.GetRequiredService<DataContext>();
    var userManager = service.GetRequiredService<UserManager<AppUser>>();
    
    var roleManager = service.GetRequiredService<RoleManager<AppRole>>();
    await context.Database.MigrateAsync();
    await context.Database.ExecuteSqlRawAsync("DELETE FROM [Connections]");
    await Seed.SeedUsers(userManager, roleManager);
}catch(Exception ex){
    var logger = service.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex.InnerException.Message, "Error occured during migration");
}
app.Run();


//.net6
// namespace API
// {
//     public class Program
//     {
//         public static async Task Main(string[] args)
//         {
//            var host =  CreateHostBuilder(args).Build();
//            using var scope = host.Services.CreateScope();
//            var service = scope.ServiceProvider;
//            try{
//                 var context = service.GetRequiredService<DataContext>();
//                 var userManager = service.GetRequiredService<UserManager<AppUser>>();
                
//                 var roleManager = service.GetRequiredService<RoleManager<AppRole>>();
//                 await context.Database.MigrateAsync();
//                 await context.Database.ExecuteSqlRawAsync("DELETE FROM [Connections]");
//                 await Seed.SeedUsers(userManager, roleManager);
//            }catch(Exception ex){
//                 var logger = service.GetRequiredService<ILogger<Program>>();
//                 logger.LogError(ex.InnerException.Message, "Error occured during migration");
//            }

//            await host.RunAsync();
//         }

//         public static IHostBuilder CreateHostBuilder(string[] args) =>
//             Host.CreateDefaultBuilder(args)
//                 .ConfigureWebHostDefaults(webBuilder =>
//                 {
//                     webBuilder.UseStartup<Startup>();
//                 });
//     }
// }
