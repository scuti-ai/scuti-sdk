using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using Image = UnityEngine.UI.Image;


using Scuti.GraphQL.Generated;

using LoadedWidgetQueue = System.Collections.Generic.Queue<System.Tuple<Scuti.UI.OfferSummaryPresenterBase, bool>>;
using GetNextRequestQueue = System.Collections.Generic.Queue<System.Tuple<System.Action<Scuti.UI.OfferSummaryPresenterBase.Model>, Scuti.UI.OfferSummaryPresenterBase>>;
using System.Threading;
using Scuti.Net;
using UnityEngine.Events;


namespace Scuti.UI
{
    public class OffersPresenterBase : Presenter<OffersPresenterBase.Model>
    {

        public class OfferPool
        {

            private bool _allowedToEmpty = true;
            public OfferPool(bool allowedToEmpty)
            {
                _allowedToEmpty = allowedToEmpty;
            }


            private Pagination _pagination;
            public Pagination Pagination
            {
                get
                {
                    return _pagination;
                }
            }
            protected Dictionary<string, Pagination> PaginationMap = new Dictionary<string, Pagination>();

            public HashSet<OfferSummaryPresenterBase.Model> ActiveItems = new HashSet<OfferSummaryPresenterBase.Model>();
            public List<OfferSummaryPresenterBase.Model> NewItems = new List<OfferSummaryPresenterBase.Model>();
            public List<OfferSummaryPresenterBase.Model> PooledItems = new List<OfferSummaryPresenterBase.Model>();

            public bool TrySetCategory(string category)
            {
                if (_pagination != null)
                {
                    if (_pagination.Category != null && _pagination.Category.Equals(category))
                    {
                        return false;
                    }
                }

                string categoryValue = category;
                if (category.Equals("DEFAULT"))
                {
                    categoryValue = null;
                }
                if (!PaginationMap.ContainsKey(category))
                    PaginationMap[category] = new Pagination()
                    {
                        Category = categoryValue,
                        Index = 0,
                        VideoIndex = 0,
                        TotalCount = 0
                    };
                _pagination = PaginationMap[category];
                _pagination.Index = 0;
                _pagination.VideoIndex = 0;
                foreach(var active in ActiveItems)
                {
                    // ensure they do not return to pool
                    active.OnDispose -= ReturnItem;
                }
                ActiveItems.Clear();
                NewItems.Clear();
                PooledItems.Clear();
                return true;
            }


            public int ActiveItemsCount
            {
                get
                {
                    if (ActiveItems == null) return 0;
                    return ActiveItems.Count;
                }
            }

            public int NewItemsCount
            {
                get
                {
                    if (NewItems == null) return 0;
                    return NewItems.Count;
                }
            }

            public int PooledItemsCount
            {
                get
                {
                    if (PooledItems == null) return 0;
                    return PooledItems.Count;
                }
            }

            public int TotalItemCount
            {
                get
                {
                    return ActiveItemsCount + NewItemsCount + PooledItemsCount;
                }
            }


            public OfferSummaryPresenterBase.Model UseItem()
            {
                OfferSummaryPresenterBase.Model result = null;
                if (NewItemsCount > 0)
                {
                    result = NewItems[0];
                    NewItems.RemoveAt(0);
                    result.OnDispose -= ReturnItem;
                    result.OnDispose += ReturnItem;
                    ActiveItems.Add(result);
                }
                return result;
            }

            public void AddNewItem(OfferSummaryPresenterBase.Model item)
            {
                NewItems.Add(item);
            }

            public void AddNewItems(OfferSummaryPresenterBase.Model[] items)
            {
                NewItems.AddRange(items);
            }

            public OfferSummaryPresenterBase.Model[] GetNewItems()
            {
                return NewItems.ToArray();
            }

            public void ReturnItem(OfferSummaryPresenterBase.Model item)
            {
                if (item != null) item.OnDispose -= ReturnItem;

                if (ActiveItems != null)
                    ActiveItems.Remove(item);
                if (PooledItems != null)
                    PooledItems.Add(item);
            }

