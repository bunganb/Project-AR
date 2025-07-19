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
            _webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        }

        _webViewObject.Init(
            (msg) => {
                Debug.Log($"Page loaded: {msg}");
            }
        );

        if (isFullScreen)
        {
            _webViewObject.SetMargins(0, 0, 0, 0);
        }
        else
        {
            _webViewObject.SetMargins(50, 150, 50, 50);
        }

        _webViewObject.LoadURL(url);
        
        _webViewObject.SetVisibility(true);
    }

    public void HideWebView()
    {
        if (_webViewObject != null)
        {
            _webViewObject.SetVisibility(false);
        }
    }
}