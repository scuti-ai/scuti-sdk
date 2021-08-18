using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using Image = UnityEngine.UI.Image;


using Scuti.GraphQL.Generated;

using LoadedWidgetQueue = System.Collections.Generic.Queue<System.Tuple<Scuti.UI.OfferSummaryPresenterBase, bool>>;
using GetNextRequestQueue = System.Collections.Generic.Queue<System.Action<Scuti.UI.OfferSummaryPresenterBase.Model>>;
using System.Threading;
using Scuti.Net;
using UnityEngine.Events;

namespace Scuti.UI
{
    public class OffersPresenterBase : Presenter<OffersPresenterBase.Model>
    {
        [Serializable]
        public class Model : Presenter.Model
        {
            public List<OfferSummaryPresenterBase.Model> Items = new List<OfferSummaryPresenterBase.Model>();
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
        [SerializeField] protected bool clearInitialElements = true;

        protected List<GameObject> initialElements = new List<GameObject>();

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
        protected Pagination m_Pagination;
        protected int m_offerIndex;
        protected Dictionary<string, Pagination> m_PaginationMap = new Dictionary<string, Pagination>();
        protected List<OfferSummaryPresenterBase> m_Instantiated = new List<OfferSummaryPresenterBase>();


        protected CancellationTokenSource _loadingSource;
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
            TimeoutTimer.onFinished.AddListener(OnTimeout); 
            if (categoryNavigator)
                categoryNavigator.OnOpenRequest += ShowCategory;
            ProcessLoadedWidgetQueue();
        }

        private void Update()
        {
#if UNITY_IOS || UNITY_ANDROID
            if(Input.touchCount>0)
#else
            if(Input.anyKey || Input.GetAxis("Mouse X") != 0)
#endif
            { 
                ResetTimeout();
            }
            ProcessGetNextRequestQueue();
        }


        async void ProcessLoadedWidgetQueue()
        {
            Debug.LogError("=========== ProcessLoadedWidgetQueue ===========");
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
            m_PaginationMap.Clear();
            if(m_Pagination!=null)
                m_Pagination.VideoIndex = 0;
        }

       
#endregion

        // ================================================
#region CATEGORY AND PAGINATION
        // ================================================
        public async void ShowCategory(string category)
        {

            if (TrySetCategory(category))
            { 
                m_ChangingCategories = true;
                Clear();
                await ShowCategoryHelper();
                await RequestMoreOffers(true);
            } 
            UIManager.HideLoading(true);
        }

        protected virtual async Task ShowCategoryHelper() { }

     
         
        public bool TrySetCategory(string category)
        {
            if (m_Pagination != null)
            {
                if (m_Pagination.Category != null && m_Pagination.Category.Equals(category))
                {
                    return false;
                }
            }
            UpdatePagination(category);
            return true;
        }

        void UpdatePagination(string categoryName)
        {
            string categoryValue = categoryName;
            if (categoryName.Equals("DEFAULT"))
            {
                categoryValue = null;
            }

            if (!m_PaginationMap.ContainsKey(categoryName))
                m_PaginationMap[categoryName] = new Pagination()
                {
                    Category = categoryValue,
                    Index = 0,
                    VideoIndex = 0
                };

            m_Pagination = m_PaginationMap[categoryName];
        }
#endregion

