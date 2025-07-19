using UnityEngine;
using UnityEngine.UI;

public class MySimpleWebView : MonoBehaviour
{
    private WebViewObject webViewObject;

    public void ShowWebView()
    {
        // If the WebView object already exists, just show it
        if (webViewObject != null)
        {
            webViewObject.SetVisibility(true);
            return;
        }

        // Initialize a new WebView object
        webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        webViewObject.Init(
            // Callback action when the page finishes loading
            (msg) => {
                Debug.Log($"Page loaded: {msg}");
            }
        );

        // Set the margins (left, top, right, bottom) in pixels
        // This makes the webview appear below the status bar and above a bottom banner if you have one
        webViewObject.SetMargins(50, 150, 50, 50);

        // Load the URL
        webViewObject.LoadURL("https://www.youtube.com/watch?v=6VWxwL-q9O0");
    }

    public void HideWebView()
    {
        if (webViewObject != null)
        {
            webViewObject.SetVisibility(false);
        }
    }
}