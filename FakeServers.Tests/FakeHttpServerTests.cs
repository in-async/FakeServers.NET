using System;
using System.Net;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Inasync.FakeServers.Tests {

    [TestClass]
    public class FakeHttpServerTests {

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext) {
            _currentServer = new FakeHttpServer($"http://localhost:{_usedPort}/", null, null);
            _currentServer.Start();
            _httpClient = new HttpClient();
        }

        [ClassCleanup]
        public static void ClassCleanup() {
            _currentServer.Dispose();
            _httpClient.Dispose();
        }

        [TestMethod]
        public void StartNew() {
            var availablePort = HttpServer.GetAvailablePort();
            foreach (var item in TestCases()) {
                new TestCaseRunner($"No.{item.testNumber}")
                    .Run(() => FakeServers.FakeHttpServer.StartNew(item.uriPrefix, item.requestHandler, item.exceptionHandler))
                    .Verify((res, desc) => {
                        Assert.IsTrue(res.IsListening, desc);
                        res.Dispose();
                    }, item.expectedException);
            }

            (int testNumber, string uriPrefix, Action<HttpListenerRequest, HttpListenerResponse> requestHandler, Action<Exception> exceptionHandler, Type expectedException)[] TestCases() => new[] {
                ( 0, null                                 , null        , null       , (Type)typeof(ArgumentNullException)),
                ( 1, ""                                   , null        , null       , (Type)typeof(ArgumentException)),
                ( 2, "h"                                  , null        , null       , (Type)typeof(ArgumentException)),
                ( 3, $"http://localhost:{availablePort}"  , null        , null       , (Type)typeof(ArgumentException)),
                ( 4, $"http://localhost:{_usedPort}/"     , null        , null       , (Type)typeof(HttpListenerException)),
                (10, $"http://localhost:{availablePort}/" , null        , null       , (Type)null),
                (11, $"http://localhost:{availablePort}/" , ReqHandler(), ExHandler(), (Type)null),
                (12, $"https://localhost:{availablePort}/", null        , null       , (Type)null),
            };

            Action<HttpListenerRequest, HttpListenerResponse> ReqHandler() => (req, res) => { };
            Action<Exception> ExHandler() => ex => { };
        }

        [TestMethod]
        public void FakeHttpServer() {
            var availablePort = HttpServer.GetAvailablePort();
            foreach (var item in TestCases()) {
                new TestCaseRunner($"No.{item.testNumber}")
                    .Run(() => new FakeHttpServer(item.uriPrefix, null, null))
                    .Verify((res, desc) => {
                        Assert.IsFalse(res.IsListening, desc);
                    }, item.expectedException);
            }

            (int testNumber, string uriPrefix, Action<HttpListenerRequest, HttpListenerResponse> requestHandler, Action<Exception> exceptionHandler, Type expectedException)[] TestCases() => new[] {
                ( 0, null                                 , null        , null       , (Type)typeof(ArgumentNullException)),
                ( 1, ""                                   , null        , null       , (Type)typeof(ArgumentException)),
                ( 2, "h"                                  , null        , null       , (Type)typeof(ArgumentException)),
                ( 3, $"http://localhost:{availablePort}"  , null        , null       , (Type)typeof(ArgumentException)),
                ( 4, $"http://localhost:{_usedPort}/"     , null        , null       , (Type)null),
                (10, $"http://localhost:{availablePort}/" , null        , null       , (Type)null),
                (11, $"http://localhost:{availablePort}/" , ReqHandler(), ExHandler(), (Type)null),
            };

            Action<HttpListenerRequest, HttpListenerResponse> ReqHandler() => (req, res) => { };
            Action<Exception> ExHandler() => ex => { };
        }

        [TestMethod]
        public void Start() {
            var availablePort = HttpServer.GetAvailablePort();
            foreach (var item in TestCases()) {
                using (var server = new FakeHttpServer(item.uriPrefix, null, null)) {
                    new TestCaseRunner($"No.{item.testNumber}")
                        .Run(() => server.Start())
                        .Verify(desc => {
                            Assert.IsTrue(server.IsListening, desc);
                        }, item.expectedException);
                }
            }

            (int testNumber, string uriPrefix, Type expectedException)[] TestCases() => new[] {
                ( 0, $"http://localhost:{_usedPort}/"     , (Type)typeof(HttpListenerException)),
                (10, $"http://localhost:{availablePort}/" , (Type)null),
                (11, $"https://localhost:{availablePort}/", (Type)null),
            };
        }

        [TestMethod]
        public void OnRequest() {
            foreach (var item in TestCases()) {
                var url = $"http://localhost:{HttpServer.GetAvailablePort()}/";
                using (FakeServers.FakeHttpServer.StartNew(url, item.requestHandler, null)) {
                    new TestCaseRunner($"No.{item.testNumber}")
                        .Run<HttpResponseMessage>(() => _httpClient.GetAsync(url))
                        .Verify((res, desc) => {
                            Assert.AreEqual(item.expected, (int)res.StatusCode, desc);
                            res.Dispose();
                        }, item.expectedException);
                }
            }

            (int testNumber, Action<HttpListenerRequest, HttpListenerResponse> requestHandler, int expected, Type expectedException)[] TestCases() => new[] {
                (11, ReqHandler(100), 100, (Type)null),
                (12, ReqHandler(200), 200, (Type)null),
                (13, ReqHandler(300), 300, (Type)null),
                (14, ReqHandler(400), 400, (Type)null),
                (15, ReqHandler(500), 500, (Type)null),
            };

            Action<HttpListenerRequest, HttpListenerResponse> ReqHandler(int statusCode) => (req, res) => { res.StatusCode = statusCode; };
        }

        [TestMethod]
        public void Stop() {
            using (var server = new FakeHttpServer($"http://localhost:{HttpServer.GetAvailablePort()}/", null, null)) {
                server.Start();

                new TestCaseRunner()
                    .Run(() => server.Stop())
                    .Verify(desc => {
                        Assert.IsFalse(server.IsListening, desc);
                    }, (Type)null);
            }
        }

        [TestMethod]
        public void Close() {
            using (var server = new FakeHttpServer($"http://localhost:{HttpServer.GetAvailablePort()}/", null, null)) {
                server.Start();

                new TestCaseRunner()
                    .Run(() => server.Close())
                    .Verify(desc => {
                        Assert.IsFalse(server.IsListening, desc);
                    }, (Type)null);
            }
        }

        #region Helpers

        private static readonly int _usedPort = FakeServers.HttpServer.GetAvailablePort();
        private static HttpServer _currentServer;
        private static HttpClient _httpClient;

        #endregion Helpers
    }
}