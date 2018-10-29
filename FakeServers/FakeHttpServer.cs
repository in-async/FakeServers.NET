using System;
using System.Diagnostics;
using System.Net;

namespace Inasync.FakeServers {

    /// <summary>
    /// <see cref="IHttpServer"/> の Fake 実装。
    /// </summary>
    public sealed class FakeHttpServer : HttpServer {

        public static FakeHttpServer StartNew(string url, Action<HttpListenerRequest, HttpListenerResponse> handler) {
            var httpServer = new FakeHttpServer(url, handler);
            httpServer.Start();
            return httpServer;
        }

        private readonly Action<HttpListenerRequest, HttpListenerResponse> _handler;

        public FakeHttpServer(string url, Action<HttpListenerRequest, HttpListenerResponse> handler) : base(url) {
            _handler = handler;
        }

        protected override void OnRequest(HttpListenerContext context) {
            var request = context.Request;
            using (var response = context.Response) {
                _handler?.Invoke(request, response);
            }
        }

        protected override void OnException(Exception exception) {
            Trace.TraceError(exception.ToString());
        }
    }
}