using Scuti;
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

    [SerializeField] private GameObject btnFirstSelection;

    [Header("Customization")]
    public RectTransform rectBanner;
    public BannerWidget Banner;
    public List<BannerWidget> additionalBanners;
    private bool isAdditionalBanners;

    
    protected override void Awake()
    {
        base.Awake();
        // Only the default banner is enrolled.        
        Banner.onCreateBanners += CreateBanners;

        Scuti.UI.UIManager.SetFirstSelected(btnFirstSelection);
    }

    private void CreateBanners(int offerCount)
    {
        additionalBanners = new List<BannerWidget>();

        if (offerCount < 2) return;

        widthBanner = rectBanner.sizeDelta.x + thresholdBanners;
        maxWidthContent = contentBanners.rect.size.x;
        int bannerCount = (int)(maxWidthContent / widthBanner);

        if (bannerCount > offerCount) bannerCount = offerCount;

        if (bannerCount > 1)
        {
            int increment = offerCount / bannerCount;
            if (increment < 1) increment = 1;

            // Instance banners
            for (int i = 0; i < bannerCount - 1; i++)
            {
                BannerWidget banner = Instantiate(Banner, contentBanners.transform);
                banner.gameObject.name = "Banner - " + (int)(i + 1);
                //It is added 1 because the default banner is already instantiated with index zero.
                banner.SetIndex(((i+1)* + increment >= offerCount)? offerCount - 1: (i+1)*increment);
                banner.SecondDelay = 10;
                additionalBanners.Add(banner);
            }
            isAdditionalBanners = true;
        }

        Banner.onCreateBanners -= CreateBanners;
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