            public void EmptyPool()
            {
                if (_allowedToEmpty && PooledItems.Count > 0)
                {
                    NewItems.AddRange(PooledItems);
                    PooledItems.Clear();
                }
            }

            public void ClearPagination()
            {
                PaginationMap.Clear();
                if (_pagination != null)
                    _pagination.VideoIndex = 0;
            }

            public void Shuffle()
            {
                NewItems.Shuffle();
            }
        }

        internal bool IsUnableToChangeCategory()
        {
            return m_ChangingCategories;
        }

        [Serializable]
        public class Model : Presenter.Model
        {
            public OfferPool VerticalOffers = new OfferPool(false);
            public OfferPool TileOffers = new OfferPool(false);
            public OfferPool ProductOffers = new OfferPool(true);
            public OfferPool BannerOffers = new OfferPool(true);




            public OfferSummaryPresenterBase.Model RequestOffer(OfferService.MediaType mediaType)
            {
                OfferPool pool = GetPool(mediaType);
                return pool.UseItem();
            }

            public void AddNewItems(OfferService.MediaType mediaType, OfferSummaryPresenterBase.Model[] items)
            {
                OfferPool pool = GetPool(mediaType);
                pool.AddNewItems(items);
            }

            public void AddNewItem(OfferSummaryPresenterBase.Model item)
            {
                OfferService.MediaType mediaType = OfferService.MediaType.Product;

                if (item.DisplayAd)
                {
                    if (!string.IsNullOrEmpty(item.TallURL) && !ScutiUtils.IsPortrait()) mediaType = OfferService.MediaType.Vertical;
                    else if(!string.IsNullOrEmpty(item.SmallURL))
                    {
                        mediaType = OfferService.MediaType.SmallTile;
                    }
                }

                Debug.LogError("Adding " + mediaType + " due to " + item.DisplayAd + " and " + item.IsTall  +"  "+ item.TallURL);
                OfferPool pool = GetPool(mediaType);
                pool.AddNewItem(item);
            }

            public void Shuffle()
            {
                ProductOffers.Shuffle();
                BannerOffers.Shuffle();
            }


            public int NewItemsCount(OfferService.MediaType mediaType)
            {
                OfferPool pool = GetPool(mediaType);
                return pool.NewItemsCount;
            }

            public OfferSummaryPresenterBase.Model[] GetNewItems(OfferService.MediaType mediaType)
            {
                OfferPool pool = GetPool(mediaType);
                return pool.GetNewItems();
            }

            public void EmptyPool(OfferService.MediaType mediaType)
            {
                OfferPool pool = GetPool(mediaType);
                pool.EmptyPool();
            }

            private OfferPool GetPool(OfferService.MediaType mediaType)
            {
                switch (mediaType)
                {
                    case OfferService.MediaType.SmallTile:
                        return TileOffers;
                    case OfferService.MediaType.Vertical:
                        return  VerticalOffers;
                    case OfferService.MediaType.Banner:
                        return BannerOffers;
                    default:
                        return ProductOffers;
                }
            }

            public Pagination GetPagination(OfferService.MediaType mediaType)
            {
                var pool = GetPool(mediaType);
                return pool.Pagination;
            }

            public void ClearPagination(bool impressionAds = false)
            {
                if (impressionAds)
                {
                    VerticalOffers.ClearPagination();
                    TileOffers.ClearPagination();
                }
                ProductOffers.ClearPagination();
                BannerOffers.ClearPagination();
            }

            public bool TrySetCategory(string category)
            {
                VerticalOffers.TrySetCategory(category);
                TileOffers.TrySetCategory(category);
                BannerOffers.TrySetCategory(category);
                return  ProductOffers.TrySetCategory(category);
            }
        }

        [Serializable]
        public class Pagination
        {
            public string Category = string.Empty;
            public int VideoIndex;
            [SerializeField] int _index;
            public int Index
            {
                get { return _index; }
                set { _index = value; }
            }

            public int TotalCount;
        }

        [SerializeField] CategoryNavigator categoryNavigator;

        public CategoryNavigator GetNavigator()
        {
            return categoryNavigator;
        }

