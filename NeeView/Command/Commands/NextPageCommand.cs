﻿namespace NeeView
{
    public class NextPageCommand : CommandElement
    {
        public NextPageCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Move;
            this.ShortCutKey = "Left,LeftClick";
            this.TouchGesture = "TouchL1,TouchL2";
            this.MouseGesture = "L";
            this.IsShowMessage = false;
            this.PairPartner = "PrevPage";

            // PrevPage
            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Control.MoveNext(this);
        }
    }
}
