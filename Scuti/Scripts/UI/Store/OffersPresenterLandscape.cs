using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using Image = UnityEngine.UI.Image;
using Scuti.GraphQL.Generated;

using System.Threading;
using Scuti.Net;
using UnityEngine.Events;

namespace Scuti.UI
{
    public class OffersPresenterLandscape : OffersPresenterBase
    {


        [Header("Settings")]
        [SerializeField] int maxOffers = 6;
        [SerializeField] int videoOfferBackFill = 3;
        [SerializeField] int largeOffers = 2;
        [SerializeField] float showDuration = 10;

        [Header("Instantiation")]
        [SerializeField] OfferSummaryPresenterLandscape widgetPrefab_Large;
        [SerializeField] OfferSummaryPresenterLandscape widgetPrefab_Small;
        [SerializeField] Transform container_Large;
        [SerializeField] Transform container_Small;
        [SerializeField] Transform container_Video;
        [SerializeField] OfferVideoPresenter videoWidget;

        private Vector3 _largeContainerDefaultPosition;

        // ================================================
        #region LIFECYCLE
        // ================================================
       

        protected override void ResumeAds()
        {
            base.ResumeAds();
            videoWidget?.ResumeTimer();
        }

        protected override void PauseAds()
        {
            base.PauseAds();
            videoWidget?.PauseTimer();
        }

        #endregion

        // ================================================
        #region CATEGORY AND PAGINATION
        // ================================================

        protected override async Task ShowCategoryHelper() 
        {
            
            if(container_Video!=null && videoWidget!=null) 
            {
                var offersPage = await ScutiNetClient.Instance.Offer.GetOffers(new List<CampaignType> { CampaignType.Video }, FILTER_TYPE.Eq, m_Pagination.Category, null, null, m_Pagination.VideoIndex, 1);
                if (offersPage != null && offersPage.Nodes != null && offersPage.Nodes.Count>0)
                {
                    m_Pagination.VideoIndex++;
                    ShowVideo((offersPage.Nodes as List<Offer>)[0]);
                } else
                {
                    m_Pagination.VideoIndex=0;
                    HideVideo();
                }
            }
             
        }

        private int GetActiveLarge()
        {
            return largeOffers + _activeVideoOffers;
        }

        private int GetActiveMax()
        {
            return maxOffers + _activeVideoOffers;
        }

        private void HideVideo()
        {
            _activeVideoOffers = videoOfferBackFill;
            container_Video.gameObject.SetActive(false);
            container_Large.position = container_Video.position;
        }

        private void ShowVideo(Offer offer)
        {
            _activeVideoOffers = 0;
            container_Video.gameObject.SetActive(true);
            container_Large.position = _largeContainerDefaultPosition;
            videoWidget.SetDuration(15f);
            videoWidget.Data = Mappers.GetVideoPresenterModel(offer);
        }


        #endregion

        // ================================================
        #region API
        // ================================================



        public override void Clear()
        {
            base.Clear();
            foreach (Transform child in container_Large)
                Destroy(child.gameObject);

            foreach (Transform child in container_Small)
                Destroy(child.gameObject);
        }

        private void ResetTimeout()
        {
            m_Idle = false;
            TimeoutTimer.SoftReset();
        }


        #endregion

        // ================================================
        #region PRESENTATION
        // ================================================

        async protected override Task PopulateOffers(CancellationToken cancelToken)
        {
            var max = GetActiveMax();
            for (int i = 0; i < max; i++)
            {
                if (cancelToken.IsCancellationRequested) return;

                OfferSummaryPresenterLandscape template;
                Transform container;

                var index = i;
                
                    // Based on the index, the template and container are chosen.
                    // Currently, the first two offers are large, the other are small
                    template = GetTemplateForIndex(index);
                    container = GetContainerForIndex(index);
                var widget = Instantiate(template, container);
                m_Instantiated.Add(widget);
                widget.gameObject.hideFlags = HideFlags.DontSave;
                widget.Inject(GetNext);
                var colorData = GetColorInfo(index);
                widget.SetColorData(colorData.Background, colorData.Glow);
                widget.SetDuration(showDuration);

                await Task.Delay((int)(instantiationInterval * 1000));

                if (cancelToken.IsCancellationRequested) return;

                // If the index exceeds the count, we don't assign any data to it
                // nor do we listen to the click event. The offer widget does get
                // instantiated but it's just loading and doesn't do anything.
                var newData = Data.UseItem();
                if (newData == null) continue;
                widget.Data = newData;
                widget.Data.Index = index;
                widget.Data.IsTall = widget.IsTall;
                widget.Data.LoadImage();
                widget.OnLoaded += OnWidgetLoaded;
            

                widget.OnClick += async () =>
                {
                    UIManager.ShowLoading(false);
                    var id = widget.Data.ID;
                    var offer = await ScutiNetClient.Instance.Offer.GetOfferByID(id);
                    var panelModel = Mappers.GetOfferDetailsPresenterModel(offer);

                    try
                    {
                        UIManager.OfferDetails.SetData(panelModel); 
                        UIManager.OfferDetails.SetIsVideo(!string.IsNullOrEmpty(widget.Data.VideoURL));
                        UIManager.Open(UIManager.OfferDetails);
                    } catch(Exception e)
                    {
                        ScutiLogger.LogException(e);
                        UIManager.Alert.SetHeader("Out of Stock").SetBody("This item is out of stock. Please try again later.").SetButtonText("OK").Show(() => { });
                        //UIManager.Open(UIManager.Offers);
                    }

                    UIManager.HideLoading(false);
                };
            }


            await Task.Delay(250);
            //Debug.LogWarning(container_Large.childCount+"   ++++++++++++++    "+ container_Small.childCount);
            OnPopulateFinished?.Invoke();
            m_ChangingCategories = false;
        }


        protected override void OnWidgetLoaded(bool initial, OfferSummaryPresenterBase widget)
        {
            Debug.LogError("WIDGET LOADED");
            loadedWidgetQueue.Enqueue(new Tuple<OfferSummaryPresenterBase, bool>(widget, initial));
        }
        OfferSummaryPresenterLandscape GetTemplateForIndex(int index)
        {
            return index < GetActiveLarge() ? widgetPrefab_Large : widgetPrefab_Small;
        }

        Transform GetContainerForIndex(int index)
        {
            return index < GetActiveLarge() ? container_Large : container_Small;
        }
#endregion
    }
}