using System;
using System.Threading.Tasks;

namespace Inasync.FakeServers.Tests {

    public static class TestCaseRunnerExtensions {

        public static ITestActual<TResult> Run<TResult>(this TestCaseRunner runner, Func<Task<TResult>> targetCode) {
            if (targetCode == null) { throw new ArgumentNullException(nameof(targetCode)); }

            return runner.Run(() => targetCode().GetAwaiter().GetResult());
        }

        public static ITestActual Run(this TestCaseRunner runner, Func<Task> targetCode) {
            if (targetCode == null) { throw new ArgumentNullException(nameof(targetCode)); }

            return runner.Run(() => targetCode().GetAwaiter().GetResult());
        }
    }
}