﻿using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;

namespace NeeView
{
    public class MouseWheelGestureConverter : TypeConverter
    {
        private const char _modifiersDelimiter = '+';

        private static readonly MouseWheelActionConverter _mouseActionConverter = new();
        private static readonly ModifierKeysConverter _modifierKeysConverter = new();
        private static readonly ModifierMouseButtonsConverter _modifierMouseButtonsConverter = new();


        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object source)
        {
            if (source is string s && source != null)
            {
                string fullName = s.Trim();
                string mouseActionToken;
                string modifierMouseButtonsToken;
                string modifiersToken;

                if (fullName == string.Empty)
                {
                    return new MouseWheelGesture(MouseWheelAction.None, ModifierKeys.None, ModifierMouseButtons.None);
                }

                int offset = fullName.LastIndexOf(_modifiersDelimiter);
                if (offset >= 0)
                {
                    string modifiers = fullName[..offset];
                    mouseActionToken = fullName[(offset + 1)..];

                    offset = modifiers.IndexOf("BUTTON", StringComparison.OrdinalIgnoreCase);
                    if (offset >= 0)
                    {
                        offset = modifiers.LastIndexOf(_modifiersDelimiter, offset);
                        if (offset >= 0)
                        {
                            modifiersToken = modifiers[..offset];
                            modifierMouseButtonsToken = modifiers[(offset + 1)..];
                        }
                        else
                        {
                            modifiersToken = string.Empty;
                            modifierMouseButtonsToken = modifiers;
                        }
                    }
                    else
                    {
                        modifiersToken = modifiers;
                        modifierMouseButtonsToken = string.Empty;
                    }

                }
                else
                {
                    modifiersToken = string.Empty;
                    modifierMouseButtonsToken = string.Empty;
                    mouseActionToken = fullName;
                }

                object? mouseAction = _mouseActionConverter.ConvertFrom(context, culture, mouseActionToken);
                object modifierKeys = ModifierKeys.None;
                object? modifierMouseButtons = ModifierMouseButtons.None;

                if (mouseAction != null)
                {
                    if (modifiersToken != string.Empty)
                    {
                        modifierKeys = _modifierKeysConverter.ConvertFrom(context, culture, modifiersToken);

                        if (modifierKeys is not ModifierKeys)
                        {
                            modifierKeys = ModifierKeys.None;
                        }
                    }

                    if (modifierMouseButtonsToken != string.Empty)
                    {
                        modifierMouseButtons = _modifierMouseButtonsConverter.ConvertFrom(context, culture, modifierMouseButtonsToken);

                        if (modifierMouseButtons is not ModifierMouseButtons)
                        {
                            modifierMouseButtons = ModifierMouseButtons.None;
                        }
                    }

                    return new MouseWheelGesture((MouseWheelAction)mouseAction, (ModifierKeys)modifierKeys, (ModifierMouseButtons)modifierMouseButtons);
                }
            }
            throw GetConvertFromException(source);
        }


        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (context?.Instance is MouseWheelGesture mouseGesture)
                {
                    return (ModifierKeysConverter.IsDefinedModifierKeys(mouseGesture.Modifiers)
                           && ModifierMouseButtonsConverter.IsDefinedModifierMouseButtons(mouseGesture.ModifierButtons)
                           && MouseWheelActionConverter.IsDefinedMouseWheelAction(mouseGesture.WheelAction));
                }
            }
            return false;
        }


        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException(nameof(destinationType));
            }

            if (destinationType == typeof(string))
            {
                if (value == null)
                {
                    return string.Empty;
                }

                if (value is MouseWheelGesture mouseGesture)
                {
                    string strGesture = "";

                    strGesture += _modifierKeysConverter.ConvertTo(context, culture, mouseGesture.Modifiers, destinationType) as string;
                    if (strGesture != string.Empty)
                    {
                        strGesture += _modifiersDelimiter;
                    }

                    strGesture += _modifierMouseButtonsConverter.ConvertTo(context, culture, mouseGesture.ModifierButtons, destinationType) as string;
                    if (strGesture != string.Empty && strGesture[^1] != _modifiersDelimiter)
                    {
                        strGesture += _modifiersDelimiter;
                    }

                    strGesture += _mouseActionConverter.ConvertTo(context, culture, mouseGesture.WheelAction, destinationType) as string;

                    return strGesture;
                }
            }
            throw GetConvertToException(value, destinationType);
        }
    }
}
