﻿using NeeView.Data;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Setting
{
    /// <summary>
    /// Setting: Manipulate
    /// </summary>
    public class SettingPageManipulate : SettingPage
    {
        public SettingPageManipulate() : base(Properties.Resources.SettingPage_Manipurate)
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageMouse(),
                new SettingPageTouch(),
                new SettingPageLoupe(),
            };

            var section = new SettingItemSection(Properties.Resources.SettingPage_Manipurate_GeneralViewOperation);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.IsLimitMove))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.ViewOrigin))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.RotateCenter))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.ScaleCenter))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.FlipCenter))));
            //section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.IsKeepScale))));
            //section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.IsKeepAngle))));
            //section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.IsKeepFlip))));
            section.Children.Add(new SettingItemIndexValue<double>(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.AngleFrequency)), new AngleFrequency(), false));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.IsKeepPageTransform))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.ScrollDuration))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.PageMoveDuration))));

            this.Items = new List<SettingItem>() { section };
        }

        /// <summary>
        /// ビュー回転スナップ値
        /// </summary>
        public class AngleFrequency : IndexDoubleValue
        {
            private static readonly List<double> _values = new() { 0, 5, 10, 15, 20, 30, 45, 60, 90 };

            public AngleFrequency() : base(_values)
            {
            }

            public AngleFrequency(double value) : base(_values)
            {
                Value = value;
            }

            public override string ValueString => Value == 0 ? Properties.Resources.Word_Stepless : $"{Value} {Properties.Resources.Word_Degree}";
        }
    }

    /// <summary>
    /// Setting: Mouse
    /// </summary>
    public class SettingPageMouse : SettingPage
    {
        public SettingPageMouse() : base(Properties.Resources.SettingPage_Manipurate_Mouse)
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPage_Manipurate_MouseDrag);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.IsDragEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(DragActionTable.Current, nameof(DragActionTable.Elements)), new SettingMouseDragControl()) { IsStretch = true, });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.MinimumDragDistance))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.InertiaSensitivity))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.IsGestureEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.GestureMinimumDistance))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPage_Manipurate_MouseHold);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.LongButtonDownMode))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.LongButtonMask))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.LongButtonDownTime))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.LongButtonRepeatTime))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPage_Manipurate_HoverScroll);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.HoverScrollDuration))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPage_Manipurate_MouseVisibility);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.IsCursorHideEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.CursorHideTime))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.CursorHideReleaseDistance))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.IsCursorHideReleaseAction))));
            this.Items.Add(section);
        }
    }

    /// <summary>
    /// Setting: Touch
    /// </summary>
    public class SettingPageTouch : SettingPage
    {
        public SettingPageTouch() : base(Properties.Resources.SettingPage_Manipurate_Touch)
        {
            var dragEnumMap = TouchActionClass.Drag.GetAliasNameMap().ToDictionary(e => (Enum)e.Key, e => e.Value);
            var holdEnumMap = TouchActionClass.Hold.GetAliasNameMap().ToDictionary(e => (Enum)e.Key, e => e.Value);

            var section = new SettingItemSection(Properties.Resources.SettingPage_Manipurate_TouchGeneral);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.DragAction), new PropertyMemberElementOptions() { EnumMap = dragEnumMap })));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.HoldAction), new PropertyMemberElementOptions() { EnumMap = holdEnumMap })));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.IsAngleEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.IsScaleEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.GestureMinimumDistance))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.MinimumManipulationRadius))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.MinimumManipulationDistance))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.InertiaSensitivity))));

            this.Items = new List<SettingItem>() { section };
        }
    }

    /// <summary>
    /// Setting: Loupe
    /// </summary>
    public class SettingPageLoupe : SettingPage
    {
        public SettingPageLoupe() : base(Properties.Resources.SettingPage_Manipurate_Loupe)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPage_Manipurate_LoupeGeneral);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsLoupeCenter))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsResetByRestart))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsVisibleLoupeInfo))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsResetByPageChanged))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsWheelScalingEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsEscapeKeyEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.DefaultScale))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.MinimumScale))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.MaximumScale))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.ScaleStep))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.Speed))));

            this.Items = new List<SettingItem>() { section };
        }
    }
}
