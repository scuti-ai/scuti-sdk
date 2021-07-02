using System;
using UnityEngine;
using UnityEngine.UI;
using Scuti;
using System.Threading.Tasks;
using Scuti.Net;
using Scuti.GraphQL;
using Scuti.GraphQL.Generated;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Scuti.UI
{
    public class CardDetailsForm : Form<CardDetailsForm.Model>
    {
        [Serializable]
        public class Model : Form.Model
        {
            public CreditCardData Card;
            public AddressData Address;
        }

        [SerializeField] InputField cardholderNameInput;
        [SerializeField] InputField cardNumberInput;
        [SerializeField] InputField cvvInput;
        [SerializeField] Text cardType;
        [SerializeField] Toggle makeDefaultToggle;
        [SerializeField] Button saveButton;

        [SerializeField] InputField line1Input;
        [SerializeField] InputField line2Input;
        [SerializeField] InputField cityInput;
        [SerializeField] Dropdown stateDropDown;
        [SerializeField] InputField zipInput;
        [SerializeField] Dropdown countryDropDown;
        [SerializeField] InputField expirationDateInput;
        [SerializeField] InputMonthYearValidator ExpirationValidator;
        [SerializeField] Toggle SaveCard;


        public override void Open()
        {
            base.Open();
            Refresh();
        }

   

        public override void Bind()
        {
            Data.Address.Country = countryDropDown.options[countryDropDown.value].text;
            Data.Address.State = stateDropDown.options[stateDropDown.value].text;

            line1Input.onValueChanged.AddListener(value => {
                Data.Address.Line1 = value;
                });
            line2Input.onValueChanged.AddListener(value => Data.Address.Line2 = value);
            cityInput.onValueChanged.AddListener(value => Data.Address.City = value);
            stateDropDown.onValueChanged.AddListener(value => Data.Address.State = stateDropDown.options[value].text);
            zipInput.onValueChanged.AddListener(value => Data.Address.Zip = value);
            countryDropDown.onValueChanged.AddListener(value => Data.Address.Country = countryDropDown.options[value].text);

            cardholderNameInput.onValueChanged.AddListener(value => Data.Card.Name = value);
            cardNumberInput.onValueChanged.AddListener(value => Data.Card.Number = value);
            ExpirationValidator.OnUpdated += OnExpirationChanged;
            cvvInput.onValueChanged.AddListener(value => Data.Card.CVV = value);
            makeDefaultToggle.onValueChanged.AddListener(value => Data.Card.MakeDefault = value);
            SaveCard.onValueChanged.AddListener(value => Data.Card.SaveCard = value);
        }

        private void OnExpirationChanged()
        {
            Data.Card.ExpirationMonth = ExpirationValidator.Month;
            Data.Card.ExpirationYear = ExpirationValidator.Year;
        }

        public override void Refresh()
        {
            cardholderNameInput.text = Data.Card.Name;
            cardNumberInput.text = Data.Card.Number;

            expirationDateInput.text = Data.Card.ExpirationMonth.ToString("D2") + "/" + (Data.Card.ExpirationYear % 100).ToString("D2");
            cvvInput.text = Data.Card.CVV;
            makeDefaultToggle.isOn = Data.Card.MakeDefault;
            SaveCard.isOn = Data.Card.SaveCard;

            cardType.text = Data.Card.CardType;
            line1Input.text = Data.Address.Line1;
            line2Input.text = Data.Address.Line2;
            cityInput.text = Data.Address.City;
            stateDropDown.SetDropDown(Data.Address.State);
            zipInput.text = Data.Address.Zip;
            countryDropDown.SetDropDown(Data.Address.Country);
        }

        public void Save()
        {
            if (!Evaluate())
            {
                UIManager.Alert.SetHeader("Invalid Field").SetBody("Please ensure all form fields are filled in correctly.").SetButtonText("OK").Show(() => { });
                return;
            }

            Data.Card.CardType = cardType.text;

            SavePayment();
        }

        private async void SavePayment()
        {
            if (!Evaluate())
            {
                UIManager.Alert.SetHeader("Invalid Field").SetBody("Please ensure all form fields are filled in correctly.").SetButtonText("OK").Show(() => { });
                return;
            }

            saveButton.interactable = false;
            try
            {

                JObject cardDetails = new JObject();
                cardDetails["number"] = Data.Card.Number;
                cardDetails["cvv"] = Data.Card.CVV;
                EncryptedInput encryptedInput = await ScutiUtils.Encrypt(cardDetails.ToJson().ToUTF8Bytes());
               
                Data.Card.Encrypted = encryptedInput; 

                Submit();
                Close();
            }
            catch (GQLException ex)
            {
                ScutiLogger.LogException(ex);
                ScutiLogger.LogError(ex.response);
                UIManager.Alert.SetHeader("Error").SetBody("Setting payment info failed. " + ex.responseCode + " " + ex.error).Show(() => { });
            }
            saveButton.interactable = true;
        }

        public override Model GetDefaultDataObject()
        {
            var model = new Model() { Card = new CreditCardData() { ExpirationMonth = DateTime.Now.Month, ExpirationYear = DateTime.Now.Year }, Address = new AddressData() { Line2 = "" } };
            model.Card.Reset();

            model.Address.Country = countryDropDown.options[countryDropDown.value].text;
            model.Address.State = stateDropDown.options[stateDropDown.value].text;

            return model;
        }

        internal void SetCached(UserCard cachedCard, AddressData address)
        {
            if(address!=null)
            {
                Data.Address = address;

            }

            //if (cachedCard != null)
            //{
            //    Data.Card = new CreditCardData() { CardType = cachedCard.;
            //}
                Refresh();
        }
    }
}
