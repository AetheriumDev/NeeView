﻿using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleBookshelfCommand : CommandElement
    {
        public ToggleVisibleBookshelfCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Panel");
            this.ShortCutKey = "B";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisibleFolderList)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return SidePanelFrame.Current.IsVisibleFolderList ? Properties.TextResources.GetString("ToggleVisibleBookshelfCommand.Off") : Properties.TextResources.GetString("ToggleVisibleBookshelfCommand.On");
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                SidePanelFrame.Current.SetVisibleFolderList(Convert.ToBoolean(e.Args[0]), true, true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisibleFolderList(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
