﻿using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    // 渦巻き
    [DataContract]
    public class SwirlEffectUnit : EffectUnit
    {
        private static readonly SwirlEffect _effect = new();
        public override Effect GetEffect() => _effect;

        /// <summary>
        /// Property: Center
        /// </summary>
        [DataMember]
        [PropertyMember]
        [DefaultValue(typeof(Point), "0.5,0.5")]
        public Point Center
        {
            get { return _effect.Center; }
            set { if (_effect.Center != value) { _effect.Center = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: TwistAmount
        /// </summary>
        [DataMember]
        [PropertyRange(-50, 50)]
        [DefaultValue(10)]
        public double TwistAmount
        {
            get { return _effect.TwistAmount; }
            set { if (_effect.TwistAmount != value) { _effect.TwistAmount = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
