using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

public class MyBot : IBot {

    private readonly ConversationState _conversationState;
    private readonly DialogSet _dialogSet;
    private readonly LuisRecognizer _luisRecognizer;

    public MyBot(ConversationState conversationState, LuisRecognizer luisRecognizer) {
        _conversationState = conversationState;
        _dialogSet = new DialogSet(conversationState.CreateProperty<DialogState>("MyDialogState"));
        _luisRecognizer = luisRecognizer;

        _dialogSet.Add(new MyWaterfallDialog());
        _dialogSet.Add(new TextPrompt("AskTextValue"));
        _dialogSet.Add(new NumberPrompt<int>("AskIntValue"));
        _dialogSet.Add(new ChoicePrompt("AskChoice"));
    }

    public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken)) {
        
        if (turnContext.Activity.Type == ActivityTypes.Message) 
        {
            var luisResult = await _luisRecognizer.RecognizeAsync(turnContext, cancellationToken);
            var intent = luisResult.Intents.First();

            if (intent.Value.Score > .70) 
            {
                if (intent.Key == "Help") {
                    await turnContext.SendActivityAsync("Sorry, I'm not very helpful.");
                    return;
                }

                if (intent.Key == "Rate") {
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