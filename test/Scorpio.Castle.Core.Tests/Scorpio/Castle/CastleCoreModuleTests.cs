using Microsoft.Extensions.DependencyInjection;

using Scorpio.Castle.DynamicProxy;
using Scorpio.Modularity;

using Shouldly;

using Xunit;

namespace Scorpio.Castle
{
    public class CastleCoreModuleTests
    {
        [Fact]
        public void ConfigureServices()
        {
            var services = new ServiceCollection();
            var module = new CastleCoreModule();

            module.ConfigureServices(new TestConfigureServicesContext(services));

            services.ShouldContainTransient(typeof(AsyncDeterminationInterceptor<>), typeof(AsyncDeterminationInterceptor<>));
        }

        private sealed class TestConfigureServicesContext : ConfigureServicesContext
        {
            public TestConfigureServicesContext(IServiceCollection services) : base(null, services, null)
            {
            }
        }
    }
}
