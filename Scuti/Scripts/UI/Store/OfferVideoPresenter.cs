

using Scuti.Net;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Scuti.UI
{
    public class OfferVideoPresenter : Presenter<OfferVideoPresenter.Model>
    {

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
                        ImageDownloader.New().Download(url,
                            result =>
                            {
                                texture = result;
                                CurrentState = State.Loaded;

                            },
                            error =>
                            {
                                ScutiLogger.LogError("Failed to load video thumb: " + url + " for " + ID);
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

        public Image ThumbImage;
        public Timer timer;
        float m_TimerDuration;

        protected override void Awake()
        {
            base.Awake();
            timer.Pause();
            timer.onFinished.AddListener(OnTimerCompleted);
            timer.onCustomEvent += OnTimerCustomEvent;
        }


        private void OnTimerCustomEvent(string id)
        {
            switch (id)
            {
                case ScutiConstants.SCUTI_IMPRESSION_ID:
                    try
                    {
                        ScutiAPI.RecordOfferImpression(Data.ID);
                    }
                    catch(Exception e)
                    {
                        ScutiLogger.LogError("Failed to log video impression: " + e.Message);
;                    }
                    break;
            }
        }

        private void OnTimerCompleted()
        {

        }

        public void SetDuration(float duration)
        {
            m_TimerDuration = duration;
        }

        public void ResetTimer()
        {
            if (!_destroyed)
            {
                timer.ResetTime(m_TimerDuration);
                timer.AddCustomEvent(ScutiConstants.SCUTI_IMPRESSION_ID, ScutiConstants.SCUTI_VALID_IMPRESSION_DURATION);
                timer.Begin();
            }
        }

        public void PauseTimer()
        {
            if (!_destroyed && timer != null)
            {
                timer.Pause();
            }
        }

        public void ResumeTimer()
        {
            if (!_destroyed && timer != null)
            {
                timer.Begin();
            }
        }

        // Event Handlers
        public void ClickPlay()
        {
            LoadVideo();
        }

        protected override void OnSetState()
        {
            base.OnSetState();

            if (Data != null && Data.ImageURL != null)
            {
                Data.OnStateChanged += Data_OnStateChanged;
                Data.LoadImage();
            }
        }

        private void Data_OnStateChanged(Model.State state)
        {
            switch (state)
            {
                case Model.State.Loaded:
                    ThumbImage.sprite = Data.Texture.ToSprite();
                    ThumbImage.color = Color.white;
                    timer.ResetTime(m_TimerDuration);
                    break;
            }
        }


        // Helpers
        private async void LoadVideo()
        {
            var id = Data.ID;
            var offer = await ScutiNetClient.Instance.Offer.GetOfferByID(id);
            var panelModel = Mappers.GetOfferDetailsPresenterModel(offer);
            UIManager.OfferDetails.SetData(panelModel);
            UIManager.OfferDetails.SetIsVideo(true);
            UIManager.Open(UIManager.OfferDetails);
        }


    }
}
