using System;
using System.Threading.Tasks;

using Castle.DynamicProxy;

using NSubstitute;
using NSubstitute.Extensions;

using Scorpio.DynamicProxy.TestClasses;

using Shouldly;

using Xunit;

namespace Scorpio.Castle.DynamicProxy
{
    public class CastleMethodInvocationAdapterTests
    {
        [Fact]
        public void Arguments()
        {
            var invocation = Substitute.For<IInvocation>();
            invocation.Configure().Arguments.Returns(new object[] { 1, "a" });

            var adapter = new CastleMethodInvocationAdapter(invocation, Substitute.For<IInvocationProceedInfo>(), (i, p) => Task.CompletedTask);

            adapter.Arguments.Length.ShouldBe(2);
            adapter.Arguments[0].ShouldBe(1);
            adapter.Arguments[1].ShouldBe("a");
        }

        [Fact]
        public void ArgumentsDictionary()
        {
            var invocation = Substitute.For<IInvocation>();
            var method = typeof(IProxiedService).GetMethod(nameof(IProxiedService.InterfaceMethod));
            invocation.Configure().MethodInvocationTarget.Returns(method);
            invocation.Configure().Arguments.Returns(new object[] { 1, "a" });

            var adapter = new CastleMethodInvocationAdapter(invocation, Substitute.For<IInvocationProceedInfo>(), (i, p) => Task.CompletedTask);

            adapter.ArgumentsDictionary.Count.ShouldBe(2);
            adapter.ArgumentsDictionary["intValue"].ShouldBe(1);
            adapter.ArgumentsDictionary["stringValue"].ShouldBe("a");
        }

        [Fact]
        public void GenericArguments()
        {
            var invocation = Substitute.For<IInvocation>();
            invocation.Configure().GenericArguments.Returns(new[] { typeof(string) });

            var adapter = new CastleMethodInvocationAdapter(invocation, Substitute.For<IInvocationProceedInfo>(), (i, p) => Task.CompletedTask);

            adapter.GenericArguments.Length.ShouldBe(1);
            adapter.GenericArguments[0].ShouldBe(typeof(string));
        }

        [Fact]
        public void TargetObject_FromInvocationTarget()
        {
            var invocation = Substitute.For<IInvocation>();
            var target = new TestProxiedService();
            invocation.Configure().InvocationTarget.Returns(target);

            var adapter = new CastleMethodInvocationAdapter(invocation, Substitute.For<IInvocationProceedInfo>(), (i, p) => Task.CompletedTask);

            adapter.TargetObject.ShouldBe(target);
        }

        [Fact]
        public void TargetObject_FallbackToMethodInvocationTarget()
        {
            var invocation = Substitute.For<IInvocation>();
            var method = typeof(IProxiedService).GetMethod(nameof(IProxiedService.InterfaceMethod));
            invocation.Configure().MethodInvocationTarget.Returns(method);

            var adapter = new CastleMethodInvocationAdapter(invocation, Substitute.For<IInvocationProceedInfo>(), (i, p) => Task.CompletedTask);

            adapter.TargetObject.ShouldBe(method);
        }

        [Fact]
        public void Method_FromMethodInvocationTarget()
        {
            var invocation = Substitute.For<IInvocation>();
            var method = typeof(IProxiedService).GetMethod(nameof(IProxiedService.InterfaceMethod));
            invocation.Configure().MethodInvocationTarget.Returns(method);

            var adapter = new CastleMethodInvocationAdapter(invocation, Substitute.For<IInvocationProceedInfo>(), (i, p) => Task.CompletedTask);

            adapter.Method.ShouldBe(method);
        }

        [Fact]
        public void Method_FallbackToInvocationMethod()
        {
            var invocation = Substitute.For<IInvocation>();
            var method = typeof(IProxiedService).GetMethod(nameof(IProxiedService.InterfaceMethod));
            invocation.Configure().Method.Returns(method);

            var adapter = new CastleMethodInvocationAdapter(invocation, Substitute.For<IInvocationProceedInfo>(), (i, p) => Task.CompletedTask);

            adapter.Method.ShouldBe(method);
        }

        [Fact]
        public void ReturnValue_GetSet()
        {
            var invocation = Substitute.For<IInvocation>();
            var adapter = new CastleMethodInvocationAdapter(invocation, Substitute.For<IInvocationProceedInfo>(), (i, p) => Task.CompletedTask);

            adapter.ReturnValue = "value";
            adapter.ReturnValue.ShouldBe("value");
        }

        [Fact]
        public async Task ProceedAsync()
        {
            var invocation = Substitute.For<IInvocation>();
            var proceedInfo = Substitute.For<IInvocationProceedInfo>();
            var called = false;
            var adapter = new CastleMethodInvocationAdapter(invocation, proceedInfo, (i, p) =>
            {
                called = true;
                i.ShouldBe(invocation);
                p.ShouldBe(proceedInfo);
                return Task.CompletedTask;
            });

            await adapter.ProceedAsync();

            called.ShouldBeTrue();
        }

        [Fact]
        public async Task ProceedAsync_Generic()
        {
            var invocation = Substitute.For<IInvocation>();
            var proceedInfo = Substitute.For<IInvocationProceedInfo>();
            var adapter = new CastleMethodInvocationAdapter<string>(invocation, proceedInfo, (i, p) => Task.FromResult("result"));

            await adapter.ProceedAsync();

            adapter.ReturnValue.ShouldBe("result");
        }
    }
}
