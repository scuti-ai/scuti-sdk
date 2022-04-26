using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Scuti.Net;
using TMPro;
using System.Timers;

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
			public string ShopURL;
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

            public bool IsTall = false;


            [SerializeField] Texture2D texture;
			[SerializeField] Texture2D shoptexture;
			public Texture2D Texture { get { return texture; } }
			public Texture2D Shoptexture { get { return shoptexture; } }

			public async void LoadImage()
            {
                try
                {
                    if (texture == null)
                    {
                        CurrentState = State.Loading;

                        var url = ImageURL;


                        // had to check if image was from shopify because some images wasn't from shopify and I was getting an error
                        if (!String.IsNullOrEmpty(url) && url.IndexOf("shopify") != -1 && url.LastIndexOf(".") != -1)
                            url = url.Insert(url.LastIndexOf("."), "_large");

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
                            //Debug.LogError("DisplayAd is false because: " + IsTall + "  " + TallURL + " and " + SmallURL +"  on "+Title);
                        }
                        if (!string.IsNullOrEmpty(url))
                        {
                            //Debug.Log(url + " and " + DisplayAd);
                            var result = await ImageDownloader.New().Download(url);
                            if (result != null)
                            {
                                texture = result;
                                CurrentState = State.Loaded;
                            } else
                            {
                                ScutiLogger.LogError("Failed to load: " + url + " for " + Title);
                                CurrentState = State.Failed;
                            }
                        }
                        else
                        {
#if UNITY_EDITOR
                            ScutiLogger.LogError("No URL for " + this.ToJson());
#endif
                            CurrentState = State.Failed;
                        }
                    }
                    else
                    {
                        CurrentState = State.Loaded;
                    }
                } catch
                {
                    CurrentState = State.Failed;
                }
            }

			public async void LoadShopImage()
			{
                try
                {

				var url = ShopURL;

                if (!string.IsNullOrEmpty(url))
				{
                    var result = await ImageDownloader.New().Download(url);
                    if (result != null)
                    {
                        shoptexture = result;
                    } 
                }
				else
				{

                    if (shoptexture != null) Destroy(shoptexture);
                    shoptexture = null;
                }
                }catch
                {

                    if (shoptexture != null) Destroy(shoptexture);
                    shoptexture = null;
                }
			}

			public override void Dispose()
            {
                OnDispose?.Invoke(this);

                //Debug.Log("Dispose " + this);
                base.Dispose();

                OnStateChanged = null;
                if (texture != null) Destroy(texture);
                texture = null;
            }
        }



        public OfferSummaryPresenterBase.Model Next { get; protected set; }

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

        public bool IsTall = false; 
        public bool Single = false;
        public bool FirstColumn = false;
        public event Action<OfferSummaryPresenterBase> OnClick;

        /// <summary>
        /// Fired when the model loads an image. True if this is the
        /// first model that the instance loads.
        /// </summary>
        public event Action<OfferSummaryPresenterBase> OnLoaded;

        public bool HasData
        {
            get { return Data != null && !Data.ID.IsNullOrEmpty(); }
        }

        [SerializeField] protected Animator animator;
        //[SerializeField] protected Timer timer;

        [Header("Fields")]
        [SerializeField] protected Image backgroundImage;
        [SerializeField] protected Image displayImage;
        [SerializeField] protected Image shopImage;
        [SerializeField] public TextMeshProUGUI titleText;
        [SerializeField] protected TextMeshProUGUI brandText;
       
        public RectTransform Viewport;


        float m_TimerDuration;
        float m_PriorSpeed = 1;
        protected bool m_showing = false;

        protected bool _isStatic = false;

        System.Timers.Timer _portraitImpressionTimer = new System.Timers.Timer();

        [Serializable]
        public struct VisualRules
        {
            public GameObject Visual;
            public bool HideIfStatic;
        }

        public VisualRules[] ProductVisualRules;

        RectTransform rect;
        bool _lastVisibleState = false;

        // ================================================
        #region LICECYCLE
        // ================================================

        protected override void Awake()
        {
            base.Awake();
            rect = GetComponent<RectTransform>();
            _portraitImpressionTimer.Elapsed += ImpressionTimer_Elapsed;
            //_portraitImpressionTimer.Interval = ScutiConstants.SCUTI_VALID_IMPRESSION_DURATION * 1000;
            //_portraitImpressionTimer.Enabled = false;
        }

        private void ImpressionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Debug.LogError("      -------->> Record impression for: " + Data.Title );
            _portraitImpressionTimer.Enabled = false;
            //_portraitImpressionTimer.Stop();
            if(Data!=null && Data.CurrentState == Model.State.Loaded)
                ScutiAPI.RecordOfferImpression(Data.ID);
        }

        void Update()
        {
            var isHalfVisibleFrom = rect.IsHalfVisibleFrom(Viewport);
            if (!_timerPaused &&  m_showing && isHalfVisibleFrom &&  !_lastVisibleState && Data!=null && Data.CurrentState == Model.State.Loaded)
            {
                OnScreen();
                _lastVisibleState = true;
            }
            else if (_lastVisibleState && (!isHalfVisibleFrom || !m_showing || _timerPaused ))
            {
                OffScreen();
            }
        }



        private void OffScreen()
        {
            _lastVisibleState = false;
            //Debug.LogError("  --> OffScreen " + Data.Title);
            //_portraitImpressionTimer.Stop();
            _portraitImpressionTimer.Enabled = false;
        }

        private void OnScreen()
        {
            //Debug.LogError("  =0=> OnScreen "+ Data.Title + "  "/*+ ScutiConstants.SCUTI_VALID_IMPRESSION_DURATION*/);
            _portraitImpressionTimer.Interval = ScutiConstants.SCUTI_VALID_IMPRESSION_DURATION * 1000;
            _portraitImpressionTimer.Enabled = true;
            //_portraitImpressionTimer.Start();
        }

    
        public void LoadNext(Model data)
        {
            //Clear();
            if (Next != null)
            {
                Next.OnStateChanged -= OnNextStateChanged;
            }
            Next = data;
            Next.IsTall = data.IsTall;
            Next.LoadImage();
            Next.OnStateChanged -= OnNextStateChanged;
            Next.OnStateChanged += OnNextStateChanged;
        }

        protected virtual void SwapToNext()
        {
            if (Next != null)
            {
                //todo: what happens if we scroll too fast and Next is not ready? -mg
                Next.OnStateChanged -= OnNextStateChanged;
                Data = Next;
                Next = null;
                DisplayCurrentImage();
                ResetTimer();
                LoadCompleted();
            }
        }

        protected virtual void OnNextStateChanged(Model.State state)
        {
            switch (state)
            {
                case Model.State.Loaded:
                    if (Next != null)
                    {
                        SwapToNext();
                    }
                    break;
                case Model.State.Failed:
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
        }

        private void CleanUp(Sprite sprite)
        {
            try
            {
                if (sprite != null)
                {
                   // Destroy(sprite.texture);
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
            if (OnLoaded != null) OnLoaded.Invoke(this);
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
                m_showing = true;
                animator?.SetTrigger("LoadingFinished");
            }
        }

        public void DisplayCurrentImage()
        {
            if (!_destroyed && Data!=null)
            {
                if (Data.Texture && (displayImage.sprite == null || Data.Texture != displayImage.sprite.texture))
                {
                    CleanUp(displayImage.sprite);
                    //displayImage.material.SetTexture("_MainTex", Data.Texture);
                    displayImage.sprite = Data.Texture.ToSprite();
                }

				if (Data.Shoptexture && (shopImage.sprite == null || Data.Shoptexture != shopImage.sprite.texture))
				{
					CleanUp(shopImage.sprite);
					shopImage.sprite = Data.Shoptexture.ToSprite();
				}

				//          if (Data.DisplayAd && Single)
				//          {
				//              //AdContainer.SetActive(true);
				////              foreach (var p in ProductVisualRules)
				////              {
				////if(p.Visual != null)
				////	p.Visual.SetActive(false);
				////              }


				//          }
				//          else
				//          {
				//              // Here doble offer
				//             // AdContainer.SetActive(false);
				//              foreach (var p in ProductVisualRules)
				//              {
				//                  // Blogs Here
				//                  //p.Visual.SetActive(!_isStatic || !p.HideIfStatic);
				//              }
				//              if (Data.Texture && ( displayImage.sprite == null || Data.Texture!=displayImage.sprite.texture))
				//              {
				//                  displayImage.sprite = Data.Texture.ToSprite();
				//              }
				//          }
			}
		}



        public void SetDuration(float duration)
        {
            m_TimerDuration = duration;
        }

        public void ResetTimer()
        {
            OffScreen();
            _lastVisibleState = false;
        }

        private bool _timerPaused = false;
        

        public void PauseTimer()
        {
            _timerPaused = true;
        }

        public void ResumeTimer()
        {
            _timerPaused = false;
        }
        #endregion

        // ================================================
        #region PRESENTER
        // ================================================

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
                    //ScutiLogger.Log("Could not load summary image.");
                    break;
            }
        }

        // Updates UI based on values on View.Data
        protected virtual void UpdateUI()
        {
            titleText.text = TextElipsis(Data.Title); 
            if(brandText!=null) brandText.text = Data.Brand; 
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
