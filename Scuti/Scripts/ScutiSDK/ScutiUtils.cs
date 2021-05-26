﻿using System;
using System.Collections;
using System.Collections.Generic;
using Scuti.GraphQL.Generated;
using UnityEngine;

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
            Debug.LogError("UIProxy.Open(string) should have a ViewSet ID and View ID separated by /.  Passed: "+args);
            return null;
        }
        var setID = args.Split('/')[0];
        var viewID = args.Split('/')[1];
        Debug.Log("Open " + setID + " and  " + viewID);
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

    internal static int GetAdsPerPage()
    {
        // TODO: Modify for PC layout and Portrait
        return 6;
    }
}