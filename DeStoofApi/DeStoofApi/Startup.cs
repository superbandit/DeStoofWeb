using DeStoofApi.Chatsources.Discord;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using DeStoofApi.Services;
using DeStoofApi.Chatsources.Twitch;
using Discord.Commands;
using Discord.WebSocket;
using Models.Domain.Users;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Identity;
using TwitchLib.Client;

namespace DeStoofApi
{
    public class Startup
    {  
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy("CorsPolicy", builder => {
                builder
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithOrigins("http://localhost:4200");
            }));

            var documentStore = new DocumentStore
            {
                Urls = new []{Configuration["Secure:DataBaseUrl"]},
                Database = Configuration["Secure:DataBaseName"],
                Conventions = new DocumentConventions
                {
                    IdentityPartsSeparator = "/"
                }
            };
            documentStore.Initialize();

            services.AddSingleton<IDocumentStore>(documentStore);
            services.AddScoped(s => documentStore.OpenAsyncSession());

            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig {AlwaysDownloadUsers = true}));
            services.AddSingleton<DiscordManager>();
            services.AddSingleton<DiscordCommandHandler>();

            services.AddSingleton<CommandService>();

            services.AddSingleton<TwitchClient>();
            services.AddSingleton<TwitchManager>();
            services.AddSingleton<TwitchCommandHandler>();

            services.AddScoped<LoggingService>();
            services.AddScoped<MessageService>();
            services.AddScoped<CustomCommandService>();

            services.AddRavenDbIdentity<ApplicationUser>();
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            });

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseCors("CorsPolicy");            

            app.UseMvc();

            app.ApplicationServices.GetRequiredService<TwitchManager>().Start();
            app.ApplicationServices.GetRequiredService<DiscordManager>().RunBotAsync().Wait();
        }
    }
}
