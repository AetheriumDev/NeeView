﻿namespace NeeView
{
    public class ReloadSettingCommand : CommandElement
    {
        public ReloadSettingCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Other;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            var setting = SaveData.Current.LoadUserSetting(false);
            UserSettingTools.Restore(setting);
        }
    }
}
