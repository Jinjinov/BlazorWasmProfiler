# BlazorWasmProfiler

Poor Man's Blazor Wasm Profiler

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
