using System;

using UnityEngine;
using UnityEngine.UI;


using Scuti.Net;
using System.Linq;

namespace Scuti.UI
{
    public class OfferDetailsPresenter : Presenter<OfferDetailsPresenter.Model>
    {
        [Serializable]
        public class Model : Presenter.Model
        {
            public string ShopName;
            public OfferInfoPresenter.Model Info = new OfferInfoPresenter.Model();
            public OfferRewardPresenter.Model Reward = new OfferRewardPresenter.Model();
            public OfferFeedbackPresenter.Model Feedback = new OfferFeedbackPresenter.Model();
            public OfferShowcasePresenter.Model Showcase = new OfferShowcasePresenter.Model();
            public OfferCustomizationPresenter.Model Customization = new OfferCustomizationPresenter.Model();
        }

        [Header("Sub Widgets")]
        [SerializeField] OfferInfoPresenter infoWidget;
        [SerializeField] OfferRewardPresenter rewardWidget;
        [SerializeField] OfferFeedbackPresenter feedbackWidget;
        [SerializeField] OfferShowcasePresenter showcaseWidget;
        [SerializeField] OfferCustomizationPresenter customizationWidget;
        [SerializeField] GameObject LargeDescription;
        [SerializeField] GameObject Reviews;
        [SerializeField] OfferRecommendedPresenter RecommendedWidget;

        [Header("Interactions")]
        [SerializeField] Text addToCartLabel;
        [SerializeField] Image addToCartIcon;
        [SerializeField] Button shareButton;

        public ScrollRect ScrollContent;

        private bool _isVideo;

        float browseTime;

        protected override void Awake()
        {
            LargeDescription.SetActive(false);
            Reviews.SetActive(false);

            customizationWidget.VariantChanged += OnVariantChanged;
        }


        protected override void OnSetState()
        {
            ScrollContent.verticalNormalizedPosition = 1;
            infoWidget.Data = Data.Info;
            rewardWidget.Data = Data.Reward;
            feedbackWidget.Data = Data.Feedback;
            showcaseWidget.Data = Data.Showcase;
            customizationWidget.Data = Data.Customization;
            RecommendedWidget.SearchForRecommendations(Data.ShopName, Data.Info.ID);
        }

        public override void Open()
        {
            base.Open();
            browseTime = Time.time;
        }

        public override void Close()
        {
            base.Close();

       
                if (Data != null && Data.Info != null)
                {
                    var diff = Time.time - browseTime;

                    if (diff > 1f)
                        ScutiAPI.EngagementWithProductMetric((int)Mathf.Round(diff), 1, Data.Info.ID);
                }
        }

        internal void SetIsVideo(bool isVideo)
        {
            _isVideo = isVideo;
            if (isVideo)
            {
                addToCartLabel.text = "WATCH VIDEO";
                addToCartIcon.gameObject.SetActive(false);
            }
            else
            {
                addToCartLabel.text = "ADD TO CART";
                addToCartIcon.gameObject.SetActive(true);
            }
            customizationWidget.SetIsVideo(isVideo);
            feedbackWidget.gameObject.SetActive(!isVideo);
            customizationWidget.gameObject.SetActive(!isVideo);
        }

        public void AddToCart()
        {
            if (!ScutiNetClient.Instance.IsAuthenticated)
            {
                UIManager.Open(UIManager.PromoAccount);
                UIManager.PromoAccount.SetMessage("Create Account to Add to Your Cart.");
                return;
            }

            if (_isVideo)
            {
                PlayVideo();
            }
            else
            {
                var model = Mappers.CartEntryWidgetFrom(Data);
                model.quantity = Data.Customization.Quantity;
                //GetVariantId(model);
                model.campaignId = Data.Info.ID;
                UIManager.Cart.GetData().AddItem(model);
                UIManager.Open(UIManager.Cart);
            }
        }

        //private void GetVariantId(CartEntryPresenter.Model model)
        //{
        //    GraphQL.Generated.ProductVariant variant = Data.Customization.Variants[0];
        //    model.variant = variant.Options == null || variant.Options.Count == 0 ? variant.Id : variant.Options.First().Id;
        //}

        public void PlayVideo()
        {
            UIManager.Open(UIManager.VideoPlayer);
            UIManager.VideoPlayer.Play(Data.Showcase.VideoURL, Data.Reward.scutiReward);
        }

        public void Share()
        {
            ShareDialog.Share($"https://scuti.store/product?offerid={Data.Info.ID}");
        }

        // Handlers
        private void OnVariantChanged()
        {
            var productVariant = customizationWidget.Data.GetVariant(); 
            infoWidget.SetVariant(productVariant);
            rewardWidget.SetVariant(productVariant);
            showcaseWidget.SetVariant(productVariant);

        }
    }
}