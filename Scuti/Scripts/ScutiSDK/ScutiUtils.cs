using System;
using System.Collections;
using System.Collections.Generic;
using Scuti;
using Scuti.GraphQL.Generated;
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

    internal static int RequiredAdsPerCategory()
    {
        // TODO: Modify for PC layout and Portrait
        return 12;
    }

}
