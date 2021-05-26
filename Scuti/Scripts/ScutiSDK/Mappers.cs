﻿using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Scuti.GraphQL.Generated;
using Scuti.UI;
using System;

namespace Scuti
{
    /// <summary>
    /// This class provides data mapping features. Usually converting GQL response objects
    /// to UI ready objects
    /// </summary>
    public static class Mappers
    {
        // ================================================
        #region OFFERS
        // ================================================
        public static OffersPresenter.Model GetOffersPresenterModel(List<Offer> offers)
        {
            var result = new OffersPresenter.Model();
            var allowAd = UIManager.IsLargeDisplay() ? 5 : 2;
            if (offers != null)
            {
                offers.ToList().ForEach(offer =>
                {
                    var element = GetOfferSummaryPresenterModel(offer, allowAd > 0);
                    if (element.DisplayAd) allowAd--;
                    result.Items.Add(element);
                });
            }
            result.Items.Shuffle();
            return result;
        }


        internal static OfferVideoPresenter.Model GetVideoPresenterModel(Offer offer)
        {
            OfferVideoPresenter.Model data = new OfferVideoPresenter.Model();
            data.ID = offer.Id.ToString();

            List<string> images = new List<string>();
            if (offer != null && offer.Media?.Images != null) images = offer.Media.Images.ToList();

            if(images.Count>0)
                data.ImageURL = images.Last(); // bit of a hack for now until we add a propert dto -mg

            return data;
        }

        #endregion

        // ================================================
        #region OFFER SUMMARY
        // ================================================
        public static OfferSummaryPresenter.Model GetOfferSummaryPresenterModel(Offer offer, bool allowAd)
        {
            List<string> images = new List<string>();
            if (offer != null && offer.Media?.Images != null) images = offer.Media.Images.ToList();

            bool displayAd = allowAd;
            if (displayAd || !string.IsNullOrEmpty(offer.Media.VideoUrl) && offer.Media.Banner!=null)
            {
                displayAd = !string.IsNullOrEmpty(offer.Media.Banner.TallUrl) || !string.IsNullOrEmpty(offer.Media.Banner.SmallUrl);
            }
            if (offer.Product == null)
            {
                offer.Product = new OfferProduct();
            }
            if (offer.Product.Price == null || offer.Product.Price.Amount == null)
            {
                offer.Product.Price = new Price();
                offer.Product.Price.Amount = 0;
            }

            int scutis = 0;
            if (offer.Reward != null) scutis = (int)offer.Reward.Scutis.Value;
            decimal rating = 0;
            if (offer.Review != null) rating = offer.Review.Score.Value;
            return new OfferSummaryPresenter.Model
            {
                ID = offer.Id.ToString(),
                ImageURL = images != null && images.Count > 0 ? images[0] : string.Empty,
                TallURL = offer.Media.Banner.TallUrl,
                SmallURL = offer.Media.Banner.SmallUrl,
                VideoURL = offer.Media.VideoUrl,
                Scutis = scutis,
                DisplayAd = displayAd,
                Title = offer.Product.Name,
                Description = offer.Product.Description,
                DisplayPrice = $"${offer.Product.Price.Amount.Value.ToString("F2")}",
                IsNew = offer.Promotions.IsNew.Value,
                IsHot = offer.Promotions.IsHotItem.Value,
                IsHotPrice = offer.Promotions.IsHotPrice.Value,
                IsRecommended = offer.Promotions.IsRecommended.Value,
                IsSpecialOffer = offer.Promotions.IsSpecialOffer.Value,
                IsBestSeller = offer.Promotions.IsBestSeller.Value,
                IsMoreExposure = offer.Promotions.IsMoreExposure.Value,
                IsFeatured = offer.Promotions.IsFeatured.Value,
                IsScuti = offer.IsScuti(),
                Rating = (float)rating

            };
        }

        #endregion

        // ================================================
        #region OFFER DETAILS
        // ================================================
        public static OfferDetailsPresenter.Model GetOfferDetailsPresenterModel(Offer offer)
        {

            var subtitle = string.Empty;
            if (offer.Promotions != null && !string.IsNullOrEmpty(offer.Promotions.SpecialOfferText))
                subtitle = offer.Promotions.SpecialOfferText;

            if (offer.Product.Images == null) offer.Product.Images = new List<string>();

            //if (offer.Product.Price.Amount == null) offer.Product.Price.Amount = 0;
            int scuti = 0;
            if (offer.Reward != null && offer.Reward.Scutis != null) scuti = (int)offer.Reward.Scutis.Value;

            decimal rating = 0;
            int count = 0;
            if(offer.Review!=null)
            {
                rating = offer.Review.Score.Value;
                count = offer.Review.Count.Value;
            }
            var sm = new OfferDetailsPresenter.Model
            {
                ShopName = offer.Shop.Name,
                Info = new OfferInfoPresenter.Model
                {
                    ID = offer.Id.ToString(),
                    Title = offer.Product.Name,
                    Subtitle = subtitle,
                    Description = offer.Product.Description,
                    //DisplayPrice = $"${offer.Product.Price.Amount.Value.ToString("F2")}",
                    //Price = (float)offer.Product.Price.Amount.Value,
                    IsHotPrice = offer.Promotions.IsHotPrice.Value,
                    IsRecommended = offer.Promotions.IsRecommended.Value,
                    IsSpecialOffer = offer.Promotions.IsSpecialOffer.Value,
                    IsBestSeller = offer.Promotions.IsBestSeller.Value,
                    IsScuti = offer.IsScuti(),
                },
                
                Feedback = new OfferFeedbackPresenter.Model
                {
                    ReviewCount = count,
                    Rating = rating
                },

                Reward = new OfferRewardPresenter.Model
                {
                    scutiReward = scuti
                },

                Showcase = new OfferShowcasePresenter.Model
                {
                    URLs = offer.Product.Images.ToList().Where(x => !x.IsNullOrEmpty()).ToList(),
                    VideoURL = offer.Media.VideoUrl
                },

                Customization = new OfferCustomizationPresenter.Model
                {
                    Quantity = 1,
                    Variants = offer.Product.Variants.ToArray()
                }
            };
            return sm;
        }
        #endregion

        // ================================================
        #region CART
        // ================================================
        // TODO: This is a stub
        public static CartPresenter.Model GetEmptyCart()
        {
            var sm = new CartPresenter.Model
            {
                // Stub data
                ShippingAddress = new AddressData() { Line2 = "" },
                BillingAddress = new AddressData() { Line2 = "" },
                SalesTax = 0,
                ShippingFee = 0,

                // No items in an empty cart
                Items = new List<CartEntryPresenter.Model>()
            };
            return sm;
        }

        public static CartEntryPresenter.Model CartEntryWidgetFrom(OfferDetailsPresenter.Model data)
        {
            Texture2D copy = null;
            var texture = data.Showcase.GetTexture(0);

            if (texture != null)
            {
                copy = new Texture2D(texture.width, texture.height);
                copy.SetPixels(texture.GetPixels());
                copy.Apply();
            }

            ProductVariant variant = data.Customization.GetVariant();

            string firstImage = null;
            if (data.Showcase.URLs.Count > 0) firstImage = data.Showcase.URLs[0];
            return new CartEntryPresenter.Model {
                id = variant.Id.ToString(),
                variant = variant.Id,
                icon = copy ? copy.ToSprite() : null,
                title = data.Info.Title,
                imageUrlFallback = firstImage,
                price = variant.Price.Amount,
                scutiCoinReward = data.Reward.scutiReward,
                quantity = 1
            };
        }

        #endregion
    }
}