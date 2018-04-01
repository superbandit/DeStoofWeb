﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Sockets;
using DeStoofApi.Controllers;
using DeStoofApi.Services;
using DeStoofApi.Chatsources;
using MongoDB.Driver;

namespace DeStoofApi
{
    public class Startup
    {
        private string mongoDBConnection = "mongodb+srv://user-1:redacted@destoofbot-cdr2z.mongodb.net/test";      

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

            var mongoDB = new MongoClient(mongoDBConnection);
            IMongoDatabase database = mongoDB.GetDatabase("DeStoofBot");

            services.AddSingleton(database);

            services.AddSingleton<IrcManager>();

            services.AddSignalR();
            services.AddScoped<ChatController>();

            services.AddScoped<MessageService>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");            

            app.UseSignalR(routes => {
                routes
                .MapHub<ChatHub>("/chat");                
            });

            app.UseMvc();
        }
    }
}
