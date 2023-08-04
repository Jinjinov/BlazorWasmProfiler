# BlazorWasmProfiler

Poor Man's Blazor Wasm Profiler

It uses `AspectInjector` to time the execution of every method in your Blazor WASM project.

It also measures the render time of every Blazor Component that defines these methods:

        protected override void OnParametersSet()
        // or
        protected override async Task OnParametersSetAsync()

        // and

        protected override void OnAfterRender(bool firstRender)
        // or
        protected override async Task OnAfterRenderAsync(bool firstRender)

## How to use:

1. Include NuGet package from https://www.nuget.org/packages/BlazorWasmProfiler

        <ItemGroup>
            <PackageReference Include="BlazorWasmProfiler" Version="0.0.1.0" />
        </ItemGroup>

2. Add the attribute somewhere in your code

        [assembly: BlazorTimer]

        [assembly: MethodTimer]
        [assembly: RenderTimer]

3. Access statistics:

        var methodStatistics = BlazorTimerAttribute.GetMethodStatistics();
        var renderStatistics = BlazorTimerAttribute.GetRenderStatistics();

        var methodStatistics = ExecutionStatistics.GetMethodStatistics();
        var renderStatistics = ExecutionStatistics.GetRenderStatistics();

4. (optional) Use `MethodCallStatistics` and `RenderTimeStatistics` as components

        <BlazorWasmProfiler.MethodCallStatistics />
        <BlazorWasmProfiler.RenderTimeStatistics />

5. (optional) Change

        <Router AppAssembly="@typeof(App).Assembly">

    to

        <Router 
            AppAssembly="@typeof(App).Assembly" 
            AdditionalAssemblies="new[] { typeof(BlazorWasmProfiler.BlazorTimerAttribute).Assembly }">

    and use `MethodCallStatistics` and `RenderTimeStatistics` as pages

        <a href="MethodCallStatistics">Method Call Statistics</a>
        <a href="RenderTimeStatistics">Render Time Statistics</a>

## Version history:

- 0.0.1.0:
    - Excluded the body of `OnParametersSet()` and `OnAfterRender(bool firstRender)` from render timing
    - Render timing now works with `OnParametersSetAsync()` and `OnAfterRenderAsync(bool firstRender)`
    - Added `enum StatisticsOrder` to get statistics ordered by any property
- 0.0.0.1:
    - Initial release

## Screenshots:

![MethodCallStatistics](https://raw.githubusercontent.com/Jinjinov/BlazorWasmProfiler/main/MethodCallStatistics.png)

![RenderTimeStatistics](https://raw.githubusercontent.com/Jinjinov/BlazorWasmProfiler/main/RenderTimeStatistics.png)