        public bool ShouldUpdateOffers
        {
            get
            {
                return IsOpenOrOpening && (UIManager.Navigator.CurrentModal == null || UIManager.Navigator.CurrentModal == UIManager.TopMenu) && !m_Idle;
            }
        }

       

        protected int _activeVideoOffers = 0;

       
        [SerializeField] protected int offerDataToRequest = 6;
        [SerializeField] protected float instantiationInterval = .5f;
        [SerializeField] protected float showInterval = .5f;
        [SerializeField] protected int MinDataCached = 6;


        [Serializable]
        public struct OfferColorData
        {
            public Sprite Background;
            public Color32 Glow;
        }


        [Header("Customization")]
        [SerializeField] protected Image bannerImage;
        [SerializeField] protected Sprite[] backgrounds;
        [SerializeField] protected OfferColorData[] colorInfo;


        public Timer TimeoutTimer;
        public BannerWidget Banner;

        public UnityEvent OnPopulateFinished;
        public UnityEvent OnClearFinished;

        protected bool m_Idle = false;
        protected bool m_Paused = false;
        protected bool m_ChangingCategories = false;

        protected GetNextRequestQueue GetNextRequestQueue = new GetNextRequestQueue();
        protected bool m_requestOffersInProgress = false;
        protected List<OfferSummaryPresenterBase> m_Instantiated = new List<OfferSummaryPresenterBase>();
         


        protected CancellationTokenSource _loadingSource;
        protected List<CancellationTokenSource> _offerSources;
        // QUEUE HANDLERS
        protected LoadedWidgetQueue loadedWidgetQueue = new LoadedWidgetQueue();

        
         
        // ================================================
        #region LIFECYCLE
        // ================================================
        public override void Open()
        {
            var first = (firstOpen);
            base.Open();
            if (first)
            {
                UIManager.ShowLoading(true);
                categoryNavigator.OpenCurrent();
            }
            else
            {
                ResumeAds();
            }
            Banner.Open();
        }

        public override void Close()
        {
            base.Close();

            PauseAds();
        }

        protected virtual void ResumeAds()
        {
            m_Paused = false;
            m_Idle = false;
            TimeoutTimer.ResetTime(ScutiConstants.SCUTI_TIMEOUT);
            Banner.Play();
            TimeoutTimer.Begin();
            foreach (var offer in m_Instantiated)
            {
                if (offer.HasData)
                    offer.ResumeTimer();
            }
        }

        protected virtual void PauseAds()
        {
            Banner.Pause();
            m_Paused = true;
            TimeoutTimer.Pause();
            foreach (var offer in m_Instantiated)
            {
                offer.PauseTimer();
            }
        }

        private void OnTimeout()
        {
            m_Idle = true;
            PauseAds();
        }

        protected override void Awake()
        {
            base.Awake();


            // ugly coupling but needs to be done quickly. TODO: cleanup -mg
            categoryNavigator.SetPresenter(this);

            TimeoutTimer.onFinished.AddListener(OnTimeout); 
            if (categoryNavigator)
                categoryNavigator.OnOpenRequest += ShowCategory;
            ProcessLoadedWidgetQueue();
        }

        private void Update()
        {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            if(Input.touchCount>0)
#else
#if ENABLE_INPUT_SYSTEM

            if (Keyboard.current.anyKey.isPressed ||
                    Mouse.current.delta.x.ReadValue() != 0)

#else
            if (Input.anyKey || Input.GetAxis("Mouse X") != 0)

#endif

#endif
            {
                if (m_Paused && ShouldUpdateOffers)
                {
                    ResumeAds();
                }
                else
                {
                    ResetTimeout();
                }
            }
            ProcessGetNextRequestQueue();
        }


        async void ProcessLoadedWidgetQueue()
        {
            while (true)
            {
                if (!m_Paused && !m_ChangingCategories  && loadedWidgetQueue.Count > 0)
                {
                    var dequeue = loadedWidgetQueue.Dequeue();
                    var widget = dequeue.Item1;
                    var initializing = dequeue.Item2;

                    if (initializing)
                        widget.Show();
                    widget.DisplayCurrentImage();
                    widget.ResetTimer();
                }
                await Task.Delay((int)(showInterval * 1000));
            }
        }

