using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Scuti;
using Scuti.GraphQL.Generated;
using Scuti.Net;
using UnityEngine;
using UnityEngine.Networking;

public class ScutiURL
{
    public string SetID;
    public string ViewID;
}

public class ScutiUtils  {
     
    internal static ScutiURL ParseScutiURL(string args)
    {
        if (args == null || !args.Contains("/"))
        {
            ScutiLogger.LogError("UIProxy.Open(string) should have a ViewSet ID and View ID separated by /.  Passed: "+args);
            return null;
        }
        var setID = args.Split('/')[0];
        var viewID = args.Split('/')[1];
        return new ScutiURL { SetID = setID, ViewID = viewID };
    }

    // Bezier
    public static Vector3 GetQuadraticCoordinates(float t, Vector3 p0, Vector3 c0, Vector3 p1)
    {
        return Mathf.Pow(1f - t, 2f) * p0 + 2f * t * (1f - t) * c0 + Mathf.Pow(t, 2f) * p1;
    }

    internal static decimal GetTotalWallet(Wallet wallet)
    {
        return wallet.Promotional.Value + wallet.Purchase.Value;
    }

    public static string FormatPrice(string price)
    {
        var strings = price.Split('.');
        const string SuperscriptDigits =
            "\u2070\u00b9\u00b2\u00b3\u2074\u2075\u2076\u2077\u2078\u2079";

        string superscript = "";
        if(strings.Length > 1)
            superscript = new string(strings[1].Select(x => SuperscriptDigits[x - '0'])
            .ToArray());
        return strings[0] + superscript;
    }

    public static bool IsPortrait()
    {
        float pixelsWide = Camera.main.pixelWidth;
        float pixelsHigh = Camera.main.pixelHeight;

        bool portrait = pixelsHigh > pixelsWide;
        if(ScutiConstants.FORCE_LANDSCAPE)
        {
            portrait = false;
        }

        return portrait;
    }

    public static string StripHTML(string HTMLText)
    {
        Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
        return reg.Replace(HTMLText, "");
    }

    internal static int RequiredAdsPerCategory()
    {
        // TODO: Modify for PC layout and Portrait
        return 12;
    }

    internal static async Task<EncryptedInput> Encrypt(byte[] data)
    {
        var encryption = await ScutiAPI.GetPublicKey();

        var encryptedInput = new EncryptedInput();
        encryptedInput.KeyId = encryption.KeyId;

        var pgp = new Pgp();
        var key64 = Convert.FromBase64String(encryption.PublicKey);
        var decodedPublicKey = Encoding.UTF8.GetString(key64);
        encryptedInput.EncryptedData = Convert.ToBase64String(pgp.Encrypt(data, decodedPublicKey.ToUTF8Bytes()));

        return encryptedInput;
    }

    internal static string HtmlDecode(string text)
    {
        text = WebUtility.HtmlDecode(WebUtility.HtmlDecode(text));
        text = text.Replace("</style>\n", string.Empty);
        var stripped = StripHTML(text);
        return  StripHTML(text);
    }

    internal static string GetSupportEmail()
    {

#if UNITY_EDITOR
        return "mgrossnickle@mindtrust.com";
#elif UNITY_IOS
        return ScutiConstants.MAIL_TO_IOS;
#elif UNITY_ANDROID
        return ScutiConstants.MAIL_TO_ANDROID;
#else
        return ScutiConstants.MAIL_TO_PC;
    
#endif

    }

    internal static bool TryOpenLink(Offer offer)
    {


#if UNITY_IOS

            if (offer.AppleId == null)
            {
                return false;
            }
            else
            {
                ScutiAPI.EngagementWithProductMetric(0, 1, offer.Id.ToString());
                AppstoreHandler.Instance.OpenAppInStore(offer.AppleId);
                return true;
            }

#elif UNITY_ANDROID

            if (offer.AndroidId == null)
            {
                return false;
            }
            else
            {
                ScutiAPI.EngagementWithProductMetric(0, 1, offer.Id.ToString());
                AppstoreHandler.Instance.OpenAppInStore(offer.AndroidId);
                return true;
            }
#else

        if (offer.PcLink == null)
        {
            return false;
        }
        else
        {
            ScutiAPI.EngagementWithProductMetric(0, 1, offer.Id.ToString());
            Application.OpenURL(offer.PcLink);
            return true;
        }
#endif
    }


    internal static string GetSupportInfo(string userInfo)
    {
        return ($"User {userInfo} | Game {ScutiNetClient.Instance.GameId} | Platform {Application.platform} | Device {SystemInfo.deviceModel}");
    }
}
