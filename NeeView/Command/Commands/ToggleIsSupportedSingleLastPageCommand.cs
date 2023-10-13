﻿using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsSupportedSingleLastPageCommand : CommandElement
    {
        public ToggleIsSupportedSingleLastPageCommand()
        {
            this.Group = Properties.Resources.CommandGroup_PageSetting;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettings.Current.IsSupportedSingleLastPage));
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return BookSettings.Current.IsSupportedSingleLastPage ? Properties.Resources.ToggleIsSupportedSingleLastPageCommand_Off : Properties.Resources.ToggleIsSupportedSingleLastPageCommand_On;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookSettings.Current.CanPageModeSubSetting(PageMode.WidePage);
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                BookSettings.Current.SetIsSupportedSingleLastPage(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                BookSettings.Current.ToggleIsSupportedSingleLastPage();
            }
        }
    }
}
