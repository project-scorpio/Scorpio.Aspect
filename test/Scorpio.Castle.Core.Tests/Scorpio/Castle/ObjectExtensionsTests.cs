using System;

using Castle.DynamicProxy;

using Scorpio.DynamicProxy.TestClasses;

using Shouldly;

using Xunit;

namespace Scorpio.Castle
{
    public class ObjectExtensionsTests
    {
        [Fact]
        public void GenerateInterfaceProxy()
        {
            IProxiedService target = new TestProxiedService();
            var interceptor = new RecordingInterceptor();

            var proxy = target.GenerateInterfaceProxy(interceptor);

            proxy.ShouldNotBeNull();
            ReferenceEquals(proxy, target).ShouldBeFalse();
            proxy.InterfaceMethod(1, "a");
            interceptor.Count.ShouldBe(1);
        }

        [Fact]
        public void GenerateClassProxy()
        {
            var target = new TestProxiedService();
            var interceptor = new RecordingInterceptor();

            var proxy = target.GenerateClassProxy(interceptor);

            proxy.ShouldNotBeNull();
            ReferenceEquals(proxy, target).ShouldBeFalse();
            proxy.ProxiedMethod(1, "a");
            interceptor.Count.ShouldBe(1);
        }

        private sealed class RecordingInterceptor : IInterceptor
        {
            public int Count { get; private set; }

            public void Intercept(IInvocation invocation)
            {
                Count++;
                invocation.Proceed();
            }
        }
    }
}
