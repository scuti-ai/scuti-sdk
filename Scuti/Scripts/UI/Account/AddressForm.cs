using UnityEngine;
using UnityEngine.UI;
using Scuti;
using Scuti.Net;
using System.Threading.Tasks;
using System;

namespace Scuti.UI
{
    public class AddressForm : Form<AddressForm.Model>
    {
        public class Model : Form.Model
        {
            public AddressData Address;
            public override string ToString()
            {
                return Address.ToString();
            }
        }

        [SerializeField] InputField line1Input;
        [SerializeField] InputField line2Input;
        [SerializeField] InputField cityInput;
        [SerializeField] Dropdown stateDropDown;
        [SerializeField] InputField zipInput;
        [SerializeField] Dropdown countryDropDown;
        [SerializeField] Button saveButton;
        [SerializeField] Button prevButton;
        [SerializeField] Text saveButtonLabel;

        public bool UseAsOnboarding = true;
        private bool _cachedAddress = false;

        public override void Bind()
        {
            Data.Address.Country = countryDropDown.options[countryDropDown.value].text;
            Data.Address.State = stateDropDown.options[stateDropDown.value].text;

            line1Input.onValueChanged.AddListener(value => Data.Address.Line1 = value);
            line2Input.onValueChanged.AddListener(value => Data.Address.Line2 = value);
            cityInput.onValueChanged.AddListener(value => Data.Address.City = value);
            stateDropDown.onValueChanged.AddListener(value => Data.Address.State = stateDropDown.options[value].text);
            zipInput.onValueChanged.AddListener(value => Data.Address.Zip = value);
            countryDropDown.onValueChanged.AddListener(value => Data.Address.Country = countryDropDown.options[value].text);
#pragma warning disable 
            saveButton.onClick.AddListener(async () => SaveShippingInfo());
#pragma warning restore 
        }

        public override void Open()
        {
            base.Open();



            if (UseAsOnboarding)
            {
                if (ScutiNetClient.Instance.FinishedOnBoarding)
                {
                    if (prevButton)
                        prevButton.gameObject.SetActive(false);
                    if (saveButtonLabel)
                        saveButtonLabel.text = "SAVE";
                }
                else
                {
                    if (prevButton)
                        prevButton.gameObject.SetActive(true);
                    if (saveButtonLabel)
                        saveButtonLabel.text = "NEXT STEP";
                }
            }


            TryToLoadData();
        }

        private async void TryToLoadData()
        {
            if (!await ScutiNetClient.Instance.InSupportedCountry())
            {
                UIManager.Alert.SetHeader("Unsupported Location").SetButtonText("Ok").SetBody("We currently do not support your location. International support coming soon!").Show(() => { });
            }

            if (!_cachedAddress)
                {
                    var shippingInfo = await ScutiAPI.GetShippingInfo();
                    if (shippingInfo != null)
                    {
                        Data.Address = new AddressData()
                        {
                            Line1 = shippingInfo.Address1,
                            Line2 = shippingInfo.Address2,
                            State = shippingInfo.State,
                            Zip = shippingInfo.ZipCode,
                            Country = shippingInfo.Country,
                            City = shippingInfo.City
                        };
                        _cachedAddress = true;
                        Refresh();
                    }
                }
        }

        public override void Refresh()
        {
            line1Input.text = Data.Address.Line1;
            line2Input.text = Data.Address.Line2;
            cityInput.text = Data.Address.City;
            stateDropDown.SetDropDown(Data.Address.State);
            zipInput.text = Data.Address.Zip;
            countryDropDown.SetDropDown(Data.Address.Country);
        }

        private async Task SaveShippingInfo()
        {
            if (!Evaluate())
                return;

            saveButton.interactable = false;
            bool submit = !UseAsOnboarding;
            if (UseAsOnboarding || !_cachedAddress)
            {
                try
                {
                    await ScutiAPI.EditShippingAddress(Data.Address.Line1, Data.Address.Line2, Data.Address.City, Data.Address.State, Data.Address.Country, Data.Address.Zip);
                    submit = true;
                    _cachedAddress = false;
                }
                catch (Exception ex)
                {
                    ScutiLogger.LogError(ex);
                    UIManager.Alert.SetHeader("Error").SetButtonText("Ok").SetBody("Setting shipping info failed").Show(() => { });
                }
            } 

            // Don't submit in the try catch as it could catch errors unrelated to the actual call
            if (submit)
            {
                Submit();
            }

            if(!UseAsOnboarding)
            {
                Close();
            }
            saveButton.interactable = true;
        }

        public override Model GetDefaultDataObject()
        {
            var model = new Model();
            model.Address = new AddressData()
            {
                Line2 = "",
                Country = countryDropDown.options[countryDropDown.value].text,
                State = stateDropDown.options[stateDropDown.value].text
            };
            return model;
        }
    }
}
