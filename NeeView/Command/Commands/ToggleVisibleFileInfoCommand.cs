﻿using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleFileInfoCommand : CommandElement
    {
        public ToggleVisibleFileInfoCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Panel");
            this.ShortCutKey = new ShortcutKey("I");
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisibleFileInfo)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return SidePanelFrame.Current.IsVisibleFileInfo ? Properties.TextResources.GetString("ToggleVisibleFileInfoCommand.Off") : Properties.TextResources.GetString("ToggleVisibleFileInfoCommand.On");
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                SidePanelFrame.Current.SetVisibleFileInfo(Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture), true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisibleFileInfo(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
