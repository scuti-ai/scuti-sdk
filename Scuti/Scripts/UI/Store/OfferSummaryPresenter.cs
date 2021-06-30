using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Scuti.Net;

namespace Scuti.UI
{
    public class OfferSummaryPresenter : Presenter<OfferSummaryPresenter.Model>
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

            public string ImageURL;
            public string TallURL;
            public string SmallURL;
            public string VideoURL;
            public string Title;
            public string DisplayPrice;
            public string Description;
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
                        if (DisplayAd)
                        {
                            if (IsTall && !string.IsNullOrEmpty(TallURL))
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
                        CurrentState = State.Failed;
                    }
                }
            }

            public override void Dispose()
            {
                base.Dispose();
                if (texture != null) Destroy(texture);
            }
        }

        /// <summary>
        /// Fired when the model loads an image. True if this is the
        /// first model that the instance loads.
        /// </summary>
        public event Action<bool, OfferSummaryPresenter> OnLoaded;

        public event Action OnClick;

        public delegate Task<Model> GetNext();

        public Model Next { get; private set; }
        public bool HasNext
        {
            get { return Next != null && !Next.ID.IsNullOrEmpty(); }
        }

        public bool HasData
        {
            get { return Data != null && !Data.ID.IsNullOrEmpty(); }
        }

        [SerializeField] Animator animator;
        [SerializeField] Timer timer;

        [Header("Fields")]
        [SerializeField] Image backgroundImage;
        [SerializeField] Image displayImage;
        [SerializeField] Text titleText;
        [SerializeField] Text displayPriceText;
        [SerializeField] Text ratingText;
        [SerializeField] RatingStarsWidget ratingStarsWidget;

        [Header("Badges")]
        [SerializeField] GameObject newBadge;
        [SerializeField] GameObject hotBadge;

        [Header("Promos")]
        [SerializeField] GameObject hotPricePromo;
        [SerializeField] GameObject recommendedPromo;
        [SerializeField] GameObject specialOfferPromo;
        [SerializeField] GameObject bestsellerPromo;
        [SerializeField] GameObject scutiPromo;
        [SerializeField] Image GlowImage;
        [SerializeField] GameObject loadingVFX = null;

        float m_TimerDuration;
        GetNext m_NextRequest;
        float m_PriorSpeed = 1;
        private bool m_showing = false;

        private bool timerCompleted = false;
        private bool loadingNextCompleted = false;
        private bool _isStatic = false;
        private bool _isPortrait = false;

        [Serializable]
        public struct VisualRules
        {
            public GameObject Visual;
            public bool HideIfStatic;
        }

        public GameObject AdContainer;
        public VisualRules[] ProductVisualRules;

        public Image AdImage;
        public bool IsTall = false;


        // ================================================
        #region LICECYCLE
        // ================================================
        public void Inject(GetNext getNextMethod)
        {
            m_NextRequest = getNextMethod;
        }

        protected override void Awake()
        {
            base.Awake();

            float pixelsWide = Camera.main.pixelWidth;
            float pixelsHigh = Camera.main.pixelHeight;

            _isPortrait = pixelsHigh > pixelsWide;
            if (ScutiConstants.FORCE_LANDSCAPE)
            {
                _isPortrait = false;
            }

            timer.gameObject.SetActive(false);
            AdContainer.SetActive(false);
            timerCompleted = false;
            timer.Pause();
            timer.onFinished.AddListener(OnTimerCompleted);
            timer.onCustomEvent += OnTimerCustomEvent;
            GlowImage.gameObject.SetActive(false);

            // hiding for now, I'm not a fan of it -mg
            if (loadingVFX != null) loadingVFX.SetActive(false);
        }


        private void OnTimerCustomEvent(string id)
        {
            switch(id)
            {
                case ScutiConstants.SCUTI_IMPRESSION_ID:
                    try
                    {
                        ScutiAPI.RecordOfferImpression(Data.ID);
                    }
                    catch
                    {

                    }
                    break;
            }
        }

        private void OnTimerCompleted()
        {
            timerCompleted = true;
            CheckReady();
        }

        public void OnRotateOutComplete()
        {
            if (HasNext)
            {
                Data = Next;
                Next = null;
                DisplayCurrentImage();
                ResetTimer();
                LoadCompleted();
            }
        }

        private void LoadCompleted()
        {
            OnLoaded?.Invoke(!HasNext, this);
            LoadNext();
        }

        private async void LoadNext()
        {
            loadingNextCompleted = false;
            if (!_isStatic)
            {
                Next = await m_NextRequest();
                if (Next != null)
                {
                    Next.IsTall = IsTall;
                    Next.OnStateChanged += OnNextStateChanged;
                    Next.LoadImage();
                }
            }
        }

        public void Click()
        {
            OnClick?.Invoke();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
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
                    Destroy(sprite.texture);
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
        #endregion

        // ================================================
        #region API
        // ================================================
        public void SetColorData(Sprite bg, Color32 color)
        {
            if (!_destroyed)
            {
                backgroundImage.sprite = bg;
                GlowImage.color = color;
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
                if (Data.IsMoreExposure && !_isStatic)
                {
                    GlowImage.gameObject.SetActive(true);
                }
                else
                {
                    GlowImage.gameObject.SetActive(false);
                }
                m_showing = true;
                ResumeTimer();
                animator?.SetTrigger("LoadingFinished");
            }
        }

        public void DisplayCurrentImage()
        {
            if (!_destroyed)
            {
                if (Data.DisplayAd)
                {
                    AdContainer.SetActive(true);
                    foreach (var p in ProductVisualRules)
                    {
                        p.Visual.SetActive(false);
                    }
                    AdImage.sprite = Data.Texture.ToSprite();
                }
                else
                {
                    AdContainer.SetActive(false);
                    foreach (var p in ProductVisualRules)
                    {
                        p.Visual.SetActive(!_isStatic || !p.HideIfStatic);
                    }
                    displayImage.sprite = Data.Texture.ToSprite();
                }
            }
        }

        public void CheckReady()
        {
            if (!_isPortrait && !_destroyed)
            {
                if (loadingNextCompleted && timerCompleted)
                {

                    animator.SetTrigger("Rotate");
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
                timer.gameObject.SetActive(true);
                timerCompleted = false;
                timer.ResetTime(m_TimerDuration);
                timer.AddCustomEvent(ScutiConstants.SCUTI_IMPRESSION_ID, ScutiConstants.SCUTI_VALID_IMPRESSION_DURATION);
                timer.Begin();
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
        protected override void OnSetState()
        {
            gameObject.name = Data.Title;
            UpdateUI();
#pragma warning disable
            Data.OnStateChanged += async state => {
                switch (state)
                {
                    case Model.State.Loaded:
                        LoadCompleted();
                        break;
                    case Model.State.Failed:
                        ScutiLogger.Log("Could not load summary image.");
                        Next = null;
                        break;
                }
            };
#pragma warning restore
        }

        private void OnNextStateChanged(Model.State state)
        {
            switch (state)
            {
                case Model.State.Loaded:
                    loadingNextCompleted = true;
                    CheckReady();
                    break;
                case Model.State.Failed:
                    LoadNext();
                    break;
            }
        }

	    
	    

        // Updates UI based on values on View.Data
        void UpdateUI()
        {
            titleText.text = Data.Title;
            displayPriceText.text = ScutiUtils.FormatPrice(Data.DisplayPrice);
            var isPortrait = ScutiUtils.IsPortrait();

            // New and Hot Badges only in portrait
            //if (isPortrait)
            //{
            //    newBadge.SetActive(Data.IsNew);
            //    hotBadge.SetActive(Data.IsNew ? false : Data.IsHot);
            //}
            //else
            //{
                newBadge.SetActive(false);
                hotBadge.SetActive(false);
            //}

            // Show ONLY THE FIRST promo that is applicable
            var list = new List<KeyValuePair<GameObject, bool>> {
                new KeyValuePair<GameObject, bool>(hotPricePromo, Data.IsHotPrice),
                new KeyValuePair<GameObject, bool>(recommendedPromo, Data.IsRecommended),
                new KeyValuePair<GameObject, bool>(specialOfferPromo, Data.IsSpecialOffer),
                new KeyValuePair<GameObject, bool>(bestsellerPromo, Data.IsBestSeller),
                new KeyValuePair<GameObject, bool>(scutiPromo, Data.IsScuti)
            };

            list.ForEach(x => x.Key.SetActive(false));

            if (isPortrait)
            {
                foreach (var pair in list)
                {
                    if (pair.Value)
                    {
                        pair.Key.SetActive(true);
                        break;
                    }
                }
            }

            GlowImage.gameObject.SetActive(false);

            // Show the rating if there is a rating
            bool hasRatingValue = Data.Rating > 0f && isPortrait;

            ratingText.gameObject.SetActive(hasRatingValue);
            ratingStarsWidget.gameObject.SetActive(hasRatingValue);
            if (hasRatingValue)
            {
                ratingText.text = Data.Rating.ToString("0.0");
                ratingStarsWidget.Value = Data.Rating / ratingStarsWidget.Levels;
            }
        }
        #endregion
    }
}
