using UnityEngine;
using UnityEngine.UI;

public class MySimpleWebView : MonoBehaviour
{
    private WebViewObject webViewObject;

    public void ShowWebView()
    {
        if (webViewObject == null) 
        {
            webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();

            webViewObject.Init(
                (msg) => {
                    Debug.Log($"Page loaded: {msg}");
                },
                err => {
                    Debug.LogError($"WebView error: {err}");
                },
                started => {
                    Debug.Log($"WebView started: {started}");
                },
                hooked => {
                    Debug.Log($"WebView hooked: {hooked}");
                }
            );
        }

        webViewObject.SetMargins(50, 150, 50, 50);
        webViewObject.LoadURL("https://www.youtube.com/embed/6VWxwL-q9O0?autoplay=1&playsinline=1");
        webViewObject.SetVisibility(true);
    }

    public void HideWebView()
    {
        if (webViewObject != null)
        {
            webViewObject.SetVisibility(false);
        }
    }
}