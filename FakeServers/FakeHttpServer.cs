using System;
using System.Net;

namespace Inasync.FakeServers {

    /// <summary>
    /// <see cref="IHttpServer"/> の Fake 実装。
    /// </summary>
    public sealed class FakeHttpServer : HttpServer {

        public static FakeHttpServer StartNew(string url, Action<HttpListenerRequest, HttpListenerResponse> requestHandler, Action<Exception> exceptionHandler) {
            var httpServer = new FakeHttpServer(url, requestHandler, exceptionHandler);
            httpServer.Start();
            return httpServer;
        }

        private readonly Action<HttpListenerRequest, HttpListenerResponse> _requestHandler;
        private readonly Action<Exception> _exceptionHandler;

        public FakeHttpServer(string url, Action<HttpListenerRequest, HttpListenerResponse> requestHandler, Action<Exception> exceptionHandler) : base(url) {
            _requestHandler = requestHandler;
            _exceptionHandler = exceptionHandler;
        }

        protected override void OnRequest(HttpListenerContext context) {
            var request = context.Request;
            using (var response = context.Response) {
                _requestHandler?.Invoke(request, response);
            }
        }

        protected override void OnException(Exception exception) {
            _exceptionHandler?.Invoke(exception);
        }
    }
}