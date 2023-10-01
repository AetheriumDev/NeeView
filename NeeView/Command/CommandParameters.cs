﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Data;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;

namespace NeeView
{
    /// <summary>
    /// コマンドパラメータ（基底）
    /// </summary>
    [JsonConverter(typeof(JsonCommandParameterConverter))]
    public abstract class CommandParameter : BindableBase, ICloneable
    {
        private readonly Func<object, object, bool> _equals;

        public CommandParameter()
        {
            _equals = ObjectExtensions.MakeEqualsMethod(this.GetType());
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public bool MemberwiseEquals(CommandParameter? other)
        {
            if (other is null) return false;
            return _equals(this, other);
        }
    }

    public static class CommandParameterExtensions
    {
        public static T Cast<T>(this CommandParameter? self) where T : CommandParameter
        {
            var param = self as T ?? throw new InvalidCastException();
            return param;
        }
    }


    /// <summary>
    /// 操作反転コマンドパラメータ基底
    /// </summary>
    public class ReversibleCommandParameter : CommandParameter
    {
        private bool _isReverse = true;

        [PropertyMember]
        public bool IsReverse
        {
            get => _isReverse;
            set => SetProperty(ref _isReverse, value);
        }
    }

    /// <summary>
    /// JsonConverter for CommandParameter.
    /// Support polymorphism.
    /// </summary>
    public sealed class JsonCommandParameterConverter : JsonConverter<CommandParameter>
    {
        // NOTE: need add polymorphism class type.
        public static Type[] KnownTypes { get; set; } = new Type[]
        {
            typeof(ReversibleCommandParameter),
            typeof(MoveSizePageCommandParameter),
            typeof(TogglePageModeCommandParameter),
            typeof(ToggleStretchModeCommandParameter),
            typeof(StretchModeCommandParameter),
            typeof(ViewScrollCommandParameter),
            typeof(ViewScaleCommandParameter),
            typeof(ViewRotateCommandParameter),
            typeof(MovePlaylsitItemInBookCommandParameter),
            typeof(ScrollPageCommandParameter),
            typeof(FocusMainViewCommandParameter),
            typeof(ExportImageAsCommandParameter),
            typeof(ExportImageCommandParameter),
            typeof(OpenExternalAppCommandParameter),
            typeof(CopyFileCommandParameter),
            typeof(ViewScrollNTypeCommandParameter),
            typeof(ScriptCommandParameter),
            typeof(ImportBackupCommandParameter),
            typeof(ExportBackupCommandParameter),
        };


        public override CommandParameter? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "Type")
            {
                throw new JsonException();
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }
            var typeString = reader.GetString();

            Type? type = KnownTypes.FirstOrDefault(e => e.Name == typeString);
            Debug.Assert(type != null);
            if (type is null) throw new JsonException($"Nor support type: {typeString}");

            if (!reader.Read() || reader.GetString() != "Value")
            {
                throw new JsonException();
            }
            if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            object? instance;
            if (type != null)
            {
                instance = JsonSerializer.Deserialize(ref reader, type, options);
            }
            else
            {
                Debug.WriteLine($"Nor support type: {typeString}");
                reader.Skip();
                instance = null;
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException();
            }

            return instance as CommandParameter;
        }

        public override void Write(Utf8JsonWriter writer, CommandParameter value, JsonSerializerOptions options)
        {

            var type = value.GetType();
            Debug.Assert(KnownTypes.Contains(type));

            var def = Activator.CreateInstance(type) as CommandParameter ?? throw new InvalidOperationException();

            if (value.MemberwiseEquals(def))
            {
                //Debug.WriteLine($"{type} is default.");
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStartObject();
                writer.WriteString("Type", type.Name);
                writer.WritePropertyName("Value");
                JsonSerializer.Serialize(writer, value, type, options);
                writer.WriteEndObject();
            }

        }
    }
}
