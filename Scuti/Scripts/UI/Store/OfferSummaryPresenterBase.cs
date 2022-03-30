using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Scuti.Net;
using TMPro;

namespace Scuti.UI
{
    public class OfferSummaryPresenterBase : Presenter<OfferSummaryPresenterBase.Model>
    {
        [Serializable]
        public class Model : Presenter.Model
        {
            public enum State
            {
                Loading,
                Loaded,
                Failed
            }

            public event Action<Model> OnDispose;
            public event Action<State> OnStateChanged;

            [SerializeField] State state;
            public State CurrentState
            {
                get { return state; }
                private set
                {
                    state = value;
                    OnStateChanged?.Invoke(state);
                }
            }
            public string ID;

            public int Index;


            public string ImageURL;
            public string TallURL;
            public string SmallURL;
            public string VideoURL;
            public string Title;
            public string DisplayPrice;
            public string Description;
            public string Brand;
            public float Rating;

            public int Scutis;

            public bool IsNew;
            public bool IsHot;

            public bool IsRecommended;
            public bool IsHotPrice;
            public bool IsBestSeller;
            public bool IsFeatured;
            public bool IsMoreExposure;
            public bool IsSpecialOffer;
            public bool IsScuti;

            public bool DisplayAd = false;
            public bool IsTall = false;
            public bool isSingle = false;


            //HashSet<OfferSummaryPresenterBase> References;

            //public void AddReference(OfferSummaryPresenterBase r)
            //{
            //    References.Add(r);
            //}

            //public void RemoveReference(OfferSummaryPresenterBase r)
            //{
            //    References.Remove(r);
            //    if(References.Count<1)
            //    {
            //        GarbageCollect();
            //    }
            //}


            [SerializeField] Texture2D texture;
            public Texture2D Texture { get { return texture; } }

            public void LoadImage()
            {
                if (texture == null)
                {
                    CurrentState = State.Loading;

                    var url = ImageURL;

                    if (!string.IsNullOrEmpty(url))
                    {
                        // had to check if image was from shopify because some images wasn't from shopify and I was getting an error
                        if (url.IndexOf("shopify") != -1 && url.LastIndexOf(".") != -1)
                            url = url.Insert(url.LastIndexOf("."), "_large");
                        if (DisplayAd)
                        {
                            if(isSingle)
                            {
                                if (IsTall && !string.IsNullOrEmpty(TallURL) )
                                {
                                    url = TallURL;

                                }
                                else if (!IsTall && !string.IsNullOrEmpty(SmallURL))
                                {                    
                                    url = SmallURL;
                                }
                                else
                                {
                                    DisplayAd = false;
                                }
                            }
                        }
                        //Debug.Log(url + " and " + DisplayAd);
                        ImageDownloader.New().Download(url,
                            result =>
                            {
                                texture = result;
                                CurrentState = State.Loaded;

                            },
                            error =>
                            {
                                ScutiLogger.LogError("Failed to load: " + url + " for " + Title);
                                CurrentState = State.Failed;
                            }
                        );
                    }
                    else
                    {
#if UNITY_EDITOR
                        ScutiLogger.LogError("No URL for " + this.ToJson());
#endif
                        CurrentState = State.Failed;
                    }
                } else
                {
                    CurrentState = State.Loaded;
                }
            }

            //private void GarbageCollect()
            //{

            //}

            public override void Dispose()
            {
                OnDispose?.Invoke(this);

                base.Dispose();

                OnStateChanged = null;
                if (texture != null) Destroy(texture);
                texture = null;
            }
        }


        public OfferSummaryPresenterBase.Model Next { get; protected set; }
        public delegate Task<Model> GetNext(OfferSummaryPresenterBase presenter);
        protected GetNext m_NextRequest;
        public void Inject(GetNext getNextMethod)
        {
            m_NextRequest = getNextMethod;
        }

        internal void Clear()
        {
            Data = null;
            ResetAnimation();
        }

        public override void SetData(Model data)
        {
            if(m_Data != null)
            {
                m_Data.OnStateChanged -= OnSetDataState;
                m_Data.OnStateChanged -= OnNextStateChanged;
            }
            base.SetData(data);
        }

        public bool Single = false;
        public bool FirstColumn = false;
        public bool FirstLoad = true;
        public event Action<OfferSummaryPresenterBase> OnClick;

        /// <summary>
        /// Fired when the model loads an image. True if this is the
        /// first model that the instance loads.
        /// </summary>
        public event Action<bool, OfferSummaryPresenterBase> OnLoaded;

        public bool HasData
        {
            get { return Data != null && !Data.ID.IsNullOrEmpty(); }
        }

