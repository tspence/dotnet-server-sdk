using System;
using Wasmtime;

namespace DevCycle.SDK.Server.Local.Api;

public class LocalBucketingOverrides
{
    public Action<Caller, int, int, int, int> AbortCallback;
    public Action<Caller, int> ConsoleLogCallback;
    public Func<Caller, double> DateNowCallback;
    public Func<Caller, double> SeedCallback;
}