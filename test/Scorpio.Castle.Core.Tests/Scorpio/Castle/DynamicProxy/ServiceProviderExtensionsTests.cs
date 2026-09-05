using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Scorpio.DynamicProxy;
using Scorpio.DynamicProxy.TestClasses;

using Shouldly;

using Xunit;

namespace Scorpio.Castle.DynamicProxy
{
    public class ServiceProviderExtensionsTests
    {
        [Fact]
        public void GetServiceWithInterfaceProxy()
        {
            var recorder = new RecordingInterceptor();
            var services = new ServiceCollection();
            services.AddTransient<IProxiedService, TestProxiedService>();
            services.AddSingleton(recorder);
            services.AddTransient(typeof(AsyncDeterminationInterceptor<RecordingInterceptor>));
            using var provider = services.BuildServiceProvider();

            var proxy = provider.GetServiceWithInterfaceProxy<IProxiedService, RecordingInterceptor>();

            proxy.ShouldNotBeNull();
            proxy.InterfaceMethod(1, "a");
            recorder.Count.ShouldBe(1);
        }

        [Fact]
        public void GetServiceWithClassProxy()
        {
            var recorder = new RecordingInterceptor();
            var services = new ServiceCollection();
            services.AddTransient<TestProxiedService>();
            services.AddSingleton(recorder);
            services.AddTransient(typeof(AsyncDeterminationInterceptor<RecordingInterceptor>));
            using var provider = services.BuildServiceProvider();

            var proxy = provider.GetServiceWithClassProxy<TestProxiedService, RecordingInterceptor>();

            proxy.ShouldNotBeNull();
            proxy.ProxiedMethod(1, "a");
            recorder.Count.ShouldBe(1);
        }

        private sealed class RecordingInterceptor : IInterceptor
        {
            public int Count { get; private set; }

            public Task InterceptAsync(IMethodInvocation invocation)
            {
                Count++;
                return invocation.ProceedAsync();
            }
        }
    }
}