        [SerializeField] protected Animator animator;
        [SerializeField] protected Timer timer;

        [Header("Fields")]
        [SerializeField] protected Image backgroundImage;
        [SerializeField] protected Image displayImage;
        [SerializeField] public TextMeshProUGUI titleText;
        [SerializeField] protected TextMeshProUGUI displayPriceText;
        [SerializeField] protected TextMeshProUGUI ratingText;
        [SerializeField] protected TextMeshProUGUI brandText;
        [SerializeField] protected RatingStarsWidget ratingStarsWidget;

        [Header("Badges")]
        [SerializeField] protected GameObject newBadge;
        [SerializeField] protected GameObject hotBadge;

        [Header("Promos")]
        [SerializeField] protected GameObject hotPricePromo;
        [SerializeField] protected GameObject recommendedPromo;
        [SerializeField] protected GameObject specialOfferPromo;
        [SerializeField] protected GameObject bestsellerPromo;
        [SerializeField] protected GameObject scutiPromo;
        [SerializeField] protected Image GlowImage;

        float m_TimerDuration;
        float m_PriorSpeed = 1;
        protected bool m_showing = false;

        protected bool timerCompleted = false;
        protected bool _isStatic = false;
        protected bool _isPortrait = false;

        System.Timers.Timer _portraitImpressionTimer = new System.Timers.Timer();

        [Serializable]
        public struct VisualRules
        {
            public GameObject Visual;
            public bool HideIfStatic;
        }

        public GameObject AdContainer;
        public VisualRules[] ProductVisualRules;

        public Image AdImage;

        RectTransform rect;
        bool _lastVisibleState = false;

        // ================================================
        #region LICECYCLE
        // ================================================

        protected override void Awake()
        {
            base.Awake();

            _isPortrait = ScutiUtils.IsPortrait();

            rect = GetComponent<RectTransform>();

            /*float pixelsWide = Camera.main.pixelWidth;
            float pixelsHigh = Camera.main.pixelHeight;

            _isPortrait = pixelsHigh > pixelsWide;
            if (ScutiConstants.FORCE_LANDSCAPE)
            {
                _isPortrait = false;
            }*/
            //Debug.LogError(this.name + " _isPortrait " + _isPortrait);
            /*if (_isPortrait)
            {
                _portraitImpressionTimer.Elapsed += _portraitImpressionTimer_Elapsed;
                _portraitImpressionTimer.Interval = ScutiConstants.SCUTI_VALID_IMPRESSION_DURATION * 1000;
            }*/            

            timerCompleted = false;

            // Blogs Here
            /*
            if (!_isPortrait)
            {
                timer.gameObject.SetActive(false);
                timer.Pause();
                timer.onFinished.AddListener(OnTimerCompleted);
                timer.onCustomEvent += OnTimerCustomEvent;
            }
            else
            {
                timer.Pause();
                timer.onFinished.AddListener(RecordOfferImpression);
            }
            */
            //AdContainer.SetActive(false);
            //GlowImage.gameObject.SetActive(false);

        }

        private void _portraitImpressionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _portraitImpressionTimer.Stop();
            if(Data!=null)
                ScutiAPI.RecordOfferImpression(Data.ID);
        }

        private void RecordOfferImpression()
        {
            timer.Pause();
            if (Data != null)
                ScutiAPI.RecordOfferImpression(Data.ID);
        }

        void Update()
        {
            if (!_isPortrait || Data == null)
                return;

            //rect.IsFullyVisibleFrom();
            if (rect.IsHalfVisibleFrom() && rect.IsHalfVisibleFrom() != _lastVisibleState)
            {
                _lastVisibleState = true;
                //Blogs here
                //timer.ResetTime(ScutiConstants.SCUTI_VALID_IMPRESSION_DURATION);
                //timer.Begin();
                //_portraitImpressionTimer.Interval = ScutiConstants.SCUTI_VALID_IMPRESSION_DURATION * 1000;
                //_portraitImpressionTimer.Start();
            }
            else if (!rect.IsHalfVisibleFrom() && rect.IsHalfVisibleFrom() != _lastVisibleState)
            {
                _lastVisibleState = false;
                //Blogshere
                //timer.Pause();
                //_portraitImpressionTimer.Stop();
            }

        }

        private void OnTimerCustomEvent(string id)
        {
            switch(id)
            {
                case ScutiConstants.SCUTI_IMPRESSION_ID:
                    try
                    {
                        if (Data != null)
                            ScutiAPI.RecordOfferImpression(Data.ID);
                    }
                    catch
                    {

                    }
                    break;
            }
        }

        protected virtual void OnTimerCompleted()
        {
            timerCompleted = true;
        }

        public void OnScrollIndexJumped()
        {
            SwapToNextHelper();
        }

