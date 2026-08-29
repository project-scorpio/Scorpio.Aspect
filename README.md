# Scorpio.Aspect

Scorpio.Aspect 是 [Scorpio.Core](https://github.com/project-scorpio/Scorpio.Core) 框架的 **AOP / 动态代理具体实现**。它在 Scorpio.Core 提供的拦截器抽象（`IInterceptor` / `IMethodInvocation` / `IProxyTargetProvider` 等）之上，接入具体代理库，让框架可以真正生成代理对象并执行拦截逻辑。

## 项目结构

```text
Scorpio.Aspect/
├── src/
│   ├── Scorpio.AspectCore/     # 基于 AspectCore 的代理实现与属性注入
│   ├── Scorpio.Castle.Core/    # 基于 Castle.Core 的代理实现
│   └── Scorpio.Autofac/        # 基于 Autofac 的容器适配与代理实现
└── test/
    ├── Scorpio.TestBase/         # 测试基类（IntegratedTest 等，本地副本）
    ├── Scorpio.Aspect.TestBase/  # Aspect 相关测试的共享基础设施
    ├── Scorpio.AspectCore.Tests/ # Scorpio.AspectCore 的单元测试
    └── Scorpio.Autofac.Tests/    # Scorpio.Autofac 的单元测试
```

| 项目 | 说明 |
| --- | --- |
| `Scorpio.AspectCore` | 基于 [AspectCore](https://github.com/dotnetcore/AspectCore-Framework) 的动态代理；提供 `UseAspectCore()` 扩展、约定式拦截器接入以及面向 `AspectCore` 服务解析的属性注入。 |
| `Scorpio.Castle.Core` | 基于 [Castle.Core](http://www.castleproject.org/) 的动态代理；提供 `CastleCoreModule` 和 `AsyncDeterminationInterceptor<TInterceptor>` 等类型。 |
| `Scorpio.Autofac` | 基于 [Autofac](https://autofac.org/) 的 `IServiceProviderFactory<ContainerBuilder>` 容器适配与动态代理；提供 `UseAutofac()` 扩展，复用 `Scorpio.Castle.Core` 的拦截器适配。 |

## 依赖关系

三个实现项目均依赖 `Scorpio` NuGet 包（AOP 抽象与模块系统），版本由根目录
`Directory.Packages.props` 集中管理（当前 `0.1.3`）。测试基类 `Scorpio.TestBase` 已内联到本仓库，
同样依赖 `Scorpio` 包，因此本仓库不再直接引用 `Scorpio.Core` 源码。

## 构建、测试与打包

```powershell
dotnet restore Scorpio.Aspect.slnx
dotnet build Scorpio.Aspect.slnx -c Release --no-restore
dotnet test Scorpio.Aspect.slnx -c Release --no-build

# 按项目打包，输出到 artifacts
dotnet pack src/Scorpio.AspectCore/Scorpio.AspectCore.csproj -c Release -o artifacts --no-build
dotnet pack src/Scorpio.Castle.Core/Scorpio.Castle.Core.csproj -c Release -o artifacts --no-build
dotnet pack src/Scorpio.Autofac/Scorpio.Autofac.csproj -c Release -o artifacts --no-build
```

## 键控服务（Keyed Services）说明

`Scorpio.Autofac` 的容器适配在从 `IServiceCollection` 填充 Autofac 容器时，会**跳过键控服务描述符**，避免把键控服务误注册为普通服务。键控服务的解析能力由宿主/默认容器负责，Autofac 路径只保证普通依赖注入不被破坏。

## 许可证

本项目使用 MIT 许可证，详见 [LICENSE](LICENSE)。