        internal void ResetPagination()
        {
            Data.ClearPagination();
        }

       
#endregion

        // ================================================
#region CATEGORY AND PAGINATION
        // ================================================
        public async void ShowCategory(string category)
        {
            if (!m_ChangingCategories && TrySetCategory(category))
            {
                m_ChangingCategories = true;
                Clear();
                
                if(_offerSources != null)
                {
                    foreach(var src in _offerSources)
                    {
                        src.Cancel();
                    }
                }
                _offerSources = new List<CancellationTokenSource>();
                var source = new CancellationTokenSource();
                _offerSources.Add(source);
                await ShowCategoryHelper(source.Token);
                await RequestMoreOffers(false, source.Token, offerDataToRequest, OfferService.MediaType.Product);
                if(!ScutiUtils.IsPortrait())await RequestMoreOffers(false, source.Token, offerDataToRequest, OfferService.MediaType.Vertical);
                await RequestMoreOffers(false, source.Token, offerDataToRequest, OfferService.MediaType.SmallTile);

#pragma warning disable 4014
                _loadingSource = new CancellationTokenSource();
                PopulateOffers(_loadingSource.Token);
#pragma warning restore 4014
            }
            UIManager.HideLoading(true);
        }

        protected virtual async Task ShowCategoryHelper(CancellationToken token) { }

     
         
        public bool TrySetCategory(string category)
        {
            if (Data == null) Data = new Model(); 
            return Data.TrySetCategory(category);
            
        }

        
        #endregion

        // ================================================
        #region API
        // ================================================
        /// <summary>
        /// Returns a list of offers, based on the current paginataion status
        /// </summary>
        private bool requestInProgress = false;
        public async Task RequestMoreOffers(bool replaceData, CancellationToken token, int maxCount, OfferService.MediaType mediaType)
        {
            int requestMore = 0;
            var pagination = Data.GetPagination(mediaType);
            //Debug.Log("Request pagination for " + mediaType);
            if (pagination.Index >= pagination.TotalCount)
            {
                pagination.Index = 0;
            }
            else
            {
                // hacky way to break recursion
                if (replaceData && maxCount == offerDataToRequest)
                    requestMore = (pagination.Index + maxCount) - pagination.TotalCount;
            }
            var index = pagination.Index;
            requestInProgress = true;
            pagination.Index += maxCount;
            OfferPage offerPage = null;
            try
            {
                offerPage = await ScutiNetClient.Instance.Offer.GetOffers(new List<CampaignType> { CampaignType.Product, CampaignType.ProductListing, CampaignType.AppDownload }, mediaType, FILTER_TYPE.In, pagination.Category, null, null, index, maxCount);
            }
            catch (Exception e)
            {
                ScutiLogger.LogException(e);
            }
            if (token.IsCancellationRequested)
            {
                return;
            }

            if (offerPage != null)
            {
                pagination.TotalCount = offerPage.Paging.TotalCount.GetValueOrDefault(0);
                if (replaceData)
                {
                    if (requestMore > 0 && requestMore < offerDataToRequest)
                    {
                        try
                        {
                            index = 0;
                            pagination.Index = requestMore;
                            var secondPage = await ScutiNetClient.Instance.Offer.GetOffers(new List<CampaignType> { CampaignType.Product, CampaignType.ProductListing, CampaignType.AppDownload }, mediaType, FILTER_TYPE.In, pagination.Category, null, null, index, requestMore);
                            if (secondPage != null)
                            {
                                foreach (var node in secondPage.Nodes)
                                {
                                    offerPage.Nodes.Add(node);

                                }
                            }
                        }
                        catch (Exception e)
                        {
                            ScutiLogger.LogException(e);
                        }
                    }
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    Data = Mappers.GetOffersPresenterModel(offerPage.Nodes as List<Offer>);
                }
                else
                {
                    var appendData = Mappers.GetOffersPresenterModel(offerPage.Nodes as List<Offer>);

                    Data.AddNewItems(mediaType, appendData.GetNewItems(mediaType));
                }
            }
            else
            {
                if (replaceData)
                {
                    pagination.TotalCount = 0;
                    Data = null;
                }
            }
            requestInProgress = false;

        }

