using Scuti;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class TopBarView : View {

    public WalletWidget Wallet;

    [Header("Customization")]
    public BannerWidget Banner;

    public override void Open()
    {
        base.Open();
        Banner?.Open();
    }

    public void ResumeBanner()
    {
        ShowBanner(true);
        Banner?.Play();
    }
    public void PauseBanner()
    {
        Banner?.Pause();
    }

    public void ShowBanner(bool value)
    {
        Banner?.gameObject.SetActive(value);
    }

    public void Refresh()
    {
        if (Wallet)
            Wallet.DoRefresh();
    }
}
