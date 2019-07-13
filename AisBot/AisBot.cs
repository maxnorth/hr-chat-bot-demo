using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.AI.QnA;
using AisBot.Extensions;

namespace AisBot
{
    public class AisBot : IBot
    {
        private readonly QnAMaker _qnaMaker;
        private readonly IBotTelemetryClient _telemetryClient;

        public AisBot(
            QnAMaker qnaMaker,
            IBotTelemetryClient telemetryClient)
        {
            _qnaMaker = qnaMaker;
            _telemetryClient = telemetryClient;        
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {    
                // if no intent recognized, see if message matches QnA supported inquiry
                var qnaResults = await _qnaMaker.GetAnswersAsync(turnContext);

                _telemetryClient.TrackQnAMakerEvent(turnContext.Activity.Text, qnaResults);

                if (qnaResults.Any()) {
                    await turnContext.SendActivityAsync(qnaResults.First().Answer);
                    return;
                }

                return;
            }
        }
    }
}