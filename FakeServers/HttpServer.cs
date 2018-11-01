using System;
using System.Net;
using System.Net.Sockets;

namespace Inasync.FakeServers {

    /// <summary>
    /// HTTP Server を表すインターフェース。
    /// </summary>
    public interface IHttpServer : IDisposable {

        /// <summary>
        /// HTTP Server が稼働しているか否か。
        /// </summary>
        bool IsListening { get; }

        /// <summary>
        /// HTTP Server を起動します。
        /// </summary>
        /// <exception cref="HttpListenerException"></exception>
        void Start();

        /// <summary>
        /// HTTP Server を停止します。
        /// </summary>
        void Stop();

        /// <summary>
        /// HTTP Server を終了します。
        /// </summary>
        void Close();
    }

    /// <summary>
    /// <see cref="IHttpServer"/> の抽象クラス。
    /// </summary>
    public abstract class HttpServer : IHttpServer {

        /// <summary>
        /// ローカルマシン上で空いているポート番号を返します。
        /// </summary>
        /// <returns>利用可能なポート番号。</returns>
        /// <exception cref="SocketException"></exception>
        public static int GetAvailablePort() {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();

            return port;
        }

        private readonly HttpListener _listener;

        public HttpServer(string uriPrefix) {
            if (uriPrefix == null) { throw new ArgumentNullException(nameof(uriPrefix)); }

            _listener = new HttpListener();
            _listener.Prefixes.Add(uriPrefix);
        }

        public bool IsListening => _listener.IsListening;

        public void Start() {
            _listener.Start();
            _listener.BeginGetContext(OnGetContext, null);
        }

        public void Stop() => _listener.Stop();

        public void Close() => _listener.Close();

        private void OnGetContext(IAsyncResult asyncResult) {
            if (_disposed) { return; }

            try {
                var context = _listener.EndGetContext(asyncResult);
                OnRequest(context);
            }
            catch (Exception ex) {
                OnException(ex);
            }

            if (!_disposed && _listener.IsListening) {
                _listener.BeginGetContext(OnGetContext, null);
            }
        }

        protected abstract void OnRequest(HttpListenerContext context);

        protected abstract void OnException(Exception exception);

        #region IDisposable Support

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                _disposed = true;

                if (disposing) {
                    _listener.Abort();
                }
            }
        }

        public void Dispose() {
            Dispose(true);
        }

        #endregion IDisposable Support
    }
}