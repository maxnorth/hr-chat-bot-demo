using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;

namespace HrBot.Extensions {

    public static class TelemetryExtensions {

        public static void TrackQnAMakerEvent(this IBotTelemetryClient telemetryClient, string message, QueryResult[] qnaResults) 
        {
            var topAnswer = qnaResults.FirstOrDefault();
            telemetryClient.TrackEvent("QnAMakerResult", new Dictionary<string, string> {
                ["MessageContent"] = message,
                ["TopAnswer"] = topAnswer?.Answer,
                ["TopAnswerScore"] = topAnswer?.Score.ToString(),
                ["TopAnswerId"] = topAnswer?.Metadata.SingleOrDefault(x => x.Name == "id")?.Value
            });
        }
        
        public static void TrackLuisEvent(this IBotTelemetryClient telemetryClient, string message, RecognizerResult luisResult) 
        {
            var (topIntent, topIntentScore) = luisResult.GetTopScoringIntent();
            telemetryClient.TrackEvent("LuisResult", new Dictionary<string, string> {
                ["MessageContent"] = message,
                ["TopIntent"] = topIntent,
                ["TopIntentScore"] = topIntentScore.ToString()
            });
        }
                
        public static void TrackUnassistedMessageEvent(this IBotTelemetryClient telemetryClient, string message) 
        {
            telemetryClient.TrackEvent("UnassistedMessage", new Dictionary<string, string> {
                ["MessageContent"] = message
            });
        }
    }
}