﻿using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleViewFlipVerticalCommand : CommandElement
    {
        public ToggleViewFlipVerticalCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(DragTransform.IsFlipVertical)) { Source = MainViewComponent.Current.DragTransform, Mode = BindingMode.OneWay };
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                MainViewComponent.Current.ViewTransformControl.FlipVertical(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                MainViewComponent.Current.ViewTransformControl.ToggleFlipVertical();
            }
        }
    }
}
