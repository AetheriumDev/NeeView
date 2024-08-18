﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace NeeView
{
    public class ScriptCommand : CommandElement, IDisposable
    {
        private class ScriptCommandParameterDecorator : ICommandParameterDecorator
        {
            private readonly string _path;
            private readonly ScriptCommandSourceMap _sourceMap;

            public ScriptCommandParameterDecorator(string path, ScriptCommandSourceMap sourceMap)
            {
                _path = path;
                _sourceMap = sourceMap;
            }

            public void DecorateCommandParameter(CommandParameter parameter)
            {
                if (parameter is not ScriptCommandParameter scriptCommandParameter) return;

                if (_sourceMap.TryGetValue(_path, out var source))
                {
                    scriptCommandParameter.Argument = source.Args;
                }
            }
        }


        public const string Prefix = "Script_";
        public const string EventOnBookLoaded = Prefix + ScriptCommandSource.OnBookLoadedFilename;
        public const string EventOnPageChanged = Prefix + ScriptCommandSource.OnPageChangedFilename;
        public const string EventOnWindowStateChanged = Prefix + ScriptCommandSource.OnWindowStateChangedFilename;

        private readonly string _path;
        private readonly ScriptCommandSourceMap _sourceMap;
        private GesturesMemento? _defaultGestures;
        private string? _defaultArgs;
        private ScriptValueFlagBindingSource? _bindingSource;
        private bool _disposedValue;

        public ScriptCommand(string path, ScriptCommandSourceMap sourceMap) : base(PathToScriptCommandName(path))
        {
            _path = path;
            _sourceMap = sourceMap ?? throw new ArgumentNullException(nameof(sourceMap));

            this.Group = Properties.TextResources.GetString("CommandGroup.Script");
            this.Text = LoosePath.GetFileNameWithoutExtension(_path);

            this.ParameterSource = new CommandParameterSource(new ScriptCommandParameter(), new ScriptCommandParameterDecorator(path, sourceMap));
            this.ParameterSource.ParameterChanged += ParameterSource_ParameterChanged;

            UpdateDocument(true);
        }

        private void ParameterSource_ParameterChanged(object? sender, ParameterChangedEventArgs e)
        {
            if (_bindingSource != null && (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(ScriptCommandParameter.CheckFlagKey)))
            {
                _bindingSource.Key = GetScriptCommandParameter().CheckFlagKey;
            }
        }

        public string Path => _path;


        public static bool IsScriptCommandName(string name)
        {
            return name.StartsWith(Prefix);
        }

        public static string PathToScriptCommandName(string path)
        {
            return Prefix + LoosePath.GetFileNameWithoutExtension(path);
        }

        protected override CommandElement CloneInstance()
        {
            var command = new ScriptCommand(_path, _sourceMap);
            if (_sourceMap.TryGetValue(_path, out var source))
            {
                command.Text = source.Text;
                command.Remarks = source.Remarks;
                command.ShortCutKey = ShortcutKey.Empty;
                command.TouchGesture = TouchGesture.Empty;
                command.MouseGesture = MouseSequence.Empty;
            }
            return command;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            ScriptManager.Current.Execute(sender, _path, Name, (e.Parameter.Cast<ScriptCommandParameter>()).Argument);
        }

        public override void UpdateDefaultParameter()
        {
            StoreDefaultArgs();
        }

        private void StoreDefaultGesture()
        {
            _defaultGestures = CreateGesturesMemento();
        }

        private void StoreDefaultArgs()
        {
            _defaultArgs = GetDefaultArgs();
        }

        private string? GetDefaultArgs()
        {
            return (ParameterSource?.GetDefault() as ScriptCommandParameter)?.Argument;
        }

        private ScriptCommandParameter GetScriptCommandParameter()
        {
            return (Parameter as ScriptCommandParameter) ?? throw new InvalidOperationException();
        }

        public void UpdateDocument(bool isForce)
        {
            if (_sourceMap.TryGetValue(_path, out var source))
            {
                IsCloneable = source.IsCloneable;

                Text = source.Text;
                if (IsCloneCommand())
                {
                    Text += " " + NameSource.Number.ToString();
                }

                Remarks = source.Remarks;

                if (isForce || (_defaultGestures != null && _defaultGestures.IsEquals(this) && !IsCloneCommand()))
                {
                    ShortCutKey = new ShortcutKey(source.ShortCutKey);
                    MouseGesture = new MouseSequence(source.MouseGesture);
                    TouchGesture = new TouchGesture(source.TouchGesture);
                    StoreDefaultGesture();
                }

                var parameter = GetScriptCommandParameter();
                if (isForce || parameter.Argument == _defaultArgs)
                {
                    parameter.Argument = source.Args;
                    StoreDefaultArgs();
                }
            }
        }

        public void OpenFile()
        {
            ExternalProcess.OpenWithTextEditor(_path);
        }


        public override Binding? CreateIsCheckedBinding()
        {
            if (_disposedValue) return null;

            _bindingSource ??= new ScriptValueFlagBindingSource(CommandHostStaticResource.Current);
            _bindingSource.Key = GetScriptCommandParameter().CheckFlagKey;
            return new Binding(nameof(_bindingSource.IsChecked)) { Source = _bindingSource };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _bindingSource?.Dispose();
                    if (this.ParameterSource != null)
                    {
                        this.ParameterSource.ParameterChanged -= ParameterSource_ParameterChanged;
                    }
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}
