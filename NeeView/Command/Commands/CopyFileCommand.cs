﻿using System.Runtime.Serialization;

namespace NeeView
{
    public class CopyFileCommand : CommandElement
    {
        public CopyFileCommand()
        {
            this.Group = Properties.Resources.CommandGroup_File;
            this.ShortCutKey = "Ctrl+C";
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new CopyFileCommandParameter());

        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.Control.CanOpenFilePlace();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Control.CopyToClipboard(e.Parameter.Cast<CopyFileCommandParameter>());
        }
    }


}
