# Embedded Domain Specific Languages

Thus far, we've looked at the building blocks of computation and query expressions mostly from the perspective of the purpose for which they were created. Now we are going to look at examples that highlight a capability they enable, specifically embedded domain specific languages, or eDSLs.

We observed in the Extensions section that `CustomOperation`s could be used to create any new syntax you desire. In `Queries` we looked into how each of the `CustomOperation` attribute options could be applied to build query operators. As these are the intended uses, they are easy to review and teach. EDSLs, are specific to each individual use case, so the best way to learn from them is to review their source code.

We'll look at the following open source projects and how they leverage `CustomOperation`s to achieve their objectives:

1. NuGet
2. IL
3. Saturn
4. Freya

We'll also take a look at how Freya achieves its `CustomOperation` overloading, which is not supposed to be possible, as we saw at the end of the last section.

## NuGet

These NuGet builders were defined in the article [Embedded Domain Specific Languages in F# Using Custom Operations](http://www.readcopyupdate.com/blog/2014/10/10/edsls-using-custom-operations.html), describing the potential for `CustomOperation`s to be used to create eDSLs.

* [`PackageDefinitionBuilder`](https://github.com/flashcurd/NugetDsl/blob/master/NuGetDsl/Dsl.fs#L127-L198)
* [`NuGetDefinitionBuilder`](https://github.com/flashcurd/NugetDsl/blob/master/NuGetDsl/Dsl.fs#L127-L198)

> **Observation:** these are _very_ simple implementations with _no_ standard CE methods and no use of `CustomOperation` attribute parameters.

## IL

Generating IL at runtime is something of an artform, but rarely do .NET developers write literal IL to do so. The following projects allow F# developers to do almost exactly that, embedded _within_ their F# programs.

* [ILBuilder](https://github.com/kbattocchi/ILBuilder/blob/master/ILBuilder.fs) uses a Type Provider _within_ the builder definition
* [ILBuilder](https://github.com/lisovin/ILBuilder/blob/master/ILBuilder/Builders.fs) leverages Type Providers _when using_ the builder
* [LicenseToCIL](https://github.com/rspeele/LicenseToCIL/blob/master/LicenseToCIL/CILBuilder.fs) uses `Reflection.Emit`

## Saturn



## Freya

[Freya](https://freya.io/) is another web _stack_ that provides a functional-first, type-safe approach to building web applications on top of the [OWIN](http://owin.org/) spec. Freya provides additional application layers that add types to HTTP, provides routing using [URI Templates](https://tools.ietf.org/html/rfc6570), and a `webmachine`-style model inspired by the [Erlang project](https://github.com/webmachine/webmachine), which implements the HTTP state machine described in the HTTP RFCs.

### `Freya` = `AsyncState`

[`FreyaBuilder`](https://github.com/xyncro/freya-core/blob/master/src/Freya.Core/Expression.fs), where [`Freya`](https://github.com/xyncro/freya-core/blob/master/src/Freya.Core/Core.fs#L25-L61) is a `State -> Async<'a * State>` (for a pre-defined `State` type)

### Routers

[`UriTemplateRouterBuilder`](https://github.com/xyncro/freya-routers/blob/master/src/Freya.Routers.Uri.Template/Expression.fs) inherits from [`ConfigurationBuilder`](https://github.com/xyncro/freya-core/blob/master/src/Freya.Core/Configuration.fs)

### Machines



### Overloading



## Review


