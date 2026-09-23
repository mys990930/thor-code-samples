using UnityEngine;

// Shared boot failure state for required static resources and server data.
public static class StaticDataInitGuard
{
    public static bool HasFailed { get; private set; }
    public static string FailureMessage { get; private set; } = string.Empty;

    public static void Reset()
    {
        HasFailed = false;
        FailureMessage = string.Empty;
    }

    public static void Fail(string message)
    {
        if (HasFailed)
        {
            return;
        }

        HasFailed = true;
        FailureMessage = message;
        Debug.LogError(message);
    }

    public static bool RequireSuccess(string owner, RequestResult result)
    {
        if (result != RequestResult.SUCCESS)
        {
            Fail($"{owner} failed to load required server data. Result: {result}");
        }
        return !HasFailed;
    }

    public static void ApplyRequiredData(string owner, RequestResult result, System.Action apply)
    {
        if (!RequireSuccess(owner, result)) return;

        try
        {
            apply();
        }
        catch (System.Exception exception)
        {
            Fail($"{owner} could not initialize required data: {exception.GetType().Name}");
        }
    }

    public static T LoadRequiredResource<T>(string owner, string path) where T : Object
    {
        T asset = Resources.Load<T>(path);
        if (asset == null)
        {
            Fail($"{owner} failed to load required resource '{path}' ({typeof(T).Name}).");
        }

        return asset;
    }
}
