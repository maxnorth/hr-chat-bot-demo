using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HrBot.Dialogs {

    public class ChangePhoneNumberDialog : WaterfallDialog {
        
        public static readonly new string Id = nameof(ChangePhoneNumberDialog);

        public ChangePhoneNumberDialog() : base(ChangePhoneNumberDialog.Id) {

            new List<WaterfallStep> {
                (stepContext, cancellationToken) => {
                    return stepContext.BeginDialogAsync(
                        PhoneNumberPrompt.Id, 
                        "I'd be happy to help you change your phone number. Please enter your new number:", 
                        cancellationToken);
                },
                (stepContext, cancellationToken) => {
                    stepContext.Context.SendActivityAsync($"Got it. Your phone number has been updated to {stepContext.Result}");
                    // do work of saving data
                    return stepContext.EndDialogAsync();
                }
            }
            .ForEach(step => AddStep(step));
        }
    }
}