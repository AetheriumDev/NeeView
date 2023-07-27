﻿namespace NeeView
{
    public class DeleteBookCommand : CommandElement
    {
        public DeleteBookCommand()
        {
            this.Group = Properties.Resources.CommandGroup_File;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.BookControl.CanDeleteBook();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.BookControl.DeleteBook();
        }
    }
}
