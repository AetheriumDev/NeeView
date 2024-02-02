﻿namespace NeeView
{
    public class RandomBookCommand : CommandElement
    {
        public RandomBookCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.BookMove");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            _ = BookshelfFolderList.Current.RandomFolder();
        }
    }

}
