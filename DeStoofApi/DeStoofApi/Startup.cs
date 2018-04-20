using System;
using DeStoofApi.Chatsources.Discord;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using DeStoofApi.Controllers;
using DeStoofApi.Services;
using DeStoofApi.Chatsources.Twitch;
using DeStoofApi.Hubs;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;

namespace DeStoofApi
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

            services.AddCors(options => options.AddPolicy("CorsPolicy", builder => {
                builder
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithOrigins("http://localhost:4200");
            }));

            var mongoDb = new MongoClient($"{Configuration["Secure:DataBase"]}");
            IMongoDatabase database = mongoDb.GetDatabase("DeStoofBot");
            services.AddSingleton(database);

            services.AddIdentityWithMongoStores($"{Configuration["Secure:DataBase"]}")
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            });

            services.AddSignalR();

            services.AddSingleton<DiscordSocketClient>();
            services.AddSingleton<CommandService>();
            services.AddSingleton<TwitchManager>();
            services.AddSingleton<DiscordManager>();
            services.AddSingleton<MessageService>();
            services.AddSingleton<IServiceProvider>(services.BuildServiceProvider());

            services.AddScoped<ChatController>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }           
            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseCors("CorsPolicy");            

            app.UseSignalR(routes => {
                routes
                .MapHub<ChatHub>("/chat");                
            });

            app.UseMvc();

            app.ApplicationServices.GetService<MessageService>().StartDiscordConnection().GetAwaiter().GetResult();
            app.ApplicationServices.GetService<MessageService>().StartTwitchConnection();
        }
    }
}
