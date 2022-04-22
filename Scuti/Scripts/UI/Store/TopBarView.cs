﻿using Scuti;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopBarView : View {

    public WalletWidget Wallet;

    [SerializeField] private RectTransform contentBanners;
    [SerializeField] private float thresholdBanners = 30;
    public float maxWidthContent;
    public float widthBanner;

    [Header("Customization")]
    public RectTransform rectBanner;
    public BannerWidget Banner;
    public List<BannerWidget> additionalBanners;
    private bool isAdditionalBanners;

    private void Awake()
    {
        additionalBanners = new List<BannerWidget>();

        widthBanner = rectBanner.sizeDelta.x + thresholdBanners;
        maxWidthContent = contentBanners.rect.size.x;
        int amountBanners = (int)(maxWidthContent / widthBanner);

        if(amountBanners > 1)
        {
            // Instance banners
            for (int i = 0; i < amountBanners - 1; i++)
            {
                BannerWidget banner = Instantiate(Banner, contentBanners.transform);
                banner.gameObject.name = "Banner - "+ (int)(i+1);
                banner.SetIndex(i+amountBanners);
                banner.SecondDelay = 10;
                additionalBanners.Add(banner);
            }
            isAdditionalBanners = true;
        }
    }

    public override void Open()
    {
        base.Open();
        Banner?.Open();
        if(isAdditionalBanners)
        {
            for (int i = 0; i < additionalBanners.Count; i++)
            {
                additionalBanners[i]?.Open();
            }
        }
    }

    public void ResumeBanner()
    {
        ShowBanner(true);
        Banner?.Play();
        if (isAdditionalBanners)
        {
            for (int i = 0; i < additionalBanners.Count; i++)
            {
                additionalBanners[i]?.Play();
            }
        }
    }
    public void PauseBanner()
    {
        Banner?.Pause();
        if (isAdditionalBanners)
        {
            for (int i = 0; i < additionalBanners.Count; i++)
            {
                additionalBanners[i]?.Pause();
            }
        }
    }

    public void ShowBanner(bool value)
    {
        Banner?.gameObject.SetActive(value);
        if (isAdditionalBanners)
        {
            for (int i = 0; i < additionalBanners.Count; i++)
            {
                additionalBanners[i]?.gameObject.SetActive(value);
            }
        }
    }

    public void Refresh()
    {
        if (Wallet)
            Wallet.DoRefresh();
    }
}
