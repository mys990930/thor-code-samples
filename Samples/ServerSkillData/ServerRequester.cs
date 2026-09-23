using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ServerRequester : MonoBehaviour
{
    private const int RequestTimeoutSeconds = 15;
    private const float LockWaitWarningSeconds = 5f;
    private const int ConsecutiveFailureWarningThreshold = 3;

    private static bool _lock = false;
    private static string _lockOwner = "none";
    private static int _consecutiveFailureCount = 0;

#if UNITY_EDITOR
    protected string parentUri = "https://example.invalid";
    //protected string parentUri = "https://example.invalid";
#else
    protected string parentUri = "https://example.invalid";
#endif

    private static RequestResult GetRequestResult(UnityWebRequest request)
    {
        long responseCode = request.responseCode;
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.DataProcessingError)
        {
            return RequestResult.NETWORK_ERROR;
        }

        if (200 <= responseCode && responseCode < 300)
        {
            return RequestResult.SUCCESS;
        }

        if (responseCode >= 500 || responseCode == 0)
        {
            return RequestResult.NETWORK_ERROR;
        }

        return RequestResult.INVALID_VALUE;
    }

    private static bool IsHttpsEndpoint(string requestUrl)
    {
        return Uri.TryCreate(requestUrl, UriKind.Absolute, out Uri endpoint)
            && endpoint.Scheme == Uri.UriSchemeHttps
            && string.IsNullOrEmpty(endpoint.UserInfo);
    }

    private IEnumerator WaitForLock(string requestLabel)
    {
        float waitStartedAt = Time.realtimeSinceStartup;
        float nextWarningAt = LockWaitWarningSeconds;

        while (_lock)
        {
            float waitedSeconds = Time.realtimeSinceStartup - waitStartedAt;
            if (waitedSeconds >= nextWarningAt)
            {
                Debug.LogWarning(
                    $"Waiting {waitedSeconds:F1}s for network lock. Active request: {_lockOwner}. Pending request: {requestLabel}");
                nextWarningAt += LockWaitWarningSeconds;
            }

            yield return null;
        }

        _lock = true;
        _lockOwner = requestLabel;
    }

    private void ConfigureRequest(UnityWebRequest request, bool includeAuthHeader, string contentType = null)
    {
        request.timeout = RequestTimeoutSeconds;
        // HTTPS 요청이 HTTP로 자동 리디렉션되는 것을 허용하지 않는다.
        request.redirectLimit = 0;

        if (!string.IsNullOrEmpty(contentType))
        {
            request.SetRequestHeader("Content-Type", contentType);
        }

        if (includeAuthHeader)
        {
            request.SetRequestHeader("Authorization", "Bearer " + NetworkManager.instance.token);
        }
    }

    private void LogRequestOutcome(string method, UnityWebRequest request, RequestResult result)
    {
        if (result == RequestResult.SUCCESS)
        {
            _consecutiveFailureCount = 0;
            Debug.Log($"{method} successful. ResponseCode: {request.responseCode}");
            return;
        }

        _consecutiveFailureCount += 1;

        Debug.LogWarning($"{method} failed. Result: {result}, ResponseCode: {request.responseCode}");

        if (_consecutiveFailureCount >= ConsecutiveFailureWarningThreshold)
        {
            Debug.LogWarning(
                $"Network request failures have occurred {_consecutiveFailureCount} times in a row. Review backend health and request flow.");
        }
    }

    private IEnumerator ExecuteLockedRequest(
        string method,
        Func<UnityWebRequest> requestFactory,
        Action<UnityWebRequest, RequestResult> callback)
    {
        yield return StartCoroutine(WaitForLock(method));

        UnityWebRequest request = null;
        try
        {
            request = requestFactory();
            if (!IsHttpsEndpoint(request.url))
            {
                Debug.LogError($"{method} blocked: HTTPS without embedded credentials is required.");
                LogRequestOutcome(method, request, RequestResult.NETWORK_ERROR);
                callback?.Invoke(request, RequestResult.NETWORK_ERROR);
                yield break;
            }

            yield return request.SendWebRequest();

            RequestResult result = GetRequestResult(request);
            LogRequestOutcome(method, request, result);
            callback?.Invoke(request, result);
        }
        finally
        {
            request?.Dispose();
            _lockOwner = "none";
            _lock = false;
        }
    }

    public IEnumerator SendGetRequestWithoutToken(string targetUri, Action<UnityWebRequest> callback)
    {
        yield return StartCoroutine(ExecuteLockedRequest("GET", () =>
        {
            UnityWebRequest request = UnityWebRequest.Get(uri: parentUri + targetUri);
            ConfigureRequest(request, includeAuthHeader: false, contentType: "application/json");
            return request;
        }, (request, result) =>
        {
            callback?.Invoke(request);
        }));
    }

    public IEnumerator SendGetRequest(string targetUri, Action<string> callback)
    {
        yield return StartCoroutine(ExecuteLockedRequest("GET", () =>
        {
            UnityWebRequest request = UnityWebRequest.Get(uri: parentUri + targetUri);
            ConfigureRequest(request, includeAuthHeader: true, contentType: "application/json");
            return request;
        }, (request, result) =>
        {
            if (result == RequestResult.SUCCESS)
            {
                callback?.Invoke(request.downloadHandler.text);
            }
        }));
    }

    public IEnumerator SendGetRequest(string targetUri, Action<RequestResult, string> callback)
    {
        yield return StartCoroutine(ExecuteLockedRequest("GET", () =>
        {
            UnityWebRequest request = UnityWebRequest.Get(uri: parentUri + targetUri);
            ConfigureRequest(request, includeAuthHeader: true, contentType: "application/json");
            return request;
        }, (request, result) =>
        {
            callback?.Invoke(result, request.downloadHandler?.text);
        }));
    }

    public IEnumerator SendGetRequest<T>(string targetUri, Func<string, T> mapResponse, Action<RequestResult, T> callback)
    {
        yield return StartCoroutine(SendGetRequest(targetUri, (RequestResult result, string body) =>
        {
            T model = default;
            if (result == RequestResult.SUCCESS)
            {
                try
                {
                    model = mapResponse(body);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"Invalid response for GET: {exception.GetType().Name}");
                    result = RequestResult.INVALID_VALUE;
                }
            }

            callback?.Invoke(result, model);
        }));
    }

    public IEnumerator SendGetRequest(string targetUri, Action<int> callback)
    {
        yield return StartCoroutine(ExecuteLockedRequest("GET", () =>
        {
            UnityWebRequest request = UnityWebRequest.Get(uri: parentUri + targetUri);
            ConfigureRequest(request, includeAuthHeader: true, contentType: "application/json");
            return request;
        }, (request, result) =>
        {
            callback?.Invoke(result == RequestResult.NETWORK_ERROR ? 0 : (int)request.responseCode);
        }));
    }

    public IEnumerator SendLoginRequest(WWWForm form, Action<string, string> callback)
    {
#if UNITY_EDITOR
        string targetUri = "/test/login";
#else
        string targetUri = "/user/login";
#endif
        yield return StartCoroutine(SendLoginRequest(targetUri, form, callback));
    }

    public IEnumerator SendLoginRequest(string uri, WWWForm form, Action<string, string> callback)
    {
        yield return StartCoroutine(ExecuteLockedRequest("POST", () =>
        {
            UnityWebRequest request = UnityWebRequest.Post(uri: parentUri + uri, form);
            ConfigureRequest(request, includeAuthHeader: false, contentType: "application/x-www-form-urlencoded");
            return request;
        }, (request, result) =>
        {
            if (result == RequestResult.NETWORK_ERROR)
            {
                callback?.Invoke("500", "");
                return;
            }

            callback?.Invoke(request.responseCode.ToString(), request.downloadHandler.text);
        }));
    }

    public IEnumerator SendPostRequest(string targetUri, string body, Action<string> callback)
    {
        yield return StartCoroutine(ExecuteLockedRequest("POST", () =>
        {
            UnityWebRequest request = UnityWebRequest.Post(uri: parentUri + targetUri, postData: body, "application/json");
            ConfigureRequest(request, includeAuthHeader: true);
            return request;
        }, (request, result) =>
        {
            if (result == RequestResult.SUCCESS)
            {
                callback?.Invoke(request.downloadHandler.text);
            }
        }));
    }

    public IEnumerator SendPostRequest(string targetUri, Action<string> callback)
    {
        yield return StartCoroutine(SendPostRequest(targetUri, "", callback));
    }

    public IEnumerator SendPostRequest(string targetUri, Action<int> callback)
    {
        yield return StartCoroutine(ExecuteLockedRequest("POST", () =>
        {
            UnityWebRequest request = UnityWebRequest.PostWwwForm(uri: parentUri + targetUri, "application/json");
            ConfigureRequest(request, includeAuthHeader: true);
            return request;
        }, (request, result) =>
        {
            callback?.Invoke(result == RequestResult.NETWORK_ERROR ? 0 : (int)request.responseCode);
        }));
    }

    public IEnumerator SendPostRequest(string targetUri, string body, Action<UnityWebRequest> callback)
    {
        yield return StartCoroutine(ExecuteLockedRequest("POST", () =>
        {
            UnityWebRequest request = UnityWebRequest.Post(uri: parentUri + targetUri, postData: body, "application/json");
            ConfigureRequest(request, includeAuthHeader: true);
            return request;
        }, (request, result) =>
        {
            callback?.Invoke(request);
        }));
    }

    public IEnumerator SendPostRequest(string targetUri, string body, Action<RequestResult, UnityWebRequest> callback)
    {
        yield return StartCoroutine(ExecuteLockedRequest("POST", () =>
        {
            UnityWebRequest request = UnityWebRequest.Post(uri: parentUri + targetUri, postData: body, "application/json");
            ConfigureRequest(request, includeAuthHeader: true);
            return request;
        }, (request, result) =>
        {
            callback?.Invoke(result, request);
        }));
    }

    public IEnumerator SendPutRequest(string targetUri, string body, Action<RequestResult> callback)
    {
        yield return StartCoroutine(ExecuteLockedRequest("PUT", () =>
        {
            UnityWebRequest request = UnityWebRequest.Put(uri: parentUri + targetUri, body);
            ConfigureRequest(request, includeAuthHeader: true, contentType: "application/json");
            return request;
        }, (request, result) =>
        {
            callback?.Invoke(result);
        }));
    }

    public IEnumerator SendPutRequest(string targetUri, string body, Action<string> callback)
    {
        yield return StartCoroutine(ExecuteLockedRequest("PUT", () =>
        {
            UnityWebRequest request = UnityWebRequest.Put(uri: parentUri + targetUri, body);
            ConfigureRequest(request, includeAuthHeader: true, contentType: "application/json");
            return request;
        }, (request, result) =>
        {
            if (result == RequestResult.SUCCESS)
            {
                callback?.Invoke(request.downloadHandler.text);
            }
        }));
    }

    public IEnumerator SendPutRequest(string targetUri, string body, Action<RequestResult, string> callback)
    {
        yield return StartCoroutine(ExecuteLockedRequest("PUT", () =>
        {
            UnityWebRequest request = UnityWebRequest.Put(uri: parentUri + targetUri, body);
            ConfigureRequest(request, includeAuthHeader: true, contentType: "application/json");
            return request;
        }, (request, result) =>
        {
            callback?.Invoke(result, request.downloadHandler?.text);
        }));
    }
}
