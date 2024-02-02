﻿using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleThumbnailListCommand : CommandElement
    {
        public ToggleVisibleThumbnailListCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.FilmStrip");
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(FilmStripConfig.IsEnabled)) { Source = Config.Current.FilmStrip };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return ThumbnailList.Current.IsVisible ? Properties.TextResources.GetString("ToggleVisibleThumbnailListCommand.Off") : Properties.TextResources.GetString("ToggleVisibleThumbnailListCommand.On");
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                ThumbnailList.Current.SetVisibleThumbnailList(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                ThumbnailList.Current.ToggleVisibleThumbnailList(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
