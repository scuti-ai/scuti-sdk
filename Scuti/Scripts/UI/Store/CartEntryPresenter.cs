using System;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using Scuti;
using Scuti.Net;

namespace Scuti.UI {
    public class CartEntryPresenter : Presenter<CartEntryPresenter.Model> {
        public event Action<CartEntryPresenter> OnDelete;
        public event Action<int> OnQuantityChanged;

        [Serializable]
        public class Model : Presenter.Model {
            public string id;

            public Sprite icon;
            public string imageUrlFallback;
            public string title;
            public Guid? variant;
            public string campaignId;
            public decimal? price;
            public int scutiCoinReward;
            public int quantity;

            public override void Dispose()
            {
                base.Dispose();
                if(icon!=null && icon.texture)
                    Destroy(icon.texture);
            }
        }

        [Header("Interactions")]
        [SerializeField] Button deleteButton;

        [Header("Details")]
        [SerializeField] Image itemImage;
        [SerializeField] Text titleText;
        [SerializeField] Text displayPriceText;
        [SerializeField] Text scutiRewardText;

        [Header("Preferences")]
        [SerializeField] Text sizeText;
        [SerializeField] Image colorImage;
        [SerializeField] IntegerStepperWidget quantityStepper;

        public Model state;

        protected override void Awake()
        {
            base.Awake();
            HandleInteractions();

            sizeText.gameObject.SetActive(false); 
            colorImage.gameObject.SetActive(false); 


        }

        void HandleInteractions() {
            deleteButton.onClick.AddListener(() => OnDelete?.Invoke(this));
        }


        public override void OnEvent(string notification, object payload)
        {
            if (_destroyed) return;
            switch (notification)
            {
                case "item-quantity-changed":
                    RefreshVisuals();
                    break;
            }
        }

        protected override void OnSetState() {
            state = Data;

            if (Data.icon != null)
                itemImage.sprite = Data.icon;
            else
            {
                if (!string.IsNullOrEmpty(Data.imageUrlFallback))
                {
                    ImageDownloader.New().Download(Data.imageUrlFallback,
                        result =>
                        {
                            if(itemImage!=null)
                                itemImage.sprite = result.ToSprite();
                        },
                        error =>
                        {
                            ScutiLogger.LogError("Failed to load: " + Data.imageUrlFallback + " for cart.");
                        }
                    );
                }
                else
                {
                    ScutiLogger.LogError("Sprite null " + this + "  and no fallback image.");
                }
            }

            RefreshVisuals();
            quantityStepper.OnValueChanged += value => {


                if (value == 0)
                    OnDelete?.Invoke(this);
                else
                {
                    if (Data.quantity != value)
                    {
                        Data.quantity = value;
                        RefreshVisuals();
                        OnQuantityChanged?.Invoke(Data.quantity);
                    }
                }
            };

            
        }

        public void Click()
        {
            OnClick();
        }

        private async void OnClick()
        {
            var offer = await ScutiNetClient.Instance.Offer.GetOfferByID(state.campaignId);
            var panelModel = Mappers.GetOfferDetailsPresenterModel(offer);
            UIManager.OfferDetails.SetData(panelModel);
            UIManager.Open(UIManager.OfferDetails);
        }

    private void RefreshVisuals()
        {
            titleText.text = Data.title;
            displayPriceText.text = "$ " + (Data.price.Value* Data.quantity).ToString("F2");
            scutiRewardText.text = "+ " + (Data.scutiCoinReward* Data.quantity);

            //sizeText.text = Data.displaySize;
            //colorImage.color = Data.color;

            quantityStepper.Value = Data.quantity;
        }

        protected override void OnDestroy()
        {
            // don't call base destroy, it wipes the data and the data exist longer than we do
            if (m_Data != null) m_Data.OnEvent -= OnEvent;
            _destroyed = true;
        }
    }
}
