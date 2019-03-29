using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HrBot.Dialogs {
    
    public class PhoneNumberPrompt : WaterfallDialog {
        
        public static readonly new string Id = nameof(PhoneNumberPrompt);
        private static readonly Regex PhoneNumberRegex = new Regex(@"[+]*[(]{0,1}[0-9]{1,4}[)]{0,1}[-\s\./0-9]*");

        public PhoneNumberPrompt() : base(Id) {   
            new List<WaterfallStep> {
                (stepContext, cancellationToken) => {
                    var promptText = stepContext.Options as string;
                    if (promptText == null) {
                        throw new ArgumentException($"Argument passed to ${nameof(PhoneNumberPrompt)} must be a string");
                    }

                    if (PhoneNumberRegex.IsMatch(stepContext.Context.Activity.Text)) {
                        return stepContext.NextAsync(stepContext.Context.Activity.Text);
                    }
                    return stepContext.PromptAsync(
                        GenericPrompts.TextPrompt.Id, 
                        new PromptOptions { 
                            Prompt = MessageFactory.Text(promptText)
                        },
                        cancellationToken);
                },
                (stepContext, cancellationToken) => {
                    var regexMatch = PhoneNumberRegex.Match(stepContext.Result as string);
                    if (regexMatch.Success) {
                        return stepContext.EndDialogAsync(regexMatch.Value);
                    }
                    return stepContext.ReplaceDialogAsync(Id, "Sorry, I didn't recognize that as a valid phone number. Please try again:");
                }
            }
            .ForEach(step => AddStep(step));
        }
    }
}