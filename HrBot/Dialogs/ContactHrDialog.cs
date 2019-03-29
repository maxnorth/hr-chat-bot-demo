using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace HrBot.Dialogs {

    public class ContactHrDialog : WaterfallDialog {
        
        public static readonly new string Id = nameof(ContactHrDialog);

        public ContactHrDialog() : base(Id) {

            new List<WaterfallStep> {
                (stepContext, cancellationToken) => {
                    return stepContext.PromptAsync(
                        GenericPrompts.ConfirmPrompt.Id,
                        new PromptOptions {
                            Prompt = MessageFactory.Text("Sorry, I don't know how to help with that. Would you like me to reach out to the HR team on your behalf?"),
                            RetryPrompt = MessageFactory.Text("Sorry, I didn't understand that. Would you like me to send the HR team a message for you?"),
                            Choices = new List<Choice> { new Choice("Yes"), new Choice("No") }
                        },
                        cancellationToken);
                },
                async (stepContext, cancellationToken) => {
                    if ((bool)stepContext.Result) {
                        return await stepContext.PromptAsync(
                            GenericPrompts.TextPrompt.Id,
                            new PromptOptions {
                                Prompt = MessageFactory.Text("My pleasure. Please enter your message for the HR team. I'll pass it along ask them to contact you directly.")
                            },
                            cancellationToken);
                    }
                    else {
                        await stepContext.Context.SendActivityAsync("No problem. I've made a note of your difficulty so my developers can know how to make me more useful in the future.");
                        return await stepContext.EndDialogAsync();
                    }
                },
                async (stepContext, cancellationToken) => {
                     // if the user did provide a message for the HR team, send them an email                     
                    await stepContext.Context.SendActivityAsync("Thanks! I'll make sure the team gets in touch soon.");
                    return await stepContext.EndDialogAsync();
                 }
            }
            .ForEach(step => AddStep(step));
        }
    }
}