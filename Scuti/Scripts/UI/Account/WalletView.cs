using System;
using System.Threading.Tasks;
using UnityEngine.UI;
using Scuti.Net;
using Scuti.GraphQL.Generated;
using UnityEngine;

namespace Scuti.UI
{
    public class WalletView : View
    {

        public InputField ScutiInput;
        public Text ConversionText;
        public UnityEngine.UI.Image ConversionImage;
        public Text ScutiTotal;
        public Text ExchangeInstructions;
        public Button ConvertButton;

        public GameObject ExchangeEnabledContent;
        public GameObject ExchangeDisabledContent;

        public decimal ExchangeRate = 0;
        private bool _currrencySet = false;

        protected override void Awake()
        {
            base.Awake();
            ScutiInput.onValueChanged.AddListener(OnInputChanged);
            ConversionText.text = "0";
            ScutiTotal.text = "0";
            ConversionImage.gameObject.SetActive(false);
        }

        public override void Open()
        {
            base.Open();

          

#pragma warning disable 4014
            Refresh();
#pragma warning restore 4014
        }

        private Wallet _wallet;
        private async Task Refresh()
        {
#if UNITY_IOS
    
                ExchangeDisabledContent.SetActive(true);
                ExchangeEnabledContent.SetActive(false);
#else 

            if (ScutiSDK.Instance.settings.ExchangeMethod == ScutiSettings.CurrencyExchangeMethod.None)
            {
                ExchangeDisabledContent.SetActive(true);
                ExchangeEnabledContent.SetActive(false);
            }
            else
            {
                ExchangeDisabledContent.SetActive(false);
                ExchangeEnabledContent.SetActive(true);
                if (ScutiNetClient.Instance.gameInfo == null) await ScutiNetClient.Instance.GetAccountInfo();

                if ((ScutiNetClient.Instance.gameInfo != null))
                {
                    ExchangeRate = ScutiNetClient.Instance.gameInfo.Currency.ScutiExchangeRate.Value;
                    ExchangeInstructions.text = "Exchange SCUTIS for " + ScutiNetClient.Instance.gameInfo.Currency.Name;
                }
                else
                {
                    ExchangeRate = 0;
                }


                if (!_currrencySet) DownloadCurrencyIcon();

            }
#endif
            _wallet = await ScutiAPI.GetWallet(true);
            ScutiTotal.text = ScutiUtils.GetTotalWallet(_wallet).ToString();
        }

        private void DownloadCurrencyIcon()
        {
            var sprite = ScutiNetClient.Instance.CurrencyIconToSprite();
            if(sprite!=null)
            {
                ConversionImage.sprite = sprite;
                _currrencySet = true;
            }
            ConversionImage.gameObject.SetActive(sprite!=null);
        }
         

        private void OnInputChanged(string value)
        {
            var scutis = int.Parse(value);
            ConversionText.text = Math.Floor(scutis * ExchangeRate).ToString();
        }


        public async void Convert()
        {
            if (string.IsNullOrEmpty(ScutiInput.text))
            {
                UIManager.Alert.SetHeader("Invalid Field").SetBody($"Please enter a number of SCUTIS to convert.").SetButtonText("OK").Show(() => { });
                return;
            }

            ConvertButton.enabled = false;
            try
            {
                int amount = int.Parse(ScutiInput.text);
                var total = ScutiUtils.GetTotalWallet(_wallet);
                if (_wallet != null && amount > total)
                {
                    UIManager.Alert.SetHeader("Not enough Scutis").SetBody($"You can only convert up to your balance of {total}. ").SetButtonText("OK").Show(() => { });

                }
                else
                {
                    if ((Math.Floor(amount * ExchangeRate)) > 0)
                    {
                        var reward = await ScutiAPI.ExchangeRewards(amount);
                        if (ScutiSDK.Instance.settings.ExchangeMethod == ScutiSettings.CurrencyExchangeMethod.GameClient)
                        {
                            ScutiSDK.Instance.GrantCurrency(reward);
                        }
                        var newWallet = await ScutiAPI.GetWallet(true);
                        ScutiTotal.text = ScutiUtils.GetTotalWallet(newWallet).ToString();

                        var currencyName = ScutiNetClient.Instance.gameInfo.Currency.Name;
                        UIManager.Alert.SetHeader("Congratulations!").SetBody($"You successfully exchanged {ScutiInput.text} for {reward} {currencyName}.").SetButtonText("OK").Show(() => { });
                    }
                    else
                    {
                        UIManager.Alert.SetHeader("Not enough Scutis").SetBody($"You need to convert more Scutis in order to at least get 1 {ScutiNetClient.Instance.gameInfo.Currency.Name} ").SetButtonText("OK").Show(() => { });
                    }
                }
            }
            catch (Exception ex)
            {
                UIManager.Alert.SetHeader("Failed to exchange").SetBody($"Failed to exchange {ScutiInput.text} scutis {ex.Message}").SetButtonText("OK").Show(() => { });
                ScutiLogger.LogException(ex);
            }

            ConvertButton.enabled = true;
        }
    }
}