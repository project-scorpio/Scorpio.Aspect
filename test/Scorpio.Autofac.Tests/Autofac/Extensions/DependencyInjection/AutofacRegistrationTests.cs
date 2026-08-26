
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Scorpio.DependencyInjection.TestClasses;
using Scorpio.DynamicProxy.TestClasses;
using Scorpio.Modularity;

using Shouldly;

using Xunit;

namespace Autofac.Extensions.DependencyInjection
{
    public class AutofacRegistrationTests
    {
        [Fact]
        public void Register_Type()
        {
            var services = new ServiceCollection();
            services.AddTransient<IProxiedService, TestProxiedService>();
            services.AddSingleton(Substitute.For<IModuleContainer>());
            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.Build().Resolve<IProxiedService>().ShouldBeOfType<TestProxiedService>();
        }

        [Fact]
        public void Register_Instance()
        {
            var services = new ServiceCollection();
            services.AddTransient<IProxiedService, TestProxiedService>();
            var module = Substitute.For<IModuleContainer>();
            services.AddSingleton(module);
            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.Build().Resolve<IModuleContainer>().ShouldBe(module);
        }


        [Fact]
        public void Register_Generic()
        {
            var services = new ServiceCollection();
            services.AddTransient(typeof(IGenericService<>), typeof(GenericService<>));
            services.AddSingleton(Substitute.For<IModuleContainer>());
            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.Build().Resolve<IGenericService<string>>().ShouldBeOfType<GenericService<string>>();
        }

        [Fact]
        public void Register_Factory()
        {
            var services = new ServiceCollection();
            services.AddTransient<IProxiedService>(sp => new TestProxiedService());
            services.AddSingleton(Substitute.For<IModuleContainer>());
            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.Build().Resolve<IProxiedService>().ShouldBeOfType<TestProxiedService>();
        }

        [Fact]
        public void Register_KeyedService_IsNotRegisteredAsOrdinary()
        {
            var services = new ServiceCollection();
            services.AddSingleton(Substitute.For<IModuleContainer>());
            services.AddTransient<IProxiedService, TestProxiedService>();
            services.AddKeyedSingleton<IProxiedService, TestProxiedService>("keyed");

            var builder = new ContainerBuilder();
            builder.Populate(services);

            // 键控服务由宿主容器处理，不应被 Autofac 当作普通服务注册，
            // 因此普通解析只会得到一个实现。
            builder.Build().Resolve<IEnumerable<IProxiedService>>().ShouldHaveSingleItem();
        }

        [Fact]
        public void Register_LifeTime_Singleton()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IProxiedService, TestProxiedService>();
            services.AddSingleton(Substitute.For<IModuleContainer>());
            var builder = new ContainerBuilder();
            builder.Populate(services);
            var provider = builder.Build();
            var exp = provider.Resolve<IProxiedService>().ShouldBeOfType<TestProxiedService>();
            var act = provider.Resolve<IProxiedService>().ShouldBeOfType<TestProxiedService>();
            act.ShouldBe(exp);
        }

        [Fact]
        public void Register_LifeTime_Transient()
        {
            var services = new ServiceCollection();
            services.AddTransient<IProxiedService, TestProxiedService>();
            services.AddSingleton(Substitute.For<IModuleContainer>());
            var builder = new ContainerBuilder();
            builder.Populate(services);
            var provider = builder.Build();
            var exp = provider.Resolve<IProxiedService>().ShouldBeOfType<TestProxiedService>();
            var act = provider.Resolve<IProxiedService>().ShouldBeOfType<TestProxiedService>();
            act.ShouldNotBe(exp);
        }

        [Fact]
        public void Register_LifeTime_Scoped()
        {
            var services = new ServiceCollection();
            services.AddScoped<IProxiedService, TestProxiedService>();
            services.AddSingleton(Substitute.For<IModuleContainer>());
            var builder = new ContainerBuilder();
            builder.Populate(services);
            var provider = builder.Build();
            var exp = provider.Resolve<IProxiedService>().ShouldBeOfType<TestProxiedService>();
            using (var scope= provider.BeginLifetimeScope())
            {
                var act = scope.Resolve<IProxiedService>().ShouldBeOfType<TestProxiedService>();
                act.ShouldNotBe(exp);
                scope.Resolve<IProxiedService>().ShouldBeOfType<TestProxiedService>().ShouldBe(act);
            }
        }
    }
}
