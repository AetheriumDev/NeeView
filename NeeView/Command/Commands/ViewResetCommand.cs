﻿namespace NeeView
{
    public class ViewResetCommand : CommandElement
    {
        public ViewResetCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.ResetContentSizeAndTransform();
        }
    }
}
