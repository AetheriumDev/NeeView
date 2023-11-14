﻿namespace NeeView
{
    public class ViewBaseScaleDownCommand : CommandElement
    {
        public ViewBaseScaleDownCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;
            this.ParameterSource = new CommandParameterSource(new ViewScaleCommandParameter());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewTransformControl.ScaleDown(ScaleType.BaseScale, e.Parameter.Cast<ViewScaleCommandParameter>());
        }
    }

}