        // ================================================
#region API
        // ================================================
        /// <summary>
        /// Returns a list of offers, based on the current paginataion status
        /// </summary>
        public async Task RequestMoreOffers(bool replaceData)
        {
            var index = m_Pagination.Index;
            var maxCount = offerDataToRequest;

            //Debug.LogWarning("Requesting Range   index:" + index + "  m_Pagination.Index:" + m_Pagination.Index + "  maxcount:" + maxCount + "  retry:" + retry);
            m_Pagination.Index += maxCount;
            OfferPage offerPage = null;
            try
            {
                offerPage = await ScutiNetClient.Instance.Offer.GetOffers(new List<CampaignType> { CampaignType.Product, CampaignType.Product_Listing }, FILTER_TYPE.In, m_Pagination.Category, null, null, index, maxCount);
            } catch (Exception e)
            {
                ScutiLogger.LogException(e);
                //Debug.LogError("TODO: show error message ");
            }

            if (offerPage != null)
            {
                m_Pagination.TotalCount = offerPage.Paging.TotalCount.GetValueOrDefault(0);
                if (replaceData)
                {
                    Data = Mappers.GetOffersPresenterModel(offerPage.Nodes as List<Offer>);
                }
                else
                {
                    var appendData = Mappers.GetOffersPresenterModel(offerPage.Nodes as List<Offer>);
                    Data.Items.AddRange(appendData.Items);
                }
            }
            else
            {
                if (replaceData)
                {
                    m_Pagination.TotalCount = 0;
                    Data = null;
                }
            }
            //Debug.LogWarning("      actualCount" + actualCount  + "  maxcount:" + maxCount );
            ////if (actualCount < maxCount)
            ////{
            ////    if (m_Pagination.Index > index || resetOverride) m_Pagination.Index = 0; // only reset if it hasn't been already by another offer

            ////    if (actualCount == 0 && retry)
            ////    {
            ////        //Debug.LogError("      *--* m_Pagination.Index:" + m_Pagination.Index);
            ////        return await GetRange(index, maxCount, false);
            ////    }
            ////    else
            ////    {
            ////        // Attempt to wrap back to the start
            ////        index = m_Pagination.Index;
            ////        m_Pagination.Index += maxCount;
            ////        //Debug.LogError("      ** m_Pagination.Index:" + m_Pagination.Index);
            ////        var results = await ScutiNetClient.Instance.Offer.GetOffers(new List<CampaignType> { CampaignType.Product, CampaignType.Product_Listing }, FILTER_TYPE.In, m_Pagination.Category, null, null, index, maxCount - actualCount);
            ////        //Debug.LogError("            ** results -->:" + results.Count);
            ////        if(results!=null && results.Nodes.Count>0)
            ////            offerPage.Nodes.AddRange(results.Nodes);
            ////    }
            ////}
        }

        // Maintains a queue of requests that fetches them one by one. This is 
        // crucial when two offer timers get over pretty much together, the 
        // index for both their next offer requests will be the same and they will
        // get the same offers.
        public Task<OfferSummaryPresenterBase.Model> GetNext()
        {
            var source = new TaskCompletionSource<OfferSummaryPresenterBase.Model>();
            GetNextRequestQueue.Enqueue(model => source.SetResult(model));
            return source.Task;
        }

        public virtual void Clear()
        {
            m_offerIndex = 0;
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
            Clear();
#pragma warning disable 4014
            _loadingSource = new CancellationTokenSource();
            PopulateOffers(_loadingSource.Token);
#pragma warning restore 4014
        }

        void ProcessGetNextRequestQueue()
        {
            bool shouldUpdate = ShouldUpdateOffers;
            if (shouldUpdate && m_Paused) ResumeAds();
            else if (!shouldUpdate && !m_Paused) PauseAds();

            if (!m_Paused && GetNextRequestQueue.Count != 0)
            {
                if(m_offerIndex > m_Pagination.Index/2 && m_Pagination.Index<m_Pagination.TotalCount)
                {
                    Debug.Log("Requesting Data:" + m_offerIndex+ " vs " + Data.Items.Count + " index: " + m_Pagination.Index +" and " + m_Pagination.TotalCount);
#pragma warning disable 4014
                    RequestMoreOffers(false);
#pragma warning restore 4014
                }
                if (m_offerIndex < Data.Items.Count)
                {

                    var request = GetNextRequestQueue.Dequeue();
                    Debug.LogError("=========== ProcessGetNextRequestQueue =========== " + m_offerIndex);
                    var model = Data.Items[m_offerIndex];
                    model.Index = m_offerIndex;
                    m_offerIndex++;
                    request?.Invoke(model);
                } else if(m_offerIndex >= m_Pagination.TotalCount)
                {
                    m_offerIndex = 0;
                }
            }
        }

        async protected virtual Task PopulateOffers(CancellationToken cancelToken)
        {
            
        }
         

        protected void OnWidgetLoaded(bool initial, OfferSummaryPresenterBase widget)
        {
            loadedWidgetQueue.Enqueue(new Tuple<OfferSummaryPresenterBase, bool>(widget, initial));
        }

        protected OfferColorData GetColorInfo(int index)
        {
            if (index >= colorInfo.Length) index = colorInfo.Length - (index % colorInfo.Length)-1;
            return colorInfo[index];
        }

        
#endregion
    }
}