﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public class WebSocketHandler : IDisposable
    {
        public WebSocket Socket;
        public DateTime? CreatedDate;
        private readonly TaskQueue _sendQueue = new TaskQueue();

        public WebSocketState State
        {
            get
            {
                try
                {
                    return Socket.State;
                }
                catch (ObjectDisposedException)
                {
                    return WebSocketState.Closed;
                }
            }
        }

        public WebSocketHandler(WebSocket socket)
        {
            this.Socket = socket;
        }

        public Task SendAsync(string message)
        {
            return Task.Run(() =>
            {
                var sendContext = new SendContext(this, message);

                if (Socket.State != WebSocketState.Open)
                {
                    return;
                }

                _sendQueue.Enqueue(new Task(() =>
                {
                    if (sendContext.Handler.Socket.State != WebSocketState.Open)
                    {
                        return;
                    }

                    try
                    {
                        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                        Socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                    }
                    catch (Exception ex)
                    {
                        // Swallow exceptions on send
                        Trace.TraceError("Error while sending: " + ex);
                    }
                }));
            });
        }

        private class TaskAsyncHelper
        {
            private static readonly Task _emptyTask = MakeTask<object>(null);
            public static Task Empty
            {
                get
                {
                    return _emptyTask;
                }
            }

            private static Task<T> MakeTask<T>(T value)
            {
                return FromResult<T>(value);
            }

            public static Task<T> FromResult<T>(T value)
            {
                var tcs = new TaskCompletionSource<T>();
                tcs.SetResult(value);
                return tcs.Task;
            }
        }

        private class SendContext
        {
            public WebSocketHandler Handler;
            public string Message;

            public SendContext(WebSocketHandler webSocketHandler, string message)
            {
                Handler = webSocketHandler;
            }
        }

        public void Dispose()
        {
            this.Socket.Dispose();
            this.Socket = null;
        }

        internal Task CloseAsync(WebSocketCloseStatus webSocketCloseStatus, string p, CancellationToken cancellationToken)
        {
            return this.Socket.CloseAsync(webSocketCloseStatus, p, cancellationToken);
        }

        internal Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> arraySegment, CancellationToken cancellationToken)
        {
            return this.Socket.ReceiveAsync(arraySegment, cancellationToken);
        }

        internal Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType webSocketMessageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            return this.Socket.SendAsync(buffer, webSocketMessageType, endOfMessage, cancellationToken);
        }
    }

    public class TaskQueue
    {
        public void Enqueue(Task task)
        {
            lock (lockObject)
            {
                if (Busy)
                {
                    _q.Enqueue(task);
                }
                else
                {
                    Busy = true;
                    task.Start();
                    task.ContinueWith((t) =>
                    {
                        NextTask();
                    });
                }
            }
        }

        private static Queue<Task> _q = new Queue<Task>();

        private static bool Busy = false;
        private static object lockObject = new object();

        public TaskQueue()
        {

        }

        private void NextTask()
        {
            lock (lockObject)
            {
                if (_q.Count > 0)
                {
                    var task = _q.Dequeue();
                    task.Start();
                    task.ContinueWith((t) =>
                    {
                        NextTask();
                    });
                }
                else
                {
                    Busy = false;
                }
            }
        }
    }
}
