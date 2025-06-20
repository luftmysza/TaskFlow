using TaskFlow.Infrastructure.Config;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Authorization;

namespace TaskFlow.Admin
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddInfrastructure(builder.Configuration, "Razor");
            builder.Services.AddPolicies();

            builder.Services.AddRazorPages();

            var app = builder.Build();
            
            await app.SeedDataAsync();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
