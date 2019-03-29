using Microsoft.Bot.Builder.Dialogs;

namespace HrBot.Dialogs {

    public static class GenericPrompts {
        
        public static readonly TextPrompt TextPrompt = new TextPrompt("HrBot-TextPrompt");
        public static readonly TextPrompt NumberPrompt = new TextPrompt("HrBot-NumberPrompt");
        public static readonly TextPrompt ChoicePrompt = new TextPrompt("HrBot-ChoicePrompt");
    }
}