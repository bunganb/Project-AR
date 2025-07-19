using UnityEngine;
using UnityEngine.UI;

public class WebViewWrapper : MonoBehaviour
{
    public static WebViewWrapper Instance;
    
    private WebViewObject _webViewObject;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowWebView(string url, bool isFullScreen = false)
    {
        if (_webViewObject != null)
        {
            _webViewObject.SetVisibility(true);
            return;
        }

        // Initialize a new WebView object
        _webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        _webViewObject.Init(
            // Callback action when the page finishes loading
            (msg) => {
                Debug.Log($"Page loaded: {msg}");
            }
        );

        // Set the margins (left, top, right, bottom) in pixels
        // This makes the webview appear below the status bar and above a bottom banner if you have one
        if (isFullScreen)
        {
            _webViewObject.SetMargins(0, 0, 0, 0);
        }
        else
        {
            _webViewObject.SetMargins(50, 150, 50, 50);
        }

        // Load the URL
        _webViewObject.LoadURL(url);
    }

    public void HideWebView()
    {
        if (_webViewObject != null)
        {
            _webViewObject.SetVisibility(false);
        }
    }
}