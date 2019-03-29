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
    public class HrBot : IBot
    {
        private readonly ConversationState _conversationState;
        private readonly DialogSet _dialogSet;
        private readonly LuisRecognizer _luisRecognizer;
        private readonly QnAMaker _qnaMaker;
        private readonly IBotTelemetryClient _telemetryClient;

        public HrBot(
            ConversationState conversationState, 
            LuisRecognizer luisRecognizer, 
            QnAMaker qnaMaker,
            IBotTelemetryClient telemetryClient)
        {
            _conversationState = conversationState;
            _luisRecognizer = luisRecognizer;
            _qnaMaker = qnaMaker;
            _telemetryClient = telemetryClient;

            _dialogSet = new DialogSet(conversationState.CreateProperty<DialogState>(nameof(ChangePhoneNumberDialog)));            
            _dialogSet.Add(new ChangePhoneNumberDialog());
            _dialogSet.Add(new PhoneNumberPrompt());
            _dialogSet.Add(GenericPrompts.TextPrompt);
            _dialogSet.Add(GenericPrompts.NumberPrompt);
            _dialogSet.Add(GenericPrompts.ChoicePrompt);
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {        
                var dialogContext = await _dialogSet.CreateContextAsync(turnContext, cancellationToken);

                var dialogTurnResult = await dialogContext.ContinueDialogAsync(cancellationToken);

                if (dialogTurnResult.Status == DialogTurnStatus.Waiting)
                {
                    // actively engaged in dialog, wait for response
                    return;
                }

                var luisResult = await _luisRecognizer.RecognizeAsync(turnContext, cancellationToken);
                
                _telemetryClient.TrackLuisEvent(turnContext.Activity.Text, luisResult);

                var (intent, intentScore) = luisResult.GetTopScoringIntent();

                if (intentScore > .60)
                {
                    if (intent == LuisIntentConstants.ChangePhoneNumber)
                    {
                        await dialogContext.BeginDialogAsync(ChangePhoneNumberDialog.Id, cancellationToken);
                        return;
                    }

                    // try qna

                    // else offer to speak to a human
                }
                
                var qnaResults = await _qnaMaker.GetAnswersAsync(turnContext);

                _telemetryClient.TrackQnAMakerEvent(turnContext.Activity.Text, qnaResults);

                if (qnaResults.Any()) {
                    await turnContext.SendActivityAsync(qnaResults.First().Answer);
                    return;
                }
            }
        }
    }
}