        public void OnRotateOutComplete()
        {
            SwapToNext();
        }

        protected virtual void SwapToNext()
        {
            if (Next != null)
            {
                Next.OnStateChanged -= OnNextStateChanged;
                //Debug.Log("Loaded Next: " + Next.Title);
                Data = Next;
                Next = null;
                DisplayCurrentImage();
                ResetTimer();
                LoadCompleted();
            }
        }

        protected async virtual void SwapToNextHelper()
        {
            Clear();
            if(Next!=null)
            {
                Next.OnStateChanged -= OnNextStateChanged;
            }
            Next = await m_NextRequest(this);

            Next.IsTall = false;
            Next.isSingle = Single;
            //Debug.LogError("Loading next "+ gameObject.GetInstanceID());
            Next.LoadImage();
            Next.OnStateChanged -= OnNextStateChanged;
            Next.OnStateChanged += OnNextStateChanged;
        }

        protected virtual void OnNextStateChanged(Model.State state)
        {
            //Debug.LogError("On Next completed: " + gameObject);
            switch (state)
            {
                case Model.State.Loaded:
                    if (Next != null) Next.OnStateChanged -= OnNextStateChanged;
                    SwapToNext();
                    break;
                case Model.State.Failed:
                    if (Next != null) Next.OnStateChanged -= OnNextStateChanged;
                    Clear();
                    break;
            }
        }

