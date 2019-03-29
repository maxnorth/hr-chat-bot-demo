using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using HrBot.Dialogs;
using Microsoft.Bot.Builder.AI.QnA;

namespace HrBot
{
    public class MyBot : IBot
    {
        private readonly ConversationState _conversationState;
        private readonly DialogSet _dialogSet;
        private readonly LuisRecognizer _luisRecognizer;
        private readonly QnAMaker _qnaMaker;
        private readonly IBotTelemetryClient _telemetryClient;

        public MyBot(
            ConversationState conversationState, 
            LuisRecognizer luisRecognizer, 
            QnAMaker qnaMaker,
            IBotTelemetryClient telemetryClient)
        {
            _conversationState = conversationState;
            _luisRecognizer = luisRecognizer;
            _qnaMaker = qnaMaker;
            _telemetryClient = telemetryClient;

            _dialogSet = new DialogSet(conversationState.CreateProperty<DialogState>("MyDialogState"));            
            _dialogSet.Add(new MyWaterfallDialog());
            _dialogSet.Add(new TextPrompt("AskTextValue"));
            _dialogSet.Add(new NumberPrompt<int>("AskIntValue"));
            _dialogSet.Add(new ChoicePrompt("AskChoice"));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var qnaResults = await _qnaMaker.GetAnswersAsync(turnContext);

                _telemetryClient.TrackQnAMakerEvent(turnContext.Activity.Text, qnaResults);

                if (qnaResults.Any()) {
                    await turnContext.SendActivityAsync(qnaResults.First().Answer);
                    return;
                }

                var luisResult = await _luisRecognizer.RecognizeAsync(turnContext, cancellationToken);
                
                _telemetryClient.TrackLuisEvent(turnContext.Activity.Text, luisResult);

                var (intent, intentScore) = luisResult.GetTopScoringIntent();

                if (intentScore > .70)
                {
                    if (intent == "Help")
                    {
                        await turnContext.SendActivityAsync("Sorry, I'm not very helpful.");
                        return;
                    }

                    if (intent == "Rate")
                    {
                        await turnContext.SendActivityAsync(";)");
                        return;
                    }
                }

                var dialogContext = await _dialogSet.CreateContextAsync(turnContext, cancellationToken);

                var dialogTurnResult = await dialogContext.ContinueDialogAsync(cancellationToken);

                if (dialogTurnResult.Status == DialogTurnStatus.Empty)
                {
                    await dialogContext.BeginDialogAsync(MyWaterfallDialog.Id, cancellationToken);
                }
            }
        }
    }
}