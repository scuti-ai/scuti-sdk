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
using System.Collections.Generic;

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
        [SerializeField] Text stateLabel;


        private List<Dropdown.OptionData> _states;
        private List<Dropdown.OptionData> _provinces;

        protected override void Awake()
        {
            base.Awake();

            _states = new List<Dropdown.OptionData>();
            _provinces = new List<Dropdown.OptionData>();
            foreach (var state in ScutiConstants.STATES)
            {
                _states.Add(new Dropdown.OptionData(name = state));
            }

            foreach (var prov in ScutiConstants.PROVINCES)
            {
                _provinces.Add(new Dropdown.OptionData(name = prov));
            }
        }


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
            countryDropDown.onValueChanged.AddListener(OnCountryChanged);

            cardholderNameInput.onValueChanged.AddListener(value => Data.Card.Name = value);
            cardNumberInput.onValueChanged.AddListener(value => Data.Card.Number = value);
            ExpirationValidator.OnUpdated += OnExpirationChanged;
            cvvInput.onValueChanged.AddListener(value => Data.Card.CVV = value);
            makeDefaultToggle.onValueChanged.AddListener(value => Data.Card.MakeDefault = value);
            SaveCard.onValueChanged.AddListener(value => Data.Card.SaveCard = value);
        }

        private void OnCountryChanged(int value)
        {
            var newValue = countryDropDown.options[value].text;
            if (!Data.Address.Country.Equals(newValue))
            {
                Data.Address.Country = newValue;
                Refresh();
            }
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
            countryDropDown.SetDropDown(Data.Address.Country);
            if (Data.Address.Country.Equals("US"))
            {
                stateLabel.text = "State*";
                stateDropDown.options = _states;
                zipInput.characterLimit = 5;
                zipInput.contentType = InputField.ContentType.IntegerNumber;
            }
            else
            {
                stateLabel.text = "Province*";
                stateDropDown.options = _provinces;
                zipInput.characterLimit = 7;
                zipInput.contentType = InputField.ContentType.Alphanumeric;
            }
            zipInput.text = Data.Address.Zip;
            stateDropDown.SetDropDown(Data.Address.State);
        }

        public void Save()
        {
            Debug.Log("CardDetails: " + cardType.text);

            if (!Evaluate())
            {
                UIManager.Alert.SetHeader("Invalid Field").SetBody("Please ensure all form fields are filled in correctly.").SetButtonText("OK").Show(() => { });
                return;
            }
            Data.Card.CardType = cardType.text;

            SavePayment();
        }

        // --------------------------------------

        public async void DeleteCard()
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

        // -------------------------------------



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
                //
                await ScutiAPI.CreateOrReplaceCard(Data.Card.ExpirationMonth, Data.Card.ExpirationYear,
                    Data.Card.Name,
                    Data.Card.Encrypted,
                    GetBillingAddress());
                //
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

        // --------------------------------------

        private AddressInput GetBillingAddress()
        {
            AddressInput address = null;
            if (Data.Address != null && Data.Address.IsValid())
            {
                address = new AddressInput()
                {
                    Address1 = Data.Address.Line1,
                    Address2 = Data.Address.Line2,
                    City = Data.Address.City,
                    Country = Data.Address.Country,
                    State = Data.Address.State,
                    ZipCode = Data.Address.Zip
                };                
            }

            return address;
        }

    }
}
