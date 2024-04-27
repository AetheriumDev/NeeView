﻿namespace NeeView
{
    public class FirstPageCommand : CommandElement
    {
        public FirstPageCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Move");
            this.ShortCutKey = new ShortcutKey("Ctrl+Right");
            this.MouseGesture = "UR";
            this.IsShowMessage = true;
            this.PairPartner = "LastPage";

            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Control.MoveToFirst(this);
        }
    }
}
