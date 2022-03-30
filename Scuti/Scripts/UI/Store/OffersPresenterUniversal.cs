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
    public class OffersPresenterUniversal : OffersPresenterBase
    {


        [Header("Settings")]
        [SerializeField] int maxOffers = 6;
        [SerializeField] int videoOfferBackFill = 3;
        [SerializeField] int largeOffers = 2;
        [SerializeField] float showDuration = 5;

        [Header("Instantiation")]
        [SerializeField] OfferSummaryPresenterUniversal widgetPrefab_Large;
        [SerializeField] OfferSummaryPresenterUniversal widgetPrefab_Small;
        [SerializeField] ColumnSystem columnSystem;
        [SerializeField] ColumnSystem _colum;
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
            //base.ResumeAds();
            videoWidget?.ResumeTimer();
        }

        protected override void PauseAds()
        {
            //base.PauseAds();
            videoWidget?.PauseTimer();
        }

        #endregion

        // ================================================
        #region CATEGORY AND PAGINATION
        // ================================================

        protected override async Task ShowCategoryHelper(CancellationToken token) 
        {
            
            if(container_Video!=null && videoWidget!=null) 
            {
                var pagination = Data.GetPagination(OfferService.MediaType.Product);
                var offersPage = await ScutiNetClient.Instance.Offer.GetOffers(new List<CampaignType> { CampaignType.Video }, OfferService.MediaType.Product, FILTER_TYPE.Eq, pagination.Category, null, null, pagination.VideoIndex, 1);
                if(token.IsCancellationRequested)
                {
                    return;
                }
                
                if (offersPage != null && offersPage.Nodes != null && offersPage.Nodes.Count>0)
                {
                    pagination.VideoIndex++;
                    ShowVideo((offersPage.Nodes as List<Offer>)[0]);
                } else
                {
                    pagination.VideoIndex=0;
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
			columnSystem.Clear();

			/*
            foreach (Transform child in container_Large)
                Destroy(child.gameObject);

            foreach (Transform child in container_Small)
                Destroy(child.gameObject);
			*/
        }

   


        #endregion

        // ================================================
        #region PRESENTATION
        // ================================================

        async protected override Task PopulateOffers(CancellationToken cancelToken)
        {
            try
            {
				if (!columnSystem.isInitialized)
				{
					IntiColumnSystem();
				}

                m_ChangingCategories = true;
                var max = GetActiveMax();
                for (int i = 0; i < max; i++)
                { 
                    if (cancelToken.IsCancellationRequested) return;

                    OfferSummaryPresenterUniversal template;
                    Transform container;

                    var index = i;

                    // Based on the index, the template and container are chosen.
                    // Currently, the first two offers are large, the other are small
                    template = GetTemplateForIndex(index);
                    container = GetContainerForIndex(index);
                    var widget = columnSystem.InstantiateWidget(template);
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
                    var mediaType = widget.RollForMediaType();
                    var newData = Data.RequestOffer(mediaType);

                    if(newData==null && mediaType!= OfferService.MediaType.Product)
                    {
                        mediaType = OfferService.MediaType.Product;
                        newData = Data.RequestOffer(mediaType);
                    }

                    if (newData == null)
                    {
                        //Debug.LogError("Null data: " + widget.gameObject);
						m_Instantiated.Remove(widget);
						Destroy(widget.gameObject);
						continue;
                    }
                    widget.Data = newData;
                    widget.Data.Index = index;
                    widget.Data.IsTall = widget.IsTall;
                    widget.Data.isSingle = widget.Single;
                    widget.Data.LoadImage();
                    widget.OnLoaded -= OnWidgetLoaded;
                    widget.OnLoaded += OnWidgetLoaded;

                    widget.OnClick -= OnPresenterClicked;
                    widget.OnClick += OnPresenterClicked;
                }


                await Task.Delay(250);
                //Debug.LogWarning(container_Large.childCount+"   ++++++++++++++    "+ container_Small.childCount);
                OnPopulateFinished?.Invoke();

                m_ChangingCategories = false;
            } catch (Exception e)
            {
                ScutiLogger.LogException(e);
            }
        }

		private void IntiColumnSystem()
		{
			var tempColumnWidth = widgetPrefab_Large.GetComponent<RectTransform>().rect.width;
			columnSystem.Init(tempColumnWidth);
			columnSystem.ScrollPass += LaodMoreWidgets;
		}

		private async void OnPresenterClicked(OfferSummaryPresenterBase presenter)
        {
            UIManager.ShowLoading(false);
            var id = presenter.Data.ID;
            var offer = await ScutiNetClient.Instance.Offer.GetOfferByID(id);
            var panelModel = Mappers.GetOfferDetailsPresenterModel(offer);
            try
            {
                UIManager.OfferDetails.SetData(panelModel);
                UIManager.OfferDetails.SetIsVideo(!string.IsNullOrEmpty(presenter.Data.VideoURL));
                UIManager.Open(UIManager.OfferDetails);
            }
            catch (Exception e)
            {
                ScutiLogger.LogException(e);
                UIManager.Alert.SetHeader("Out of Stock").SetBody("This item is out of stock. Please try again later.").SetButtonText("OK").Show(() => { });
                //UIManager.Open(UIManager.Offers);
            }

            UIManager.HideLoading(false);
        }

        protected override void OnWidgetLoaded(bool initial, OfferSummaryPresenterBase widget)
        {
            loadedWidgetQueue.Enqueue(new Tuple<OfferSummaryPresenterBase, bool>(widget, initial));
        }
        OfferSummaryPresenterUniversal GetTemplateForIndex(int index)
        {
            return index < GetActiveLarge() ? widgetPrefab_Large : widgetPrefab_Small;
        }

        Transform GetContainerForIndex(int index)
        {
            return index < GetActiveLarge() ? container_Large : container_Small;
        }

		#endregion

		public void LaodMoreWidgets()
		{
#pragma warning disable 4014
			_loadingSource = new CancellationTokenSource();
			PopulateOffers(_loadingSource.Token);
#pragma warning restore 4014
		}
	}
}