using System;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using Scuti;
using Scuti.GraphQL.Generated;

namespace Scuti.UI {
    public class OrderItemPresenter : Presenter<OrderItemPresenter.Model> { 

        [Serializable]
        public class Model : Presenter.Model {
            public string orderId;
            public decimal price;
            public string orderStatus;
            public int quantity;
            public string purchaseDate;
            internal OrderProductSnapshot product;

            private Texture2D m_texture;
            public void ClearTexture()
            {
                if (m_texture) Destroy(m_texture);
                m_texture = null;
            }

            public void AddTexture(Texture2D tex)
            {
                ClearTexture();
                m_texture = tex;
            }

            public override void Dispose()
            {
                base.Dispose();
                ClearTexture();
            }
        }
         
        public Text[] PriceLabels;
        public Text ItemLabel;
        public Text StatusLabel;
        public Text QuantityLabel;
        public Text DateLabel;
        public Text OrderIdLabel;
        public UnityEngine.UI.Image ImageDisplay;


        protected override void OnSetState()
        {
            gameObject.SetActive(true);
            if(Data!=null)
            {
                foreach(var price in PriceLabels)
                {
                    price.text = $"${Data.price}";
                }

                ItemLabel.text = GetProductName(Data.product);
                if (Data.product.Option!=null && !string.IsNullOrEmpty(Data.product.Option.Image))
                {
                    PopulateImageFromVariant(Data.product.Option);
                } else if(Data.product.Variant!=null && !string.IsNullOrEmpty(Data.product.Variant.Image))
                {
                    PopulateImageFromVariant(Data.product.Variant);
                } else
                {
                    ScutiLogger.Log("No image for " + Data.product.Name);
                }

                StatusLabel.text = Data.orderStatus;
                QuantityLabel.text = Data.quantity.ToString();
                DateLabel.text = Data.purchaseDate;
                OrderIdLabel.text = Data.orderId;

            } else
            {
                gameObject.SetActive(false);
            }
        }

        private string GetProductName(OrderProductSnapshot product)
        {

            var productName = product.Name;

            Debug.LogError("TODO: Add variants back in here. -mg");
            //if (Data.product.Variant != null)
            //{
            //    productName += ": " + Data.product.Variant.Op;
            //}
            //if (Data.product.Option != null)
            //{
            //    productName += ": " + Data.product.Option.Name;
            //}
            return productName;

        }

        async void PopulateImageFromVariant(OrderProductVariantSnapshot variant)
        {
            if (!string.IsNullOrEmpty(variant.Image))
            {
                var downloader = ImageDownloader.New(true);
                var tex = await downloader.Download(variant.Image);
                Data.AddTexture(tex);
                ImageDisplay.sprite = tex.ToSprite();
            } else
            {
                ScutiLogger.LogError("variant image is null");
            }
        }
    }
}
