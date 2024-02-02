﻿using System.Windows.Data;


namespace NeeView
{
    public class SetSortModeTimeStampDescendingCommand : CommandElement
    {
        public SetSortModeTimeStampDescendingCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.PageOrder");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.TimeStampDescending);
        }
        
        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookSettings.Current.CanEdit;
        }
        
        public override void Execute(object? sender, CommandContext e)
        {
            BookSettings.Current.SetSortMode(PageSortMode.TimeStampDescending);
        }
    }
}
