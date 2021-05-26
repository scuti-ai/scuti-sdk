using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Scuti;

namespace Scuti.UI {
    public class VideoPresenter : Presenter<VideoPresenter.Model> {
        [Serializable]
        public class Model : Presenter.Model {
            public string Title;
            public string Description;
            public string ThumbnailGraphicURL;
            public string VideoURL;
            public string RewardGraphicURL;
            public string RewardText;
            public List<OfferSummaryPresenter.Model> Merchandize = new List<OfferSummaryPresenter.Model>();
        }

        public event Action OnPlayVideo;
        public event Action<string> OnMerchandizeWidgetClick;

        [SerializeField] VideoView m_Player;
        [SerializeField] AlertView m_Dialog;

        [Header("Fields")]
        [SerializeField] Text title;
        [SerializeField] Text description;
        [SerializeField] Image thumbnail;
        [SerializeField] Image rewardImage;
        [SerializeField] Text rewardText;

        [Header("Instantiation")]
        [SerializeField] OfferSummaryPresenter merchandizeWidgetPrefab;
        [SerializeField] CarouselLayout merchandizeCarousel;

        public void PlayVideo() {
            OnPlayVideo?.Invoke();
            m_Player.Open();
            m_Player.Play((Data as Model).VideoURL, -1);

            m_Player.OnResult += result => {
                if (result == VideoView.VideoResult.Finished) {
                    m_Dialog.SetHeader("Video Ad")
                        .SetBody("Video finished, you get the reward!")
                        .SetButtonText("Yay!")
                        .Show(() => { });
                }
                else if (result == VideoView.VideoResult.Skipped) {
                    m_Dialog.SetHeader("Video ad")
                        .SetBody("You must finish the video to get the reward.")
                        .SetButtonText("OK")
                        .Show(() => { });
                }
            };
        }

        protected override void OnSetState() {
            title.text = Data.Title;
            description.text = Data.Description;
            rewardText.text = Data.RewardText;

            merchandizeCarousel.Clear();
            foreach (var entry in Data.Merchandize) {
                var instance = Instantiate(merchandizeWidgetPrefab);
                instance.Data = entry;
                instance.OnClick += () => OnMerchandizeWidgetClick?.Invoke((string)entry.ID);

                merchandizeCarousel.Add(instance.gameObject);
            }

            DownloadImages();
        }

        async void DownloadImages() {
            var downloader = ImageDownloader.New(false);

            try {
                var thumbnailTex = await downloader.Download(Data.ThumbnailGraphicURL);
                thumbnail.ReplaceSprite(thumbnailTex.ToSprite());

                var rewardTex = await downloader.Download(Data.RewardGraphicURL);
                rewardImage.ReplaceSprite(rewardTex.ToSprite());

                downloader.Destroy();
            }
            catch { }
        }
    }
}
