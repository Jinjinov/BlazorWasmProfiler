namespace BlazorWasmProfiler;

public class Names
{
    public string MethodName { get; set; }
    public string DeclaringTypeName { get; set; }
    public string MethodFullName { get; init; }
    public string CallerClassName { get; set; }
    public string CallerMethodName { get; set; }
    public string CallerFullName { get; init; }
    public string MethodKey { get; init; }

    public Names(string methodName, string declaringTypeName, string callerClassName, string callerMethodName)
    {
        MethodName = methodName;
        DeclaringTypeName = declaringTypeName;
        CallerClassName = callerClassName;
        CallerMethodName = callerMethodName;

        MethodFullName = $"{DeclaringTypeName}.{MethodName}";
        CallerFullName = $"{CallerClassName}.{CallerMethodName}";
        MethodKey = $"{CallerFullName}-{MethodFullName}";
    }
}
