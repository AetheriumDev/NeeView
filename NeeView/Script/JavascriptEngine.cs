﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Jint;

namespace NeeView
{
    public class JavascriptEngine
    {
        private Jint.Engine _engine;
        private CommandHost _commandHost;
        private CancellationToken _cancellationToken;

        public JavascriptEngine()
        {
            _commandHost = new CommandHost();
            _engine = new Jint.Engine(config => config
                .DebugMode(true)
                .AllowClr(typeof(System.Diagnostics.Process).Assembly));

            _engine.SetValue("sleep", (Action<int>)Sleep);
            _engine.SetValue("log", (Action<object>)Log);
            _engine.SetValue("system", (Action<string, string>)SystemCall);
            _engine.SetValue("include", (Func<string, object?>)ExecureFile);
            _engine.SetValue("nv", _commandHost);
        }

        public string? CurrentPath { get; private set; }

        public string? CurrentFolder { get; set; }

        public bool IsToastEnable { get; set; }

        public bool IsDarty => _commandHost.IsDarty;


        [Documentable(Name = "nv")]
        public CommandHost CommandHost => _commandHost;


        public void SetArgs(List<string> args)
        {
            _commandHost.SetArgs(args);
        }

        [Documentable(Name = "include")]
        public object? ExecureFile(string path)
        {
            if (path is null) throw new ArgumentNullException(nameof(path));

            return ExecureFile(path, _cancellationToken);
        }

        public object? ExecureFile(string path, CancellationToken token)
        {
            var fullpath = GetFullPath(path);
            string script = File.ReadAllText(fullpath, Encoding.UTF8);

            var oldFolder = CurrentFolder;
            try
            {
                CurrentFolder = LoosePath.GetDirectoryName(fullpath);
                return Execute(fullpath, script, token);
            }
            finally
            {
                CurrentFolder = oldFolder;
            }
        }

        public object? Execute(string? path, string script, CancellationToken token)
        {
            _cancellationToken = token;
            _commandHost.SetCancellationToken(token);

            var oldPath = CurrentPath;
            try
            {
                CurrentPath = path;
                var parserOptions = path is null ? new Esprima.ParserOptions() : new Esprima.ParserOptions(path);
                var result = _engine.Evaluate(script, parserOptions);
                return result?.ToObject();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ScriptException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ScriptException(CreateScriptErrorMessage(ex.Message), ex);
            }
            finally
            {
                CurrentPath = oldPath;
            }
        }

        public void ExceptionPrcess(Exception ex)
        {
            if (ex is OperationCanceledException)
            {
                return;
            }

            var message = (ex is ScriptException) ? ex.Message : CreateScriptErrorMessage(ex.Message).ToString();
            ConsoleWindowManager.Current.ErrorMessage(message, this.IsToastEnable);
        }

        [Documentable(Name = "log")]
        public void Log(object log)
        {
            var message = log as string ?? new JsonStringBulder(log).ToString();
            ConsoleWindowManager.Current.WriteLine(ScriptMessageLevel.None, message);
        }

        [Documentable(Name = "sleep")]
        public void Sleep(int millisecond)
        {
            if (_cancellationToken.WaitHandle.WaitOne(millisecond))
            {
                throw new OperationCanceledException();
            }
        }

        [Documentable(Name = "system")]
        public void SystemCall(string filename, string? args = null)
        {
            ExternalProcess.Start(filename, args, new ExternalProcessOptions() { IsThrowException = true });
        }


        public void SetValue(string name, object value)
        {
            _engine.SetValue(name, value);
        }

        public object GetValue(string name)
        {
            return _engine.GetValue(name).ToObject();
        }

        private string GetFullPath(string path)
        {
            if (CurrentFolder != null && !Path.IsPathRooted(path))
            {
                path = Path.Combine(CurrentFolder, path);
            }

            return Path.GetFullPath(path);
        }


        public ScriptNotice CreateScriptErrorMessage(string s)
        {
            var location = _engine.DebugHandler?.CurrentLocation;

            string? source = null;
            int line = -1;
            string message = s.Trim();

            var regex = new Regex(@"^Line\s*(\d+):(.+)$", RegexOptions.IgnoreCase);
            var match = regex.Match(s);
            if (match.Success)
            {
                line = int.Parse(match.Groups[1].Value);
                message = match.Groups[2].Value.Trim();
            }
            if (location.HasValue)
            {
                source = location.Value.Source;
                line = location.Value.Start.Line;
            }

            return new ScriptNotice(source, line, message);
        }



        internal WordNode CreateWordNode(string name)
        {
            return _commandHost.CreateWordNode(name);
        }
    }

}
