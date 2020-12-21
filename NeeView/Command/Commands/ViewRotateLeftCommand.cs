﻿using NeeLaboratory;
using NeeView.Windows.Property;

namespace NeeView
{
    public class ViewRotateLeftCommand : CommandElement
    {
        public ViewRotateLeftCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;
            this.ParameterSource = new CommandParameterSource(new ViewRotateCommandParameter());
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.ViewRotateLeft((ViewRotateCommandParameter)e.Parameter);
        }
    }



    /// <summary>
    /// ビュー回転コマンド用パラメータ
    /// </summary>
    public class ViewRotateCommandParameter : CommandParameter 
    {
        private int _angle = 45;
        private bool _isStretch;


        // 属性に説明文
        [PropertyRange(0, 180)]
        public int Angle
        {
            get { return _angle; }
            set { SetProperty(ref _angle, MathUtility.Clamp(value, 0, 180)); }
        }

        // 属性に説明文
        [PropertyMember]
        public bool IsStretch
        {
            get { return _isStretch; }
            set { SetProperty(ref _isStretch, value); }
        }
    }
}
