using UnityEngine;

using Scuti.UI;
using Scuti.Net;
using System.Threading.Tasks;

namespace Scuti
{
    public class SDKManager : MonoBehaviour
    {
        public bool showForNewUser;


        public static SDKManager Instance { get; set; }
        private bool isStarted;

        void Start()
        {
            UIManager.Splash.ShowSplash(() =>
            {
                OnSplashCompleted();
            });

        }

        private async void OnSplashCompleted()
        { 
            UIManager.Open(UIManager.Offers);
            UIManager.TopBar.Open();
            Debug.Log("Splash");
            if (IsNewUser())
            {
                bool shown = await ShowOffer();
                Debug.Log("Shown " + shown);
                if (!shown)
                    UIManager.Open(UIManager.Welcome);
            }
            else
            {
                UIManager.Open(UIManager.Offers);

                var diff = await ScutiNetClient.TryToActivateRewards();

                if (diff > 0)
                {
                    UIManager.Rewards.SetData(new RewardPresenter.Model() { reward = (int)diff, subtitle = "Collect your rewards!", title = "CONGRATULATIONS!" });
                    UIManager.Open(UIManager.Rewards);
                }
                await ShowOffer();
            }
            isStarted = true;
        }

        private async Task<bool> ShowOffer()
        {
            if (!string.IsNullOrEmpty(deepLinkOffer))
            {
                var offer = await ScutiNetClient.Instance.Offer.GetOfferByID(deepLinkOffer);
                if (offer != null)
                {
                    var panelModel = Mappers.GetOfferDetailsPresenterModel(offer);
                    if (panelModel != null)
                    {
                        UIManager.OfferDetails.SetData(panelModel);
                        UIManager.OfferDetails.SetIsVideo(false);
                        UIManager.Open(UIManager.OfferDetails);
                    }
                }
                return true;
            }
            else
                return false;
        }

        private string deepLinkOffer = "";
        public void SetDeepLink(string offerId)
        {
#pragma warning disable 4014
            deepLinkOffer = offerId;
            if (isStarted)
                ShowOffer();
#pragma warning restore 4014
        }


        private bool IsNewUser()
        {

#if UNITY_EDITOR
            return showForNewUser;
#else

           if(PlayerPrefs.HasKey(ScutiConstants.KEY_WELCOME))
            {
                return (0 == PlayerPrefs.GetInt(ScutiConstants.KEY_WELCOME, 0));
            }
            return true;
#endif
        }
    }
}
