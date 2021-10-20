using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scuti.UI {
    public class OfferFeedbackPresenter : Presenter<OfferFeedbackPresenter.Model> {
        [Serializable]
        public class Model : Presenter.Model {
            public int ReviewCount;
            public decimal Rating;
        }

        [SerializeField] TextMeshProUGUI reviewsLabel;
        [SerializeField] TextMeshProUGUI feedbackCountText;
        [SerializeField] TextMeshProUGUI ratingValueText;
        [SerializeField] RatingStarsWidget starWidget;

        protected override void OnSetState() {
            // Show feedback count if it's 1 or above
            bool hasReviewCount = Data.ReviewCount > 0;
            reviewsLabel.gameObject.SetActive(hasReviewCount);
            feedbackCountText.gameObject.SetActive(hasReviewCount);
            if (hasReviewCount)
                feedbackCountText.text = Data.ReviewCount.ToString();

            // Show rating widget and value if rating string exists
            ratingValueText.gameObject.SetActive(hasReviewCount);
            starWidget.gameObject.SetActive(hasReviewCount);
            if(hasReviewCount)
            {
                ratingValueText.text = Data.Rating.ToString("0.0");
                starWidget.Value = (float)Data.Rating / starWidget.Levels;
            }
        }
    }
}
