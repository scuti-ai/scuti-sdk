using Scuti.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ShareDialog
{
    /// <summary>
    /// Shares a link using the phone's native sharing functionality
    /// </summary>
    /// <param name="url"></param>
    public static void Share(string url)
    { 
        ScutiAPI.SocializationOfBrandMetric(url, "mobile"); 
#if UNITY_IOS || UNITY_ANDROID
        new NativeShare()
            .SetTitle("Scuti Store")
            .SetSubject("Check out Scuti!")
            .SetText(url)
            .Share();
#else
        ShareTwitterPC(url);
        //new NativeShare()
        //    .SetTitle("Share")
        //    .SetSubject("Share")
        //    .SetText(url)
        //    .Share();
# endif
    }

    public static void ShareTwitterPC(string url)
    { 
        ScutiAPI.SocializationOfBrandMetric(url, "twitter"); 
        Application.OpenURL("https://twitter.com/intent/tweet?url=" + url);
    }

    public static void ShareFacebookPC(string appId, string url)
    { 
        ScutiAPI.SocializationOfBrandMetric(url, "facebook"); 
        Application.OpenURL("https://www.facebook.com/dialog/share?app_id=" + appId + "&href=" + url);
    }
}