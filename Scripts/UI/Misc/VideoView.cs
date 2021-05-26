using System;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

using Scuti;

namespace Scuti.UI
{
    [RequireComponent(typeof(VideoPlayer))]
    [RequireComponent(typeof(AudioSource))]
    public class VideoView : View
    {
        public enum VideoResult
        {
            Finished,
            Skipped,
            Error
        }

        public event Action<VideoResult> OnResult;
        [SerializeField] RawImage m_VideoSurface;
        [SerializeField] Image m_Bar;

        [SerializeField] AspectRatioFitter m_Fitter;
        VideoPlayer m_Player;
        AudioSource m_Audio;
        int m_scutis;
        string url;

        protected override void Awake()
        {
            base.Awake();
            m_Audio = GetComponent<AudioSource>();
            m_Audio.spatialBlend = 0;
            m_Audio.playOnAwake = false;

            m_Player = GetComponent<VideoPlayer>();
            m_Player.playOnAwake = false;
            m_Player.isLooping = false;
            m_Player.source = UnityEngine.Video.VideoSource.Url;
            m_Player.audioOutputMode = VideoAudioOutputMode.AudioSource;
            m_Player.EnableAudioTrack(0, true);
            m_Player.SetTargetAudioSource(0, m_Audio);
            m_Player.controlledAudioTrackCount = 1;



            m_Player.prepareCompleted += player => {
                // This event is being invoked even when the URL is invalid.
                // But Player.url will not be updated to the invalid value.
                // So we compare and only play if the url update happened.
                if (player.url == url)
                {
                    player.Play();
                    m_Audio.Play();
                }
            };
            m_Player.started += player => {
                m_VideoSurface.enabled = true;
                m_VideoSurface.texture = player.texture;
                m_Fitter.aspectRatio = (float)player.texture.width / (float)player.texture.height;
            };

            m_Player.errorReceived += (source, message) => { 
                OnResult?.Invoke(VideoResult.Error);
                UIManager.Alert.SetHeader("Video Playback Error").SetBody(message).SetButtonText("OK").Show(() => { });
                Close();
            };
            m_Player.loopPointReached += player => {
                player.Stop(); 
                OnResult?.Invoke(VideoResult.Finished); 
                UIManager.Rewards.SetData(new RewardPresenter.Model() { reward = m_scutis, subtitle = "Collect your Rewards", title = "CONGRATULATIONS!" });
                UIManager.Open(UIManager.Rewards);
                Close();
            };
        }

        void Update()
        {
            UpdateBar();
        }

        void UpdateBar()
        {
            if (m_Player.texture == null) return;

            long current = m_Player.frame;
            ulong total = m_Player.frameCount;

            if (total != 0)
                m_Bar.fillAmount = (float)decimal.Divide(current, total);
        }

        /// <summary>
        /// Shows the Video Player UI
        /// </summary>
        public override void Open()
        {
            base.Open();
            m_VideoSurface.enabled = m_Player.isPlaying;
            m_Bar.fillAmount = 0;
        }

        /// <summary>
        /// Hides the Video Player UI
        /// </summary>
        public override void Close()
        {
            base.Close();
            m_VideoSurface.enabled = false;
            m_Bar.fillAmount = 0;
        }

        /// <summary>
        /// Initiates the playback of a video at the given URL
        /// </summary>
        /// <param name="url">The URL/path at which the video can be found</param>
        public void Play(string url, int scutis)
        {
            m_scutis = scutis;
            // VideoPlayer.prepareCompleted is being called even with an invalid URL
            // so we store the URL so that we can compare it later.
            if (!url.ToUpper().StartsWith("HTTP")) url = Application.streamingAssetsPath + "/" + url; 
            this.url = url;
            m_Player.url = url;
            m_Player.Prepare();
        }

        /// <summary>
        /// Skips the video playback and closes the videoplayer interface.
        /// </summary>
        public void Skip()
        {
            OnResult?.Invoke(VideoResult.Skipped);
            m_Player.Stop();
            Close();
        }
    }
}