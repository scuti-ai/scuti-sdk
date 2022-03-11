using Scuti;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class TopBarView : View {

    public WalletWidget Wallet;

    [Header("Customization")]
    [SerializeField] protected Image bannerImage;
    public BannerWidget Banner;

    public override void Open()
    {
        Debug.Log("OPEN BANNER");
        //var first = (firstOpen);
        //if (first)
        //{
        //    Scuti.UI.UIManager.ShowLoading(true);
        //}
        //else
        //{
            Banner.Play();
        //}
        Banner.Open();
    }

    protected virtual void PauseAds()
    {
        Banner.Pause();
    }

    public void Refresh()
    {
        if (Wallet)
            Wallet.DoRefresh();
    }
}
