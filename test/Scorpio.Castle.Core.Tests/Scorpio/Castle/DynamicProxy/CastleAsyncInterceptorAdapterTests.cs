using System;
using System.Threading.Tasks;

using Castle.DynamicProxy;

using NSubstitute;

using Shouldly;

using Xunit;

namespace Scorpio.Castle.DynamicProxy
{
    public class CastleAsyncInterceptorAdapterTests
    {
        [Fact]
        public async Task InterceptAsync()
        {
            var inner = Substitute.For<Scorpio.DynamicProxy.IInterceptor>();
            _ = inner.InterceptAsync(Arg.Any<Scorpio.DynamicProxy.IMethodInvocation>())
                .Returns(c => c.Arg<Scorpio.DynamicProxy.IMethodInvocation>().ProceedAsync());
            var adapter = new TestCastleAsyncInterceptorAdapter(inner);
            var invocation = Substitute.For<IInvocation>();
            var proceedInfo = Substitute.For<IInvocationProceedInfo>();
            var proceeded = false;

            await adapter.InterceptAsyncPublic(invocation, proceedInfo, (i, p) =>
            {
                proceeded = true;
                return Task.CompletedTask;
            });

            proceeded.ShouldBeTrue();
            _ = inner.Received(1).InterceptAsync(Arg.Any<Scorpio.DynamicProxy.IMethodInvocation>());
        }

        [Fact]
        public async Task InterceptAsync_Generic()
        {
            var inner = Substitute.For<Scorpio.DynamicProxy.IInterceptor>();
            _ = inner.InterceptAsync(Arg.Any<Scorpio.DynamicProxy.IMethodInvocation>())
                .Returns(c => c.Arg<Scorpio.DynamicProxy.IMethodInvocation>().ProceedAsync());
            var adapter = new TestCastleAsyncInterceptorAdapter(inner);
            var invocation = Substitute.For<IInvocation>();
            var proceedInfo = Substitute.For<IInvocationProceedInfo>();

            var result = await adapter.InterceptAsyncPublic<string>(invocation, proceedInfo, (i, p) => Task.FromResult("result"));

            result.ShouldBe("result");
            _ = inner.Received(1).InterceptAsync(Arg.Any<Scorpio.DynamicProxy.IMethodInvocation>());
        }

        private sealed class TestCastleAsyncInterceptorAdapter : CastleAsyncInterceptorAdapter
        {
            public TestCastleAsyncInterceptorAdapter(Scorpio.DynamicProxy.IInterceptor interceptor) : base(interceptor)
            {
            }

            public Task InterceptAsyncPublic(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
                => InterceptAsync(invocation, proceedInfo, proceed);

            public Task<TResult> InterceptAsyncPublic<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
                => InterceptAsync<TResult>(invocation, proceedInfo, proceed);
        }
    }
}
