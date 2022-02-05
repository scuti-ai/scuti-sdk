using UnityEngine;
using UnityEngine.UI;
using Scuti;
using Scuti.Net;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

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
        //[SerializeField] InputField stateInput;
        [SerializeField] InputField zipInput;
        [SerializeField] InputField phoneInput;
        [SerializeField] Dropdown countryDropDown;
        [SerializeField] Button saveButton;
        [SerializeField] Button prevButton;
        [SerializeField] Text saveButtonLabel;
        [SerializeField] Text stateLabel;
        [SerializeField] Validatable stateValidatable;

        public bool UseAsOnboarding = true;
        private bool _cachedAddress = false;

        private List<Dropdown.OptionData> _states;
        private List<Dropdown.OptionData> _provinces;
        private List<Dropdown.OptionData> _countries;
        //private List<string> _countriesList;
        //private List<string> _countriesCodeList;
        private List<SupportedCountry> _supportedCountriesList;
        private SupportedCountry _selectedCountry;

        protected override void Awake()
        {
            base.Awake();

            _states = new List<Dropdown.OptionData>();
            _provinces = new List<Dropdown.OptionData>();
            _countries = new List<Dropdown.OptionData>();

            //_countriesList = ScutiConstants.COUNTRIES.ToList();
            //_countriesCodeList = ScutiConstants.SUPPORTED_COUNTRY_CODES.ToList();

            _supportedCountriesList = ScutiConstants.SUPPORTED_COUNTRIES.Countries;

            foreach (var country in _supportedCountriesList)
            {
                _countries.Add(new Dropdown.OptionData(name = country.Name));
            }

            /*foreach (var state in ScutiConstants.STATES)
            {
                _states.Add(new Dropdown.OptionData( name= state));
            }

            foreach (var prov in ScutiConstants.PROVINCES)
            {
                _provinces.Add(new Dropdown.OptionData(name = prov));
            }*/

            _selectedCountry = _supportedCountriesList[0];

            foreach (var state in _supportedCountriesList[0].Divisions)
            {
                _states.Add(new Dropdown.OptionData(name = state.Name));
            }

            stateDropDown.options = _states;

            Data.Address.State = _selectedCountry.Divisions[0].Code;
            stateDropDown.SetDropDown(_selectedCountry.Divisions[0].Name);

            Data.Address.Country = _selectedCountry.Code;

            countryDropDown.options = _countries;
        }

        public override void Bind()
        {
            Data.Address.Country = _selectedCountry.Code;
            //Data.Address.State = stateInput.text;
            Data.Address.State = _selectedCountry.Divisions.Find(d => d.Name.Equals(stateDropDown.options[stateDropDown.value].text)).Code;

            line1Input.onValueChanged.AddListener(value => Data.Address.Line1 = value);
            line2Input.onValueChanged.AddListener(value => Data.Address.Line2 = value);
            cityInput.onValueChanged.AddListener(value => Data.Address.City = value);
            stateDropDown.onValueChanged.AddListener(value => {
                Data.Address.State = _selectedCountry.Divisions.Find(d => d.Name.Equals(stateDropDown.options[value].text)).Code;
            });
            //stateInput.onValueChanged.AddListener(value => Data.Address.State = value);
            zipInput.onValueChanged.AddListener(value => Data.Address.Zip = value);
            phoneInput.onValueChanged.AddListener(value => Data.Address.Phone = value);
            countryDropDown.onValueChanged.AddListener(OnCountryChanged);
#pragma warning disable 
            saveButton.onClick.AddListener(async () => SaveShippingInfo());
#pragma warning restore 
        }

        private void OnCountryChanged(int value)
        {
            var newValue = countryDropDown.options[value].text;
            if (!Data.Address.Country.Equals(newValue))
            {
                _selectedCountry = _supportedCountriesList.Find(c => c.Name.Equals(newValue));
                Data.Address.Country = _selectedCountry.Code;
                Data.Address.State = _selectedCountry.Divisions[0].Code;

                _states.Clear();
                foreach (var state in _selectedCountry.Divisions)
                {
                    _states.Add(new Dropdown.OptionData(name = state.Name));
                }

                stateDropDown.options = _states;
                Refresh();
            }
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
            /*if (!await ScutiNetClient.Instance.InSupportedCountry())
            {
                UIManager.Alert.SetHeader("Unsupported Location").SetButtonText("Ok").SetBody("We currently do not support your location. International support coming soon!").Show(() => { });
            }*/

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
                            Phone = shippingInfo.Phone,
                            Country = shippingInfo.Country,
                            City = shippingInfo.City
                        };
                        _cachedAddress = true;
                        _selectedCountry = _supportedCountriesList.Find(c => c.Code.Equals(Data.Address.Country));
                        Refresh();
                    }
                }
        }

        public override void Refresh()
        {
            line1Input.text = Data.Address.Line1;
            line2Input.text = Data.Address.Line2;
            cityInput.text = Data.Address.City;

            var state = _selectedCountry.Divisions.Find(d => d.Code.Equals(Data.Address.State));
            countryDropDown.SetDropDown(_selectedCountry.Name);

            if (stateValidatable) stateValidatable.SetMessage(_selectedCountry.DivisionName + "*");
            stateLabel.text = _selectedCountry.DivisionName + "*";
            //stateInput.placeholder.GetComponent<Text>().text = "Your "+ ScutiConstants.PROVINCES_LABEL[_countriesList.IndexOf(countryDropDown.options[countryDropDown.value].text)];

            /*if(Data.Address.Country.Equals("US"))
            {
                stateLabel.text = "State";
                stateDropDown.options = _states;
                zipInput.characterLimit = 5;
                zipInput.contentType = InputField.ContentType.IntegerNumber;
            } else
            {
                stateLabel.text = "Province";
                stateDropDown.options = _provinces;
                zipInput.characterLimit = 7;
                zipInput.contentType = InputField.ContentType.Alphanumeric;
            }*/
            zipInput.text = Data.Address.Zip;
            phoneInput.text = Data.Address.Phone;
            //stateDropDown.SetDropDown(Data.Address.State);
            stateDropDown.SetDropDown(state.Name);
        }

        private async Task SaveShippingInfo()
        {
            if (!Evaluate())
            {
                UIManager.Alert.SetHeader("Invalid Field").SetBody("Please ensure all form fields are filled in correctly.").SetButtonText("OK").Show(() => { });
                return;
            }

            saveButton.interactable = false;
            bool submit = !UseAsOnboarding;
            if (UseAsOnboarding || !_cachedAddress)
            {
                try
                {
                    await ScutiAPI.EditShippingAddress(Data.Address.Line1, Data.Address.Line2, Data.Address.City, Data.Address.State, Data.Address.Country, Data.Address.Zip, Data.Address.Phone);
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
                Country = _selectedCountry.Code,
                State = _selectedCountry.Divisions.Find(d => d.Name.Equals(stateDropDown.options[stateDropDown.value].text)).Code
            };
            return model;
        }
    }
}
