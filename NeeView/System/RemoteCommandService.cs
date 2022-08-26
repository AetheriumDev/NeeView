﻿using NeeLaboratory;
using NeeLaboratory.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public delegate void RemoteCommandReciever(RemoteCommand command);

    /// <summary>
    /// RemoteCommandの送受信を管理
    /// </summary>
    public class RemoteCommandService : IDisposable
    {
        static RemoteCommandService() => Current = new RemoteCommandService();
        public static RemoteCommandService Current { get; }


        private RemoteCommandServer _server;
        private RemoteCommandClient _client;

        private Dictionary<string, RemoteCommandReciever> _recievers = new Dictionary<string, RemoteCommandReciever>();


        public RemoteCommandService()
        {
            _server = new RemoteCommandServer();
            _server.Called += Reciever;
            _server.Start();

            _client = new RemoteCommandClient(Environment.SolutionName);

            // アプリ終了前の開放予約
            ApplicationDisposer.Current.Add(this);
        }


        public void AddReciever(string ID, RemoteCommandReciever reciever)
        {
            if (_disposedValue) return;

            _recievers.Add(ID, reciever);
        }

        public void RemoveReciever(string ID)
        {
            if (_disposedValue) return;

            _recievers.Remove(ID);
        }

        private void Reciever(object? sender, RemoteCommandEventArgs e)
        {
            if (_disposedValue) return;

            if (_recievers.TryGetValue(e.Command.Id, out RemoteCommandReciever? reciever))
            {
                AppDispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        reciever(e.Command);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                });
            }
            else
            {
                Debug.WriteLine($"RemoteCommand not found: {e.Command.Id}");
            }
        }

        public void Send(RemoteCommand command, RemoteCommandDelivery delivery)
        {
            if (_disposedValue) return;

            var async = SendAsync(command, delivery);
        }

        public async Task SendAsync(RemoteCommand command, RemoteCommandDelivery delivery)
        {
            if (_disposedValue) return;

            try
            {
                await _client.SendAsync(command, delivery);
            }
            catch(Exception ex)
            {
                // TODO: ここで例外を握りつぶすのはまずい
                Debug.WriteLine(ex.Message);
            }
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _server.Stop();
                    _server.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
