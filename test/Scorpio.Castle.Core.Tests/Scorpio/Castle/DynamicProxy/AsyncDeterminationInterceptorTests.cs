using Scorpio.DynamicProxy.TestClasses;

using Shouldly;

using Xunit;

namespace Scorpio.Castle.DynamicProxy
{
    public class AsyncDeterminationInterceptorTests
    {
        [Fact]
        public void Create()
        {
            new AsyncDeterminationInterceptor<TestInterceptor>(new TestInterceptor()).ShouldNotBeNull();
        }
    }
}
