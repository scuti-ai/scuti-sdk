using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Scuti;
using Scuti.GraphQL.Generated;
using TMPro;

namespace Scuti.UI {
    public class OfferInfoPresenter : Presenter<OfferInfoPresenter.Model> {
        [Serializable]
        public class Model : Presenter.Model {
            public string ID;

            public string Title;
            public string Subtitle;
            public string Description;
            public string Brand;
            //public string DisplayPrice;
            //public float Price;

            // TODO these terms are old and new ones have been introduced
            public bool IsRecommended;
            public bool IsHotPrice;
            public bool IsBestSeller;
            public bool IsSpecialOffer;
            public bool IsScuti;
        }

        [Header("Fields")]
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] TextMeshProUGUI displayPrice;
        [SerializeField] TextMeshProUGUI subtitle;
        [SerializeField] TextMeshProUGUI description;
        [SerializeField] TextMeshProUGUI brand;

        [Header("Flags")]
        [Header("Promos")]
        [SerializeField] GameObject hotPricePromo;
        [SerializeField] GameObject recommendedPromo;
        [SerializeField] GameObject specialOfferPromo;
        [SerializeField] GameObject bestsellerPromo;
        [SerializeField] GameObject scutiPromo;

        protected override void OnSetState() {
            title.text = Data.Title;
            subtitle.text = Data.Subtitle.ToUpper();
            subtitle.gameObject.SetActive(!string.IsNullOrWhiteSpace(subtitle.text));
            //Debug.Log("----**----");
            //Debug.Log(Data.Description);
            //Debug.Log(Data.Description.Trim('\r', '\n', ' '));
            description.text = Data.Description.Trim('\r', '\n', ' ');
            brand.text = Data.Brand;
            // Show ONLY THE FIRST promo that is applicable
            var list = new List<KeyValuePair<GameObject, bool>> {
                new KeyValuePair<GameObject, bool>(hotPricePromo, Data.IsHotPrice),
                new KeyValuePair<GameObject, bool>(recommendedPromo, Data.IsRecommended),
                new KeyValuePair<GameObject, bool>(specialOfferPromo, Data.IsSpecialOffer),
                new KeyValuePair<GameObject, bool>(bestsellerPromo, Data.IsBestSeller),
                new KeyValuePair<GameObject, bool>(scutiPromo, Data.IsScuti)
            };

            list.ForEach(x => x.Key.SetActive(false));
            foreach (var pair in list) {
                if (pair.Value) {
                    pair.Key.SetActive(true);
                    break;
                }
            }
        }

        internal void SetVariant(ProductVariant productVariant)
        {
            if (productVariant != null && productVariant.Price != null)
            {
                displayPrice.gameObject.SetActive(productVariant.Price.Amount > 0);
                displayPrice.text = $"${ScutiUtils.FormatPrice(productVariant.Price.Amount.Value.ToString("F2"))}";
            } else
            {
                ScutiLogger.LogError("Missing product variant or price: " + productVariant);
                displayPrice.gameObject.SetActive(false);
            }
        }
    }
}
