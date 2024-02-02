﻿namespace NeeView
{
    public class ViewResetCommand : CommandElement
    {
        public ViewResetCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewTransformControl.ResetContentSizeAndTransform();
        }
    }
}
