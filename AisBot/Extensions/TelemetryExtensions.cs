using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;

namespace AisBot.Extensions {

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
    }
}