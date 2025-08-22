using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MySampleWebView : MonoBehaviour
{
    public string Url;
    public RectTransform panelRect; 

    WebViewObject webViewObject;

    public void OpenWebView()
    {
        StartCoroutine(ShowWebView());
    }

    IEnumerator ShowWebView()
    {
        if (webViewObject == null)
        {
            webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
            webViewObject.Init();

            while (!webViewObject.IsInitialized())
                yield return null;

            Vector3[] corners = new Vector3[4];
            panelRect.GetWorldCorners(corners);

            int left = (int)corners[0].x;
            int bottom = (int)corners[0].y;
            int right = Screen.width - (int)corners[2].x;
            int top = Screen.height - (int)corners[2].y;

            webViewObject.SetMargins(left, top, right, bottom);
        }

        webViewObject.LoadURL(Url.Replace(" ", "%20"));
        webViewObject.SetVisibility(true);
    }

    public void CloseWebView()
    {
        if (webViewObject != null)
        {
            webViewObject.SetVisibility(false);
        }
    }
}