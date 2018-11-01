using System;
using System.Net;
using Inasync;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Inasync.FakeServers.Tests {

    [TestClass]
    public class HttpServerTests {

        [TestMethod]
        public void GetAvailablePort() {
            new TestCaseRunner()
                .Run(() => FakeServers.HttpServer.GetAvailablePort())
                .Verify((res, desc) => Assert.IsTrue(IPEndPoint.MinPort <= res && res <= IPEndPoint.MaxPort, desc), (Type)null);
        }
    }
}