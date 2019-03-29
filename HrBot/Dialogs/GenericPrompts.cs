using Microsoft.Bot.Builder.Dialogs;

namespace HrBot.Dialogs {

    public static class GenericPrompts {
        
        public static readonly TextPrompt TextPrompt = new TextPrompt("HrBot-TextPrompt");
        public static readonly NumberPrompt<int> IntegerPrompt = new NumberPrompt<int>("HrBot-NumberPrompt");
        public static readonly ChoicePrompt ChoicePrompt = new ChoicePrompt("HrBot-ChoicePrompt");
        public static readonly ConfirmPrompt ConfirmPrompt = new ConfirmPrompt("HrBot-ConfirmPrompt");
    }
}