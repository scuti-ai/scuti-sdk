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

            UIManager.Open(UIManager.Offers);
            UIManager.HideLoading(false);
            UIManager.TopBar.Open();
            UIManager.Splash.ShowSplash(() =>
            {
                OnSplashCompleted();
            });

        }

        private async void OnSplashCompleted()
        { 
            // disabled for now, requested by Nicholas -mg
            //if (IsNewUser())
            //{
            //    bool shown = await ShowOffer();
            //    if (!shown)
            //    {
            //        UIManager.HideLoading(false);
            //        UIManager.Open(UIManager.Welcome);
            //    }
            //}
            //else
            //{
                var diff = await ScutiNetClient.TryToActivateRewards();
                if (diff > 0)
                {
                    UIManager.HideLoading(false);
                    UIManager.Open(UIManager.Rewards);
                    UIManager.Rewards.SetData(new RewardPresenter.Model() { reward = (int)diff, subtitle = "Collect your rewards!", title = "CONGRATULATIONS!" });
                } else
                {
                    UIManager.RefreshLoading();
                }
                await ShowOffer();
            //}
            isStarted = true;
        }

        private async Task<bool> ShowOffer()
        {
            //ScutiLogger.Log("Checking deep link: " + deepLinkOffer);
            if (!string.IsNullOrEmpty(deepLinkOffer))
            {
                var offer = await ScutiNetClient.Instance.Offer.GetOfferByID(deepLinkOffer);
                if (offer != null)
                {
                    var panelModel = Mappers.GetOfferDetailsPresenterModel(offer);
                    if (panelModel != null)
                    {
                        ScutiLogger.Log("Show!");
                        UIManager.OfferDetails.SetData(panelModel);
                        UIManager.OfferDetails.SetIsVideo(false);
                        UIManager.Open(UIManager.OfferDetails);
                    } else
                    {
                        ScutiLogger.LogError("No Pannel found for: " + deepLinkOffer +" offer: "+offer.Id);

                    }
                }else
                {
                    ScutiLogger.LogError("No offer found for: " + deepLinkOffer);
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
