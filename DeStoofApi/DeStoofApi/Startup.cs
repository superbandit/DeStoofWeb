using Core.Serializers;
using DeStoofBot;
using DeStoofBot.CustomCommands;
using DeStoofBot.DiscordCommands;
using DeStoofBot.DiscordCommands.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Identity;
using Twitch;
using TwitchLib.Client;
using TwitchLib.Client.Interfaces;

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
            services.AddSingleton<DiscordEventHandler>();
            services.AddSingleton<DiscordManager>();
            services.AddSingleton<CommandService>();
            services.AddScoped<DiscordCommandHandler>();
            services.AddScoped<DiscordGuildEventHandler>();

            services.AddSingleton<ITwitchClient, TwitchClient>();
            services.AddSingleton<TwitchManager>();

            services.AddSingleton<MessageService>();
            services.AddScoped<LoggingService>();
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

            services.AddMvc().AddJsonOptions(o =>
            {
                o.SerializerSettings.Converters.Add(new SnowflakeConverter());
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();

            app.UseCors("CorsPolicy");            

            app.UseMvc();

            app.ApplicationServices.GetRequiredService<DiscordManager>().RunBotAsync().GetAwaiter().GetResult();
            app.ApplicationServices.GetRequiredService<CommandService>().AddModulesAsync(typeof(HelpCommands).Assembly, app.ApplicationServices).GetAwaiter().GetResult();

            // Have classes listening to events.
            app.ApplicationServices.GetRequiredService<TwitchManager>().Start();
            app.ApplicationServices.GetRequiredService<DiscordEventHandler>();
            app.ApplicationServices.GetRequiredService<MessageService>();
        }
    }
}
