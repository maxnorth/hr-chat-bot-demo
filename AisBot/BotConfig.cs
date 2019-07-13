using System;
using Microsoft.Bot.Configuration;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

public class BotConfig : BotConfiguration
{
    [JsonIgnore]
    public CosmosDbService CosmosStorage { get; }

    [JsonIgnore]
    public QnAMakerService QnAMaker { get; }
    
    [JsonIgnore]
    public AppInsightsService AppInsights { get; }

    [JsonIgnore]
    public string Secret;

    public BotConfig(IConfiguration config)
    {
        Padlock = "";
        Name = "AisBot";
        Description = "Generated bot file for emulator reference";

        CosmosStorage = new CosmosDbService();
        config.Bind("botSettings:cosmosDb", CosmosStorage);
        Services.Add(CosmosStorage);

        QnAMaker = new QnAMakerService();
        config.Bind("botSettings:qna", QnAMaker);
        Services.Add(QnAMaker);
        
        AppInsights = new AppInsightsService();
        config.Bind("botSettings:appInsights", AppInsights);
        Services.Add(AppInsights);

        Secret = config["botSettings:secret"];
    }

    public void Save()
    {
        SaveAs("./bin/AisBot.bot", Secret);
    }
}