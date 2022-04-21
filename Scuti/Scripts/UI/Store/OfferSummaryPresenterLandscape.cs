using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Scuti.Net;

namespace Scuti.UI
{
    public class OfferSummaryPresenterLandscape : OfferSummaryPresenterBase 
    {


        public bool HasNext
        {
            get { return Next != null && !Next.ID.IsNullOrEmpty(); }
        }

        private bool loadingNextCompleted = false;



        // ================================================
        #region LICECYCLE
        // ================================================

        public override void SetData(Model data)
        {
            if (m_Data != null)
            {
                m_Data.OnStateChanged -= OnNextStateChanged;
            }
            base.SetData(data);
        }

        protected override void SwapToNext()
        {

            if (HasNext)
            {
                if (Next != null) Next.OnStateChanged -= OnNextStateChanged;
                Data = Next;
                Next = null;
                DisplayCurrentImage();
                ResetTimer();
                LoadCompleted();
            } 
        }

        protected override void LoadCompleted()
        {
            base.LoadCompleted();
            LoadNext();
        }

        protected override bool IsFirstLoad()
        {
            return !HasNext;
        }

        private async void LoadNext()
        { 
            loadingNextCompleted = false;
            if (!_isStatic)
            {
                Next = await m_NextRequest(this);
                if (Next != null)
                {
                    Next.IsTall = IsTall;
                    Next.isSingle = Single;
                    Next.OnStateChanged += OnNextStateChanged;
                    Next.LoadImage();
                }
            }
        }

        protected override void OnTimerCompleted()
        {
            base.OnTimerCompleted();
            CheckReady();
        }

        public void CheckReady()
        {
            if (!_destroyed)
            {
                if (loadingNextCompleted && timerCompleted)
                {
                    animator.SetTrigger("Rotate");
                }
            }
        }

        #endregion


        // ================================================
        #region PRESENTER
        // ================================================
        protected override void OnSetDataState(Model.State state)
        { 
            switch (state)
            {
                case Model.State.Failed:
                    Next = null;
                    break;
            }
            base.OnSetDataState(state);

        }
        protected override void OnNextStateChanged(Model.State state)
        {
            switch (state)
            {
                case Model.State.Loaded:
                    if (Next != null) Next.OnStateChanged -= OnNextStateChanged;
                    loadingNextCompleted = true;
                    CheckReady();
                    break;
                case Model.State.Failed:
                    LoadNext();
                    break;
            }
        }

       

        // Updates UI based on values on View.Data
        protected override void UpdateUI()
        {
            titleText.text = TextElipsis(Data.Title);
            displayPriceText.text = ScutiUtils.FormatPrice(Data.DisplayPrice);

            //  New and Hot Badges only in portrait
           
            //    newBadge.SetActive(false);
            //    hotBadge.SetActive(false);

            //var list = new List<KeyValuePair<GameObject, bool>> {
            //    new KeyValuePair<GameObject, bool>(hotPricePromo, Data.IsHotPrice),
            //    new KeyValuePair<GameObject, bool>(recommendedPromo, Data.IsRecommended),
            //    new KeyValuePair<GameObject, bool>(specialOfferPromo, Data.IsSpecialOffer),
            //    new KeyValuePair<GameObject, bool>(bestsellerPromo, Data.IsBestSeller),
            //    new KeyValuePair<GameObject, bool>(scutiPromo, Data.IsScuti)
            //};

            //list.ForEach(x => x.Key.SetActive(false));

            brandText.text = Data.Brand;
            //GlowImage.gameObject.SetActive(false);

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

        #endregion
    }
}
