﻿using Scuti;

using Scuti.GraphQL.Generated;
using Scuti.Net;
using Scuti.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BannerWidget : View {

    public UnityEngine.UI.Image BannerImage;
    public int SecondDelay;

    private bool _paused;
    private bool _loading;
    private float _timePassed;
    private int _index = 0;
    private int _prevIndex;
    private Offer _banner;

    public RoundedImage Rounded;

    public System.Action<int> onCreateBanners;

    protected bool timerCompleted = false;
    [SerializeField] protected Timer timer;


    protected override void Awake()
    {
        base.Awake();
        BannerImage.enabled = false;

        timer.Pause();
        timer.onFinished.AddListener(OnImpressionCompleted);
    }

    private void OnImpressionCompleted()
    {
        if (_banner != null)
        {
            ScutiAPI.RecordOfferImpression(_banner.Id.ToString());
        }
    }

    public void Start()
    {
        Rotate();
    }

    public void Pause()
    {
        _paused = true;
        if (!_destroyed && timer != null)
        {
            timer.Pause();
        }
    }

    public void Play()
    {
        _paused = false;
        if(Rounded != null) Rounded.Refresh();

        if (!_destroyed && timer != null)
        {
            ResetTimer();
        }
    }


    public void ResetTimer()
    {
        if (!_destroyed)
        {
            timerCompleted = false;
            timer.ResetTime(ScutiConstants.SCUTI_VALID_IMPRESSION_DURATION);
            timer.Begin();
        }
    }




    private void Update()
    {
        if (!_paused && !_loading)
        {
            _timePassed += Time.deltaTime;
            if (_timePassed > SecondDelay)
            {
                Rotate();
            }
        }
    }

    public void OnClick()
    {
        if (_banner != null)
        {
            if (string.IsNullOrEmpty(_banner.Media.VideoUrl))
            {
                ShowOfferDetails(false);
            }
            else if (_banner.Media.VideoUrl.StartsWith(ScutiConstants.INTERNAL_URL_PREFIX))
            {
                ScutiAPI.EngagementWithProductMetric(0, 1, _banner.Id.ToString());
                var url = _banner.Media.VideoUrl.Substring(ScutiConstants.INTERNAL_URL_PREFIX.Length);
                if (url.ToLower().StartsWith("http"))
                {
#if UNITY_ANDROID || UNITY_IOS
                    UIManager.WebForm.Url = url;
                    UIManager.WebForm.Open();
#else
                    Application.OpenURL(url);
#endif
                }
                else
                {

                    var pageUrl = ScutiUtils.ParseScutiURL(url);
                    if (pageUrl != null)
                    {
                        UIManager.Open(pageUrl.SetID, pageUrl.ViewID);
                    }
                }
            }
            else
            {
                ShowOfferDetails(true);
            }
        

// Shouldn't be needed since the opening of the offer details should record it. Does not record internal banner ad performance though :|  -mg
//#pragma warning disable 4014
//            string bannerId;
//            if (_banner.Id.HasValue) bannerId = _banner.Id.Value.ToString();
//            else bannerId = _banner.Name;
//            MetricsAPI.BannerPerformanceMetric(bannerId);
//#pragma warning restore 4014
        }
    }

    private async void ShowOfferDetails(bool isVideo)
    {
        var offer = await ScutiNetClient.Instance.Offer.GetOfferByID(_banner.Id.ToString());
        if (offer != null)
        {
            if (!ScutiUtils.TryOpenLink(offer))
            {
                var panelModel = Mappers.GetOfferDetailsPresenterModel(offer);
                if (panelModel != null)
                {
                    UIManager.OfferDetails.SetData(panelModel);
                    UIManager.OfferDetails.SetIsVideo(isVideo);
                    UIManager.Open(UIManager.OfferDetails);
                }
            }
        }
    }

    private async void Rotate()
    {
        if (!ScutiNetClient.Instance.IsInitialized)
        {
            System.Threading.CancellationTokenSource source = new System.Threading.CancellationTokenSource();
            await System.Threading.Tasks.Task.Delay(500, source.Token);
            source.Cancel();
            Rotate();
            return;
        }

        _loading = true;

        OfferPage offerPage = null;

        try
        {
            offerPage = await ScutiNetClient.Instance.Offer.GetOffers(new List<CampaignType> { CampaignType.Banner }, OfferService.MediaType.Banner, FILTER_TYPE.Eq, null, null, null, _index, 1);

        } catch (Exception e)
        {
            ScutiLogger.LogException(e);
        }

        if (offerPage != null && offerPage.Paging.TotalCount > 0)
        {
            onCreateBanners?.Invoke((int)offerPage.Paging.TotalCount);
        }

        if (offerPage != null && offerPage.Nodes.Count>0)
        {
            _banner = (offerPage.Nodes as List<Offer>)[0];
        } else
        {
            _index = 0;
            _loading = false;
           return;
        }

        if (_banner == null || string.IsNullOrEmpty(_banner.Media.Banner.BigUrl))
        {
            if (_index == 0)
                _timePassed = 0; // give it a break before trying again
            else
                //_index = 0;
                _index = _prevIndex;

            _loading = false;
        }
        else
        {
            _index++;

            try
            {
                var result = await ImageDownloader.New().Download(_banner.Media.Banner.BigUrl);
                if (result != null)
                {
                    ResetTimer();
                    BannerImage.enabled = true;
                    BannerImage.sprite = result.ToSprite();
                    _loading = false;
                    _timePassed = 0;

                }
                else
                {
                    ScutiLogger.LogError("Failed to load: " + _banner.Media.Banner.BigUrl);
                    _loading = false;
                    _timePassed = 0;
                }
            }
            catch
            {
                ScutiLogger.LogError("Failed to load: " + _banner.Media.Banner.BigUrl);
                _loading = false;
                _timePassed = 0;
            }
        }
    }

    public override void Open()
    {
        base.Open();
	    if(Rounded != null) Rounded.Refresh();
    }

    public void SetIndex(int index)
    {
        _index = index;
        _prevIndex = _index;
    }
}
