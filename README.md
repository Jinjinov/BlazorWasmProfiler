# BlazorWasmProfiler

Poor Man's Blazor Wasm Profiler

It uses `AspectInjector` to time the execution of every method in your Blazor WASM project.

It also measures the render time of every Blazor Component that defines these two methods:

        protected override void OnParametersSet()
        {
        }

        protected override void OnAfterRender(bool firstRender)
        {
        }

It does not work with `OnParametersSetAsync()` or `OnAfterRenderAsync(bool firstRender)`

## How to use:

1. Include NuGet package from https://www.nuget.org/packages/BlazorWasmProfiler

        <ItemGroup>
            <PackageReference Include="BlazorWasmProfiler" Version="0.0.0.1" />
        </ItemGroup>

2. Add `[assembly: BlazorTimer]` somewhere in your code.

3. Access statistics:

        var methodStatistics = BlazorTimerAttribute.GetMethodStatistics();

        var renderStatistics = BlazorTimerAttribute.GetRenderStatistics();

3. (optional) If you want you can change

        <Router AppAssembly="@typeof(App).Assembly">

    to

        <Router 
            AppAssembly="@typeof(App).Assembly" 
            AdditionalAssemblies="new[] { typeof(BlazorWasmProfiler.BlazorTimerAttribute).Assembly }">

    then you can use these two pages that display statistics in a table

        <a href="MethodCallStatistics">Method Call Statistics</a>
        <a href="RenderTimeStatistics">Render Time Statistics</a>

## Version history:

- 0.0.0.1:
    - Initial release

## Screenshots:

![MethodCallStatistics](https://raw.githubusercontent.com/Jinjinov/BlazorWasmProfiler/main/MethodCallStatistics.png)

![RenderTimeStatistics](https://raw.githubusercontent.com/Jinjinov/BlazorWasmProfiler/main/RenderTimeStatistics.png)