        // Maintains a queue of requests that fetches them one by one. This is 
        // crucial when two offer timers get over pretty much together, the 
        // index for both their next offer requests will be the same and they will
        // get the same offers.
        public Task<OfferSummaryPresenterBase.Model> GetNext(OfferSummaryPresenterBase presenter)
        {
            var source = new TaskCompletionSource<OfferSummaryPresenterBase.Model>();
            GetNextRequestQueue.Enqueue(Tuple.Create<Action<OfferSummaryPresenterBase.Model>, OfferSummaryPresenterBase>(model =>{
                 source.SetResult(model);
            }, presenter));
            return source.Task;
        }

       

        public virtual void Clear()
        {
            if (_loadingSource != null)
            {
                _loadingSource.Cancel();
            }
            foreach (var widget in m_Instantiated)
            {
                widget.OnLoaded -= OnWidgetLoaded;
            }
            m_Instantiated.Clear();
            GetNextRequestQueue.Clear();
            loadedWidgetQueue.Clear();
            Resources.UnloadUnusedAssets();
            OnClearFinished?.Invoke();
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
        override protected void OnSetState()
        {
            
        }

        void ProcessGetNextRequestQueue()
        {
            bool shouldUpdate = ShouldUpdateOffers;
            if (shouldUpdate && m_Paused) ResumeAds();
            else if (!shouldUpdate && !m_Paused) PauseAds();

            if (!m_Paused && GetNextRequestQueue.Count != 0 && !m_ChangingCategories)
            {

                var tuple = GetNextRequestQueue.Peek();
                var request = tuple.Item1;
                var offerSummaryPresenter = tuple.Item2;

                var mediaType = offerSummaryPresenter.RollForMediaType();
                var pagination = Data.GetPagination(mediaType);

                var newItemCount = Data.NewItemsCount(mediaType);
                if (newItemCount < MinDataCached && pagination.Index< pagination.TotalCount && !requestInProgress)
                {
#pragma warning disable 4014
                    var source = new CancellationTokenSource();
                    _offerSources.Add(source);
                    RequestMoreOffers(false, source.Token, offerDataToRequest, mediaType);
#pragma warning restore 4014
                }

                //Debug.Log("Rolled " + mediaType + " >> " + offerSummaryPresenter.gameObject + "  count " + newItemCount  +"  "+requestInProgress  +" "+offerSummaryPresenter.gameObject.GetInstanceID());
                if (newItemCount > 0)
                { 
                    if(newItemCount < 2 && ScutiUtils.IsPortrait())
                    {
                        // Check if it is a two column row and we need to fill both columns.  If FirstColumn is False then we only need 1
                        if (!offerSummaryPresenter.Single && offerSummaryPresenter.FirstColumn)
                        {
                            if(requestInProgress)
                                return;
                            else
                            {
                                Data.EmptyPool(mediaType);
                                return;
                            }
                        }
                    }

                    OfferSummaryPresenterBase.Model model = Data.RequestOffer(mediaType);

                    if (model != null)
                    {
                        //Debug.Log("Deque "+offerSummaryPresenter.gameObject  +" into "+model.Brand +" : "+model.Title + " " + offerSummaryPresenter.gameObject.GetInstanceID());
                        GetNextRequestQueue.Dequeue();
                        request?.Invoke(model);
                    }  
                } else if(pagination.Index >= pagination.TotalCount && !requestInProgress)
                {
                    if(mediaType== OfferService.MediaType.Product || mediaType == OfferService.MediaType.Banner)
                        Data.EmptyPool(mediaType);
                }
            }
        }


        async protected virtual Task PopulateOffers(CancellationToken cancelToken)
        {
            
        }
         

        protected virtual void OnWidgetLoaded(bool initial, OfferSummaryPresenterBase widget)
        {
        }

        protected OfferColorData GetColorInfo(int index)
        {
            // Hack to force light blue for now
            index = 4;

            if (index >= colorInfo.Length) index = colorInfo.Length - (index % colorInfo.Length)-1;
            return colorInfo[index];
        }

        
#endregion
    }
}