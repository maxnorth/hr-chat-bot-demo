using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

public class MyWaterfallDialog : WaterfallDialog {

    public static readonly new string Id = "MyWaterfallDialog";

    public MyWaterfallDialog() : base(MyWaterfallDialog.Id) {

        new List<WaterfallStep> {
            (stepContext, cancellationToken) => {
                return stepContext.PromptAsync(
                    "AskTextValue", 
                    new PromptOptions { 
                        Prompt = MessageFactory.Text("Hey, what's your name?")
                    }, 
                    cancellationToken);
            },
            (stepContext, cancellationToken) => {     
                return stepContext.PromptAsync(
                    "AskChoice", 
                    new PromptOptions { 
                        Prompt = MessageFactory.Text($"What's your favorite color, {stepContext.Result}?"),
                        RetryPrompt = MessageFactory.Text("Haha. No, really?"),
                        Choices = new List<Choice> { 
                            new Choice("Blue"), 
                            new Choice("Orange"), 
                            new Choice("Green"),
                            new Choice("Chartreuse")
                        }
                    },
                    cancellationToken);
            },
            async (stepContext, cancellationToken) => {
                var result = stepContext.Result as FoundChoice;
                if (result.Value == "Chartreuse") {
                    await stepContext.Context.SendActivityAsync($"This conversation is over.");
                    return await stepContext.EndDialogAsync();
                }
                return await stepContext.PromptAsync(
                    "AskIntValue", 
                    new PromptOptions { 
                        Prompt = MessageFactory.Text($"{result.Value}, huh. Interesting choice. How old are you?"),
                        RetryPrompt = MessageFactory.Text("That's not a number. I know numbers. I have the best numbers.")
                    },
                    cancellationToken);
            },
            async(stepContext, cancellationToken) => {
                await stepContext.Context.SendActivityAsync($"{stepContext.Result}, imagine that. I'll never forget you.");
                await Task.Delay(2500);
                return await stepContext.ReplaceDialogAsync(MyWaterfallDialog.Id);
            }
        }
        .ForEach(step => AddStep(step));
    }
}