        public void Click()
        {
            OnClick?.Invoke(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            animator = null;
            if (displayImage != null)
            {
                CleanUp(displayImage.sprite);
                displayImage.sprite = null;
            }
            if (AdImage != null)
            {
                CleanUp(AdImage.sprite);
                AdImage.sprite = null;
            }
        }

        private void CleanUp(Sprite sprite)
        {
            try
            {
                if (sprite != null)
                {
                    //Destroy(sprite.texture);
#if !UNITY_EDITOR
                    Destroy(sprite);
#endif
                    sprite = null;
                }
            }
            catch (Exception e)
            {
                ScutiLogger.LogWarning(e);
            }
        }

        protected virtual void LoadCompleted()
        {
            if (OnLoaded != null) OnLoaded.Invoke(IsFirstLoad(), this);
            FirstLoad = false;
        }

        protected virtual bool IsFirstLoad()
        {
            return FirstLoad;
        }

        #endregion

        // ================================================
        #region API
        // ================================================
        public void SetColorData(Sprite bg, Color32 color)
        {
            if (!_destroyed)
            {
                // Comment for no differnte background
                //backgroundImage.sprite = bg;
                //GlowImage.color = color;
            }
        }

        public void SetStatic()
        {
            _isStatic = true;
        }


        public void Show()
        {
            if (!_destroyed)
            {
                if (Data!=null && Data.IsMoreExposure && !_isStatic)
                {
                    //Blogs here
                   // GlowImage.gameObject.SetActive(true);
                }
                else
                {
                    // Blogs Here
                    //GlowImage.gameObject.SetActive(false);
                }
                m_showing = true;
                ResumeTimer();
                animator?.SetTrigger("LoadingFinished");
            }
        }

        public void DisplayCurrentImage()
        {
            if (!_destroyed && Data!=null)
            {
                if (Data.DisplayAd && Single)
                {
                    AdContainer.SetActive(true);
                    foreach (var p in ProductVisualRules)
                    {
                        p.Visual.SetActive(false);
                    }
                    if(Data.Texture && (displayImage.sprite == null || Data.Texture != displayImage.sprite.texture))
                    {
                        AdImage.sprite = Data.Texture.ToSprite();
                    }
                       
                }
                else
                {
                    // Here doble offer
                    //Blogs here
                   // AdContainer.SetActive(false);
                    foreach (var p in ProductVisualRules)
                    {
                        // Blogs Here
                        //p.Visual.SetActive(!_isStatic || !p.HideIfStatic);
                    }
                    if (Data.Texture && ( displayImage.sprite == null || Data.Texture!=displayImage.sprite.texture))
                    {
                        displayImage.sprite = Data.Texture.ToSprite();
                    }
                }
            }
        }



        public void SetDuration(float duration)
        {
            m_TimerDuration = duration;
        }

        public void ResetTimer()
        {
            if (!_isPortrait && !_destroyed && !_isStatic)
            {
                // BlogsHere
                //timer.gameObject.SetActive(true);
                //timerCompleted = false;
                //timer.ResetTime(m_TimerDuration);
                //timer.AddCustomEvent(ScutiConstants.SCUTI_IMPRESSION_ID, ScutiConstants.SCUTI_VALID_IMPRESSION_DURATION);
                //timer.Begin();
            }
        }

        public void PauseTimer()
        {
            if (!_isPortrait && !_destroyed && timer != null)
            {
                if (animator.speed != 0) m_PriorSpeed = animator.speed;
                animator.speed = 0;
                timer.Pause();
            }
        }

        public void ResumeTimer()
        {
            if (!_isPortrait && !_destroyed && timer != null && m_showing && !_isStatic)
            {
                timer.gameObject.SetActive(true);
                animator.speed = m_PriorSpeed;
                timer.Begin();
            }
        }
        #endregion

        // ================================================
        #region PRESENTER
        // ================================================
        public virtual OfferService.MediaType RollForMediaType()
        {
            var mediaType = OfferService.MediaType.Product;
            var rand = UnityEngine.Random.Range(0, 4);
            if (!ScutiUtils.IsPortrait() && this is OfferSummaryPresenterLandscape)
            {
                var landscape = this as OfferSummaryPresenterLandscape;
                
                if (landscape.IsTall)
                {
                    // 50% chance
                    if (rand > 1) mediaType = OfferService.MediaType.Vertical;
                }
                else
                {
                    // 25% chance
                    if (rand == 0) mediaType = OfferService.MediaType.SmallTile;
                }
            }
            else
            {
                if (Single)
                {
                    if (rand > 1) mediaType = OfferService.MediaType.SmallTile;
                }
            }
            return mediaType;
        }

        protected override void OnSetState()
        {
            if (Data != null)
            {
                gameObject.name = Data.Title;
                UpdateUI();
#pragma warning disable
                Data.OnStateChanged += OnSetDataState;
#pragma warning restore
            } else 
            {
                //Debug.LogError("Null state being set on " + gameObject.name +" "+gameObject.GetInstanceID());
                gameObject.name = "Cleared";
            }
        }

        protected virtual void OnSetDataState(Model.State state)
        {
            switch (state)
            {
                case Model.State.Loaded:
                    if (Data != null) Data.OnStateChanged -= OnSetDataState;
                    LoadCompleted();
                    break;
                case Model.State.Failed:
                    if (Data != null) Data.OnStateChanged -= OnSetDataState;
                    ScutiLogger.Log("Could not load summary image.");
                    break;
            }
        }



        // Updates UI based on values on View.Data
        protected virtual void UpdateUI()
        {
            titleText.text = TextElipsis(Data.Title, Single? 26:15);
            displayPriceText.text = ScutiUtils.FormatPrice(Data.DisplayPrice);

            //  New and Hot Badges only in portrait
            if (_isPortrait)
            {
                newBadge.SetActive(Data.IsNew);
                hotBadge.SetActive(Data.IsNew ? false : Data.IsHot);
            }
            else
            {
                newBadge.SetActive(false);
                hotBadge.SetActive(false);
            }

            // Show ONLY THE FIRST promo that is applicable
            var list = new List<KeyValuePair<GameObject, bool>> {
                new KeyValuePair<GameObject, bool>(hotPricePromo, Data.IsHotPrice),
                new KeyValuePair<GameObject, bool>(recommendedPromo, Data.IsRecommended),
                new KeyValuePair<GameObject, bool>(specialOfferPromo, Data.IsSpecialOffer),
                new KeyValuePair<GameObject, bool>(bestsellerPromo, Data.IsBestSeller),
                new KeyValuePair<GameObject, bool>(scutiPromo, Data.IsScuti)
            };

            list.ForEach(x => x.Key.SetActive(false));

            if (_isPortrait)
            {
                foreach (var pair in list)
                {
                    if (pair.Value)
                    {
                        pair.Key.SetActive(true);
                        pair.Key.transform.localScale = Vector3.zero;
                        break;
                    }
                }
            }

            GlowImage.gameObject.SetActive(false);
            brandText.text = Data.Brand;
            // Show the rating if there is a rating
            bool hasRatingValue = Data.Rating > 0f && _isPortrait;


            ratingText.gameObject.SetActive(hasRatingValue);
            ratingStarsWidget.gameObject.SetActive(hasRatingValue);
            if (hasRatingValue)
            {
                ratingText.text = Data.Rating.ToString("0.0");
                ratingStarsWidget.Value = Data.Rating / ratingStarsWidget.Levels;
            }
        }

        protected string TextElipsis(string text, int truncateSize = 26)
        {
	        if(text!=null && text.Length > truncateSize) return text.Remove(truncateSize) + "...";
	        return text;
        }
        private void OnEnable()
        {
            if(m_showing)
                animator?.SetTrigger("LoadingFinished");
        }

        public void ResetAnimation()
        {
            animator?.SetTrigger("RestartLoading");
        }
        #endregion

    }
}
