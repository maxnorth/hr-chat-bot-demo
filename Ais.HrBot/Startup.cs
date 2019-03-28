using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ais.HrBot
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var botConfig = BotConfiguration.LoadFromFolder(".");
            services.AddSingleton(botConfig);

            IStorage storage = new MemoryStorage(); // default behavior, same as not registering any conversation state
            //IStorage storage = new AzureBlobStorage("UseDevelopmentStorage=true", "demo-bot-state");
            // IStorage storage = new CosmosDbStorage(new CosmosDbStorageOptions { 
            //     CosmosDBEndpoint = new Uri("https://localhost:8081"),
            //     AuthKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
            //     DatabaseId = "DemoBotStorageDB",
            //     CollectionId = "DemoBotStorage"
            // });

            var state = new ConversationState(storage);
            services.AddSingleton(state);

            services.AddBot<MyBot>(opt => {
                opt.Middleware.Add(new AutoSaveStateMiddleware(state));
                opt.Middleware.Add(new MyBotMiddleware());
            });

            var luisService = botConfig.Services.First(x => x.Type == "luis") as LuisService;
            services.AddSingleton(new LuisRecognizer(new LuisApplication(luisService)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {            
            app.UseBotFramework();        
        }
    }
}
