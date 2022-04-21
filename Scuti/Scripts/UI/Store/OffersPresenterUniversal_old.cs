using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using ScrollRect = UnityEngine.UI.ScrollRect;
using Image = UnityEngine.UI.Image;
using Scuti.GraphQL.Generated;

using System.Threading;
using Scuti.Net;
using UnityEngine.Events;
using UnityEngine.EventSystems; 

namespace Scuti.UI
{
    public class OffersPresenterUniversalOld : OffersPresenterBase
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
        [SerializeField] InfiniteScrollWithRows Rows;
        [SerializeField] Transform container_Large;
        [SerializeField] Transform container_Small;
        [SerializeField] ScrollRect scrollOffers;

        [Header("Scroll")]
        [SerializeField] bool onTop;
        [SerializeField] float lastValue = 0;
        [SerializeField] bool isDown;
        [SerializeField] ScrollStateTag scrollState;


        [SerializeField] bool isInitilize = false;

        public enum ScrollStateTag
        {
            isUp,
            isDown,
            Count
        }

        // ================================================
        #region LIFECYCLE
        // ================================================
       

        protected override void ResumeAds()
        {
            //base.ResumeAds();
            UIManager.TopBar?.ResumeBanner();
        }

        protected override void PauseAds()
        {
            //base.PauseAds();
            if (!firstOpen) UIManager.TopBar?.PauseBanner();
        }

        #endregion

        // ================================================
        #region CATEGORY AND PAGINATION
        // ================================================
        private int GetActiveLarge()
        {
            return largeOffers + _activeVideoOffers;
        }
        

        private int GetActiveMax()
        {
            return maxOffers + _activeVideoOffers;
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
				Rows.Init(max);
                Debug.LogError("Popuplate offers: " + max);
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
                    var widget = Rows.InstantiateWidget(template);
					widget.gameObject.SetActive(false);
					m_Instantiated.Add(widget);
                    widget.gameObject.hideFlags = HideFlags.DontSave;
                    widget.Inject(GetNext);
                    //var colorData = GetColorInfo(index);
                    //widget.SetColorData(colorData.Background, colorData.Glow);
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
						columnSystem.Remove(widget);
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
					widget.gameObject.SetActive(true);
				}


				await Task.Delay(250);
                //Debug.LogWarning(container_Large.childCount+"   ++++++++++++++    "+ container_Small.childCount);
                OnPopulateFinished?.Invoke();

                m_ChangingCategories = false;
            } catch (Exception e)
            {
                ScutiLogger.LogException(e);
            }

            isInitilize = true;
        }

		private void IntiColumnSystem()
		{
			var tempColumnWidth = widgetPrefab_Large.GetComponent<RectTransform>().rect.width;
			columnSystem.Init(tempColumnWidth);
			//columnSystem.ScrollPass += LaodMoreWidgets;
		}

		private async void OnPresenterClicked(OfferSummaryPresenterBase presenter)
        {
            UIManager.ShowLoading(false);
            var id = presenter.Data.ID;
            var offer = await ScutiNetClient.Instance.Offer.GetOfferByID(id);
            if (!ScutiUtils.TryOpenLink(offer))
            {
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

		public async void LaodMoreWidgets()
		{
			var offersCount = m_Instantiated.Count;

#pragma warning disable 4014
			_loadingSource = new CancellationTokenSource();
			await PopulateOffers(_loadingSource.Token);
#pragma warning restore 4014

			//Nothing else is instantiated, then we need to invoke the infinite scroller
			if (offersCount == m_Instantiated.Count)
			{
				columnSystem.InfiniteScroll();
			}
		}
        #endregion

        // ================================================
        #region Scroll Detect

        /*protected override void Awake()
        {
            Debug.Log("Scroll initialize");
            scrollOffers.onValueChanged.AddListener(scrollRectCallBack);
            lastValue = scrollOffers.horizontalNormalizedPosition;
            isDown = true;
            scrollTag = ScrollTag.isUp;
        }*/

        void OnEnable()
        {
            scrollOffers.onValueChanged.AddListener(scrollRectCallBack);
            lastValue = scrollOffers.verticalNormalizedPosition;
            onTop = true;
            GetNavigator().Show();
        }


        void scrollRectCallBack(Vector2 value)
        {
            if (!isInitilize)
                return;

            if(scrollOffers.velocity.y <= 0.1f && lastValue >= 0.95f)
            {
                if (onTop)
                    return;

                scrollState = ScrollStateTag.isUp;
                GetNavigator().Show();               
                isDown = false;
                onTop = true;
            }
            else if(lastValue < 0.95f)
            {
                onTop = false;
                if (lastValue <= scrollOffers.verticalNormalizedPosition)
                {
                    if (scrollState != ScrollStateTag.isUp)
                    {
                        scrollState = ScrollStateTag.isUp;
                        CheckChange();
                        isDown = false;

                    }
                }
                else if (lastValue > scrollOffers.verticalNormalizedPosition)
                {

                    if (scrollState != ScrollStateTag.isDown)
                    {
                        scrollState = ScrollStateTag.isDown;
                        CheckChange();
                        isDown = true;
                    }
                }
            }                 

            lastValue = scrollOffers.verticalNormalizedPosition;

        }

        private void CheckChange()
        {

            if (isDown && scrollState == ScrollStateTag.isUp)
            {
                GetNavigator().Show();
            }            
            else if(!isDown && scrollState == ScrollStateTag.isDown)
            { 
                GetNavigator().Hide();
            }
               
            
        }

        void OnDisable()
        {
            scrollOffers.onValueChanged.RemoveListener(scrollRectCallBack);
        }
        #endregion
    }
}