﻿namespace NeeView
{
    public class NextFolderPageCommand : CommandElement
    {
        public NextFolderPageCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Move;
            this.IsShowMessage = true;
            this.PairPartner = "PrevFolderPage";

            // PrevFolderPage
            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return "";
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.NextFolderPage(this, this.IsShowMessage);
        }
    }
}
