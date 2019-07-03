using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.AI.QnA;
using HrBot.Dialogs;
using HrBot.Extensions;

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

            _dialogSet = new DialogSet(conversationState.CreateProperty<DialogState>("HrBotDialogState"));            
            _dialogSet.Add(new ChangePhoneNumberDialog());
            _dialogSet.Add(new PhoneNumberPrompt());
            _dialogSet.Add(new ContactHrDialog());
            _dialogSet.Add(GenericPrompts.TextPrompt);
            _dialogSet.Add(GenericPrompts.IntegerPrompt);
            _dialogSet.Add(GenericPrompts.ChoicePrompt);
            _dialogSet.Add(GenericPrompts.ConfirmPrompt);
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {        
                // check for active dialog and continue if exists
                var dialogContext = await _dialogSet.CreateContextAsync(turnContext);

                var dialogTurnResult = await dialogContext.ContinueDialogAsync(cancellationToken);

                if (dialogTurnResult.Status == DialogTurnStatus.Waiting || dialogTurnResult.Status == DialogTurnStatus.Complete)
                {
                    // actively engaged in dialog, or last response completed dialog. can return immediately.
                    return;
                }

                // no active dialog. check user's message for specific intent
                var luisResult = await _luisRecognizer.RecognizeAsync(turnContext, cancellationToken);
                
                _telemetryClient.TrackLuisEvent(turnContext.Activity.Text, luisResult);

                var (intent, intentScore) = luisResult.GetTopScoringIntent();

                if (intentScore > .70)
                {
                    if (intent == LuisIntentConstants.ChangePhoneNumber)
                    {
                        await dialogContext.BeginDialogAsync(ChangePhoneNumberDialog.Id, cancellationToken);
                        return;
                    }
                }
                
                // if no intent recognized, see if message matches QnA supported inquiry
                var qnaResults = await _qnaMaker.GetAnswersAsync(turnContext);

                _telemetryClient.TrackQnAMakerEvent(turnContext.Activity.Text, qnaResults);

                if (qnaResults.Any()) {
                    await turnContext.SendActivityAsync(qnaResults.First().Answer);
                    return;
                }

                // else acknowledge failure to recognize message
                _telemetryClient.TrackUnassistedMessageEvent(turnContext.Activity.Text);
                await dialogContext.BeginDialogAsync(ContactHrDialog.Id, cancellationToken);
                return;
            }
        }
    }
}