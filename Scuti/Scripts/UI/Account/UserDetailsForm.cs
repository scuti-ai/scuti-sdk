using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using Scuti.Net;
using System.Collections.Generic;

namespace Scuti.UI
{
    public class UserDetailsForm : Form<UserDetailsForm.Model>
    {
        public class Model : Form.Model
        {
            public string fullName;
            public string phoneNumber;
            public string gender = "Male";
            public DateTime birthDay;
        }

        [SerializeField] InputField NameInput; 
        [SerializeField] Dropdown GenderDropDown;
        [SerializeField] Dropdown YearDropDown;
 
        [SerializeField] Button saveButton;
        [SerializeField] Button prevButton;
        [SerializeField] Text saveButtonLabel;


        protected override void Awake()
        {
            base.Awake();

            var year = DateTime.Now.Year;
            List<Dropdown.OptionData> years = new List<Dropdown.OptionData>();
            for (var i = 0; i < 100; i++)
            {
                years.Add(new Dropdown.OptionData(year.ToString(), null));
                year--;
            }
            YearDropDown.AddOptions(years);
        }

        public override void Bind()
        {
            NameInput.onValueChanged.AddListener(value => Data.fullName = value); 
            GenderDropDown.onValueChanged.AddListener(value => Data.gender = GenderDropDown.options[value].text);
            YearDropDown.onValueChanged.AddListener(value => { if (setBirthdayAtDropDownChange) Data.birthDay = new DateTime(int.Parse(YearDropDown.options[YearDropDown.value].text), 12, 29); });
            saveButton.onClick.AddListener(async () => await SaveDetails());
        }

        public override void Open()
        {
            UIManager.SetFirstSelected(firstSelection);

            base.Open();
            if (ScutiNetClient.Instance.FinishedOnBoarding)
            {
                if (prevButton)
                    prevButton.gameObject.SetActive(false);

                if (saveButtonLabel)
                    saveButtonLabel.text = "SAVE";
            }
            else
            {
                if(prevButton)
                    prevButton.gameObject.SetActive(true);
                if(saveButtonLabel)
                    saveButtonLabel.text = "NEXT STEP";
                Data.birthDay = new DateTime(2000, 2, 15);
                Data.gender = "Male";
                Data.phoneNumber = "";
                Data.fullName = "";
            }

            TryToLoadData();
        }

        private async void TryToLoadData()
        {
            try
            {
                Me info = await ScutiAPI.GetPersonalInfo();
                if (info != null && info.personalInfo != null)
                {
                    Data.fullName = info.fullName;
                    Data.gender = info.personalInfo.Gender;
                    Data.phoneNumber = info.personalInfo.Phone;
                    Data.birthDay = DateTime.Parse(info.personalInfo.BirthDate.ToString());
                    Refresh();
                }
            }
            catch (Exception ex)
            {
                ScutiLogger.LogException(ex);
                UIManager.Alert.SetHeader("Error").SetBody("Loading user data failed").Show(() => Close());
            }
        }

        private async Task SaveDetails()
        {
            if (!Evaluate())
            {
                UIManager.Alert.SetHeader("Invalid Field").SetBody("Please ensure all form fields are filled in correctly.").SetButtonText("OK").Show(() => { });
                return;
            }
            saveButton.interactable = false;
            bool submit = false;
            try
            {
                await ScutiAPI.EditPersonalInfo(Data.gender, Data.birthDay, Data.phoneNumber, Data.fullName);
                submit = true;
            }
            catch (Exception ex)
            {
                ScutiLogger.LogException(ex);
                UIManager.Alert.SetHeader("Error").SetBody("Setting personal info failed").Show(() => { });
            }


            // Don't submit in the try catch as it could catch errors unrelated to the actual call
            if (submit)
            {
                Submit();
            }

            saveButton.interactable = true;
        }
        private bool setBirthdayAtDropDownChange = true;
        public override void Refresh()
        {
            setBirthdayAtDropDownChange = false;
            NameInput.text = Data.fullName; 
            GenderDropDown.SetDropDown(Data.gender);
            YearDropDown.SetDropDown(Data.birthDay.Year.ToString());
            //MonthDropDown.SetDropDown(Data.birthDay.Month.ToString());
            //DayDropDown.SetDropDown(Data.birthDay.Day.ToString());
            setBirthdayAtDropDownChange = true;
        }


        public override Model GetDefaultDataObject()
        {
            return new Model();
        }
    }
}