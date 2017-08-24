﻿// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Windows.Media.Effects;
using System.Windows;
using System.Windows.Media;
using System.Reflection;

namespace NeeView.Effects
{
    public class ColorSelectEffect : ShaderEffect
    {
        private static PixelShader s_pixelShader = new PixelShader()
        {
            UriSource = Tools.MakePackUri(typeof(ColorSelectEffect).Assembly, "Effects/Shaders/ColorSelectEffect.ps")
        };

        public ColorSelectEffect()
        {
            PixelShader = s_pixelShader;

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(HueProperty);
            UpdateShaderValue(RangeProperty);
            UpdateShaderValue(CurveProperty);
        }

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(ColorSelectEffect), 0);
        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }


        //
        public static readonly DependencyProperty HueProperty = DependencyProperty.Register("Hue", typeof(double), typeof(ColorSelectEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(0)));
        public double Hue
        {
            get { return (double)GetValue(HueProperty); }
            set { SetValue(HueProperty, value); }
        }

        //
        public static readonly DependencyProperty RangeProperty = DependencyProperty.Register("Range", typeof(double), typeof(ColorSelectEffect), new UIPropertyMetadata(0.1, PixelShaderConstantCallback(1)));
        public double Range
        {
            get { return (double)GetValue(RangeProperty); }
            set { SetValue(RangeProperty, value); }
        }

        //
        public static readonly DependencyProperty CurveProperty = DependencyProperty.Register("Curve", typeof(double), typeof(ColorSelectEffect), new UIPropertyMetadata(1.0, PixelShaderConstantCallback(2)));
        public double Curve
        {
            get { return (double)GetValue(CurveProperty); }
            set { SetValue(CurveProperty, value); }
        }
    }
}
