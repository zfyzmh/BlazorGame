using Gobang.GameHub;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;

namespace Gobang
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddSignalR()
                .AddNewtonsoftJsonProtocol();
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();

            builder.Services.AddMudServices();
            builder.WebHost.UseUrls("http://*:5005");

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");
            app.MapHub<BlazorChatSampleHub>(BlazorChatSampleHub.HubUrl);
            app.MapHub<GoBangHub>(GoBangHub.HubUrl);

            app.Run();
        }
    }
}