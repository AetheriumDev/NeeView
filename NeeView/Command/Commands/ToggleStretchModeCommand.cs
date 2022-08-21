﻿using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class ToggleStretchModeCommand : CommandElement
    {
        public ToggleStretchModeCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ImageScale;
            this.ShortCutKey = "LeftButton+WheelDown";
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new ToggleStretchModeCommandParameter());
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return MainViewComponent.Current.ViewController.GetToggleStretchMode(e.Parameter.Cast<ToggleStretchModeCommandParameter>()).ToAliasName();
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            Config.Current.View.StretchMode = MainViewComponent.Current.ViewController.GetToggleStretchMode(e.Parameter.Cast<ToggleStretchModeCommandParameter>());
        }
    }


    /// <summary>
    /// スケールモードトグル用設定
    /// </summary>
    [DataContract]
    public class ToggleStretchModeCommandParameter : CommandParameter
    {
        private bool _isLoop = true;
        private bool _isEnableNone = true;
        private bool _isEnableUniform = true;
        private bool _isEnableUniformToFill = true;
        private bool _isEnableUniformToSize = true;
        private bool _isEnableUniformToVertical = true;
        private bool _isEnableUniformToHorizontal = true;

        // ループ
        [DataMember]
        [PropertyMember]
        public bool IsLoop
        {
            get => _isLoop;
            set => SetProperty(ref _isLoop, value);
        }

        // 表示名
        [DataMember]
        [PropertyMember]
        public bool IsEnableNone
        {
            get => _isEnableNone;
            set => SetProperty(ref _isEnableNone, value);
        }

        [DataMember]
        [PropertyMember]
        public bool IsEnableUniform
        {
            get => _isEnableUniform;
            set => SetProperty(ref _isEnableUniform, value);
        }

        [DataMember]
        [PropertyMember]
        public bool IsEnableUniformToFill
        {
            get => _isEnableUniformToFill;
            set => SetProperty(ref _isEnableUniformToFill, value);
        }

        [DataMember]
        [PropertyMember]
        public bool IsEnableUniformToSize
        {
            get => _isEnableUniformToSize;
            set => SetProperty(ref _isEnableUniformToSize, value);
        }

        [DataMember]
        [PropertyMember]
        public bool IsEnableUniformToVertical
        {
            get => _isEnableUniformToVertical;
            set => SetProperty(ref _isEnableUniformToVertical, value);
        }

        [DataMember]
        [PropertyMember]
        public bool IsEnableUniformToHorizontal
        {
            get => _isEnableUniformToHorizontal;
            set => SetProperty(ref _isEnableUniformToHorizontal, value);
        }


        public IReadOnlyDictionary<PageStretchMode, bool> GetStretchModeDictionary()
        {
            return new Dictionary<PageStretchMode, bool>()
            {
                [PageStretchMode.None] = IsEnableNone,
                [PageStretchMode.Uniform] = IsEnableUniform,
                [PageStretchMode.UniformToFill] = IsEnableUniformToFill,
                [PageStretchMode.UniformToSize] = IsEnableUniformToSize,
                [PageStretchMode.UniformToVertical] = IsEnableUniformToVertical,
                [PageStretchMode.UniformToHorizontal] = IsEnableUniformToHorizontal,
            };
        }
    }

}
