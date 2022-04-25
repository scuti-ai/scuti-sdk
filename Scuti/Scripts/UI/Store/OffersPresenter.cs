using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Scuti.GraphQL.Generated;

using System.Threading;
using Scuti.Net;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.UI;


    using LoadedWidgetQueue = System.Collections.Generic.Queue<System.Tuple<Scuti.UI.OfferSummaryPresenterBase, bool>>;

namespace Scuti.UI
{
    public class OffersPresenter : Presenter<OffersPresenter.Model>
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
                foreach (var active in ActiveItems)
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
                //Debug.LogError("Return to pool: " + PooledItems.Count +"  "+item.Title);
            }

            public void EmptyPool()
            {
                if (_allowedToEmpty && PooledItems.Count > 0)
                {
                    NewItems.AddRange(PooledItems);
                    //Debug.LogError("emptied "+ PooledItems.Count + " and now size: "+NewItems.Count);
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
            public OfferPool VerticalOffers = new OfferPool(true);
            public OfferPool TileOffers = new OfferPool(true);
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
                    if (!string.IsNullOrEmpty(item.TallURL)) mediaType = OfferService.MediaType.Vertical;
                    else if (!string.IsNullOrEmpty(item.SmallURL))
                    {
                        mediaType = OfferService.MediaType.SmallTile;
                    }
                }

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
                        //Debug.LogError("Vertical Count " + VerticalOffers.TotalItemCount);
                        return VerticalOffers;
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
                return ProductOffers.TrySetCategory(category);
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



        [Header("Instantiation")]
        public List<OfferSummaryRowContainer> RowContainerPrefabs;


        private List<OfferSummaryRowContainer> _allRows = new List<OfferSummaryRowContainer>();
        private List<OfferSummaryPresenterBase> _allCells = new List<OfferSummaryPresenterBase>();
        private Dictionary<Transform, OfferSummaryRowContainer> _rowMap = new Dictionary<Transform, OfferSummaryRowContainer>();

        public ScutiInfiniteScroll InfinityScroll;
        public Transform OfferContainer;


        private int _columns = 2;
        private int _rows = 3;


        protected int _activeVideoOffers = 0;


        [SerializeField] protected int offerDataToRequest = 6;
        [SerializeField] protected float instantiationInterval = .5f;
        [SerializeField] protected float showInterval = .5f;
        [SerializeField] protected int MinDataCached = 6;
        public RectTransform Viewport;

        [Serializable]
        public struct OfferColorData
        {
            public Sprite Background;
            public Color32 Glow;
        }


        public Timer TimeoutTimer;


        protected bool m_Idle = false;
        protected bool m_Paused = false;
        protected bool m_ChangingCategories = false;

        //protected GetNextRequestQueue GetNextRequestQueue = new GetNextRequestQueue();
        protected bool m_requestOffersInProgress = false;
        //protected List<OfferSummaryPresenterBase> m_Instantiated = new List<OfferSummaryPresenterBase>();

        [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;


        protected CancellationTokenSource _loadingSource;
        protected List<CancellationTokenSource> _offerSources;
        // QUEUE HANDLERS
        protected LoadedWidgetQueue loadedWidgetQueue = new LoadedWidgetQueue();

        private bool _started;
        private bool _loadOnStart;

        // ================================================
        #region LIFECYCLE
        // ================================================
        public override void Open()
        {
            categoryNavigator.isShowingCategories = true;

            var first = (firstOpen);
            base.Open();
            if (first)
            {
                UIManager.ShowLoading(true);

                if (!_started)
                {
                    _loadOnStart = true;
                }
                else
                {
                    _loadOnStart = false;
                    InitScrollArea();
                }
            }
            else
            {
                ResumeAds();
            }
        }

        public override void Close()
        {
            categoryNavigator.isShowingCategories = false;

            base.Close();
            PauseAds();
        }

        protected virtual void ResumeAds()
        {
            m_Paused = false;
            m_Idle = false;
            TimeoutTimer.ResetTime(ScutiConstants.SCUTI_TIMEOUT);
            UIManager.TopBar?.ResumeBanner();
            TimeoutTimer.Begin();
            foreach (var offer in _allCells)
            {
                if (offer.HasData)
                    offer.ResumeTimer();
            }
        }

        protected virtual void PauseAds()
        {
            m_Paused = true;
            if (!firstOpen) UIManager.TopBar?.PauseBanner();
            TimeoutTimer.Pause();
            foreach (var offer in _allCells)
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


            var prefab = RowContainerPrefabs[0];
            if (!ScutiUtils.IsPortrait())
            {
                _verticalLayoutGroup.childAlignment = TextAnchor.UpperLeft;
                var columnWidth = prefab.Columns[0].GetComponent<RectTransform>().rect.width / 3; // Assuming the right value is 300px for a 1920px screen
                var canvasWidth = OfferContainer.GetComponentInParent<Canvas>().GetComponent<RectTransform>().rect.width;
                var screenWidth = Screen.width;

                var containerSize = screenWidth * (1920f / canvasWidth);
                var numberOfColumns = Math.Max(1, Mathf.FloorToInt(containerSize / (columnWidth)));
                if (numberOfColumns >= RowContainerPrefabs.Count)
                {
                    numberOfColumns = RowContainerPrefabs.Count - 1;
                }
                _columns = numberOfColumns;
            }
            else
            {
                _verticalLayoutGroup.childAlignment = TextAnchor.UpperCenter;
                //Debug.LogError("TODO: Should handle portrait on tablets here");
                _columns = 1;
            }

            prefab = RowContainerPrefabs[_columns - 1];
            for (var r = 0; r < _rows; r++)
            {
                var row = Instantiate(prefab, OfferContainer);
                row.gameObject.name = "Row: " + r;
                _allRows.Add(row);
                for (var c = 0; c < row.Columns.Count; c++)
                {
                    var col = row.Columns[c];
                    _allCells.Add(col.Tall);
                    col.Tall.Viewport = Viewport;
                    _allCells.AddRange(col.Small.Presenters.ToArray());

                    //m_Instantiated.Add(presenter);
                }

                _rowMap[row.transform] = row;
            }
            offerDataToRequest = (_rows * _columns) * 3;
            MinDataCached = offerDataToRequest / 2;
        }

        public void ResizeScrollRect()
        {
            if (_columns == 1) return;
            // scale ->
            // 4, 1920 => 0.521976  (screenwidth/row.rect.width)/ scaleFactor
            // 4, 2778 => 0.64399    0.7716666/1.1889
            // 3, 859 => 0.71172  

            var tempScale = Screen.width / (_allRows[0].transform.GetComponent<RectTransform>().rect.width + 125);
            var canvas = GetComponentInParent<Canvas>();
            var canvasScale = canvas.scaleFactor;

            OfferContainer.localScale = Vector3.one * (tempScale / canvasScale);

        }

        //        private bool Landscape()
        //        {
        //            var _landscape = Screen.orientation == ScreenOrientation.Landscape || Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight;
        //#if UNITY_EDITOR
        //            _landscape = Camera.main.pixelWidth > Camera.main.pixelHeight;
        //#endif
        //            return _landscape;

        //        }

        private void Start()
        {
            _started = true;
            if (_loadOnStart)
            {
                InitScrollArea();
            }
        }


        async protected Task InitScrollArea()
        {
            //ResizeScrollRect();
            //await Task.Delay(250);
            await Task.Delay(1000);
            ResizeScrollRect();
            InfinityScroll.Init();
            InfinityScroll.CheckBounds();
            categoryNavigator.OpenCurrent();
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
        }



        private void OnSiblingUpdated(Transform obj)
        {
            //Debug.LogError("Sib >>>> "+obj);
            if (_rowMap.ContainsKey(obj))
            {
                var col = _rowMap[obj];
                col.Clear();
                //Debug.Log("Queue : " + col);
                GetOffers(col);
            }
        }


        async void ProcessLoadedWidgetQueue()
        {
            while (true)
            {
                if (!m_Paused && !m_ChangingCategories && loadedWidgetQueue.Count > 0)
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

                if (_offerSources != null)
                {
                    foreach (var src in _offerSources)
                    {
                        src.Cancel();
                    }
                }
                _offerSources = new List<CancellationTokenSource>();
                var source = new CancellationTokenSource();
                _offerSources.Add(source);
                await ShowCategoryHelper(source.Token);
                await RequestMoreOffers(false, source.Token, offerDataToRequest, OfferService.MediaType.Product);
                await RequestMoreOffers(false, source.Token, offerDataToRequest / 3, OfferService.MediaType.Vertical);
                await RequestMoreOffers(false, source.Token, offerDataToRequest / 2, OfferService.MediaType.SmallTile);

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



        public virtual void Clear()
        {
            if (_loadingSource != null)
            {
                _loadingSource.Cancel();
            }
            foreach (var widget in _allCells)
            {
                widget.OnLoaded -= OnWidgetLoaded;
            }
            foreach (var presenter in _allRows)
            {
                presenter.Clear();
            }

            //m_Instantiated.Clear();
            loadedWidgetQueue.Clear();
            Resources.UnloadUnusedAssets();


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

        void GetOffers(OfferSummaryRowContainer offerSummaryRowContainer)
        {
            bool shouldUpdate = ShouldUpdateOffers;
            if (shouldUpdate && m_Paused) ResumeAds();
            else if (!shouldUpdate && !m_Paused)
            {
                PauseAds();
            }

            if (!m_ChangingCategories)
            {
                var needed = _columns * 2;
                var mediaType = OfferService.MediaType.Product;
                var productCount = Data.NewItemsCount(mediaType);
                var pagination = Data.GetPagination(mediaType);
                if (productCount < MinDataCached && pagination.Index < pagination.TotalCount && !requestInProgress)
                {
#pragma warning disable 4014
                    //Debug.LogError("Request more!!!");
                    var source = new CancellationTokenSource();
                    _offerSources.Add(source);
                    //RequestMoreOffers(false, source.Token, offerDataToRequest, mediaType);
                    RequestMoreOffers(false, source.Token, offerDataToRequest, OfferService.MediaType.Product);
                    RequestMoreOffers(false, source.Token, offerDataToRequest / 3, OfferService.MediaType.Vertical);
                    RequestMoreOffers(false, source.Token, offerDataToRequest / 2, OfferService.MediaType.SmallTile);
#pragma warning restore 4014
                }

                if (productCount > needed - 1)
                {
                    //var offerSummaryRowContainer = GetNextRequestQueue.Dequeue();
                    PopulateRow(offerSummaryRowContainer, false);

                }
                else if (pagination.Index >= pagination.TotalCount && !requestInProgress)
                {
                    //Debug.LogError("Empty pool");
                    if (mediaType == OfferService.MediaType.Product || mediaType == OfferService.MediaType.Banner)
                        Data.EmptyPool(mediaType);

                    // At least show what we can
                    productCount = Data.NewItemsCount(mediaType);
                    if (productCount > 0)
                    {
                        //Debug.LogError("Filling: " + productCount + " of " + needed);
                        //var offerSummaryRowContainer = GetNextRequestQueue.Dequeue();
                        PopulateRow(offerSummaryRowContainer, false);
                    }
                }
            }
        }


        async protected Task PopulateOffers(CancellationToken cancelToken)
        {
            foreach (var row in _allRows)
            {
                if (cancelToken.IsCancellationRequested) return;

                PopulateRow(row, true);
            }

            await Task.Delay(250);
            InfinityScroll.OnSiblingUpdate -= OnSiblingUpdated;
            InfinityScroll.OnSiblingUpdate += OnSiblingUpdated;
            m_ChangingCategories = false;
        }

        private void PopulateRow(OfferSummaryRowContainer row, bool firstLoad)
        {
            try
            {
                OfferSummaryPresenterBase.Model offerData = null;
                var columns = row.Columns;
                foreach (var col in columns)
                {

                    var mediaType = ScutiUtils.RollForType(true);
                    offerData = Data.RequestOffer(mediaType);

                    // Fallback to products
                    if (offerData == null && mediaType != OfferService.MediaType.Product)
                    {
                        mediaType = OfferService.MediaType.Product;
                        offerData = Data.RequestOffer(mediaType);
                    }

                    List<OfferSummaryPresenterBase> presenters = col.GetPresenters(mediaType);

                    OfferSummaryPresenterBase.Model offerData2 = null;
                    if (presenters.Count>1)
                    {
                            offerData2 = Data.RequestOffer(mediaType);
                        if(offerData2==null)
                        {
                            offerData2 = Data.RequestOffer(OfferService.MediaType.Product);
                        }
                    }

                    // still needed?
                    //await Task.Delay((int)(instantiationInterval * 1000));

                    //if (cancelToken.IsCancellationRequested) return;

                    int count = 0;
                    foreach (var presenter in presenters)
                    {
                        var oData = offerData;
                        if (count > 0) oData = offerData2;

                        presenter.gameObject.hideFlags = HideFlags.DontSave;

                        if (oData == null)
                        {
                            //Debug.LogError("NULL on : " + presenter + " " + row + "   Prods: " + Data.NewItemsCount(OfferService.MediaType.Product) + "   Small: " + Data.NewItemsCount(OfferService.MediaType.SmallTile) + "   Tall: " + Data.NewItemsCount(OfferService.MediaType.Vertical) +"  attempted: "+ mediaType);
                            continue;
                        }



                        oData.DisplayAd = (mediaType != OfferService.MediaType.Product);
                        presenter.Data = oData;
                        presenter.OnLoaded -= OnWidgetLoaded;
                        presenter.OnLoaded += OnWidgetLoaded;
                        presenter.Data.IsTall = (mediaType == OfferService.MediaType.Vertical);
                        presenter.Data.LoadImage();

                        presenter.OnClick -= OnPresenterClicked;
                        presenter.OnClick += OnPresenterClicked;
                        count++;
                    }
                }
            }
            catch (Exception e)
            {
                ScutiLogger.LogException(e);
            }

        }

        protected void OnWidgetLoaded(OfferSummaryPresenterBase widget)
        {
            widget.Show();
            widget.DisplayCurrentImage();
            widget.ResetTimer();
        }

        private async void OnPresenterClicked(OfferSummaryPresenterBase presenter)
        {
            if (presenter.Data != null && !presenter.Data.ID.IsNullOrEmpty())
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
        }

#endregion
    }
}
