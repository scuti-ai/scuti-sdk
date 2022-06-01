using UnityEngine.UI;
using UnityEngine;
using System.Threading.Tasks;
using Scuti.Net;
using System;
using System.Collections.Generic;


using Scuti.GraphQL;


namespace Scuti.UI
{
    public class UserCreateCredentialForm : Form<UserCreateCredentialForm.Model>
    {
        public class Model : Form.Model
        {
            public string Email;
            public string Password;

            public string fullName;
            public string phoneNumber;
            public string gender = "Male";
            public DateTime birthDay;
        }

        // For email password
        [SerializeField] InputField emailInput;
        [SerializeField] InputField passwordInput;
        [SerializeField] Button registerButton;

        //For info User
        [SerializeField] InputField NameInput;
        [SerializeField] Dropdown GenderDropDown;
        [SerializeField] Dropdown YearDropDown;

        #region UserDetails
        
        protected override void Awake()
        {
            base.Awake();

            Debug.Log("Opened Form");

            YearDropDown.ClearOptions();

            var year = DateTime.Now.Year;
            List<Dropdown.OptionData> years = new List<Dropdown.OptionData>();
            for (var i = 0; i < 100; i++)
            {
                Debug.Log("Year: " + year);
                years.Add(new Dropdown.OptionData(year.ToString(), null));
                year--;

            }
            YearDropDown.AddOptions(years);
        }

        public override void Open()
        {
            base.Open();
            Data.birthDay = new DateTime(2000, 2, 15);
            Data.gender = "Male";
            Data.phoneNumber = "3214567878"; //For testing
            Data.fullName = "";         
        }     

        private bool setBirthdayAtDropDownChange = true;

        #endregion region

        public override Model GetDefaultDataObject()
        {
            return new Model();
        }

        public override void Bind()
        {
            emailInput.onValueChanged.AddListener(value => Data.Email = value);
            passwordInput.onValueChanged.AddListener(value => Data.Password = value);
            registerButton.onClick.AddListener(async () => await Register());

            NameInput.onValueChanged.AddListener(value => Data.fullName = value);
            GenderDropDown.onValueChanged.AddListener(value => Data.gender = GenderDropDown.options[value].text);
            YearDropDown.onValueChanged.AddListener(value => { if (setBirthdayAtDropDownChange) Data.birthDay = new DateTime(int.Parse(YearDropDown.options[YearDropDown.value].text), 12, 29); });

        }
       
        public async Task Register()
        {
            if (!Evaluate())
            {
                UIManager.Alert.SetHeader("Invalid Field").SetBody("Please ensure all form fields are filled in correctly.").SetButtonText("OK").Show(() => { });
                return;
            }

            UIManager.ShowLoading(false);
            try
            {  
                await ScutiNetClient.Instance.RegisterUser(Data.Email, Data.Password, Data.fullName, Data.gender, Data.birthDay.Year.ToString());
                Submit();
            }
            catch (Exception ex)
            {
                ScutiLogger.LogError(ex);

                string message = ex.Message;
                if (ex is UserRegistrationException)
                {
                    var authException = ex as UserRegistrationException;
                    if (authException.InnerException != null && authException.InnerException is GQLException)
                    {
                        var gqlException = authException.InnerException as GQLException;
                        ScutiLogger.LogError(gqlException.error + " " + gqlException.responseCode);

                        message = "Error Response Code: " + gqlException.responseCode;
                        switch (gqlException.responseCode)
                        {
                            case 409:
                                message = "User already exists. Try logging in or request a new password.";
                                break;
                            default:
                                //Debug.LogError("TODO: Add more response code messages here. -mg");
                                break;
                        }
                    }
                }
                UIManager.Alert.SetHeader("Create Account Failed").SetBody(message).SetButtonText("OK").Show(() => { });
            }
            UIManager.HideLoading(false);
        }

        public override void Refresh()
        {
            emailInput.text = Data.Email;
            passwordInput.text = Data.Password;
            /////
            setBirthdayAtDropDownChange = false;
            NameInput.text = Data.fullName;
            GenderDropDown.SetDropDown(Data.gender);
            YearDropDown.SetDropDown(Data.birthDay.Year.ToString());
            //MonthDropDown.SetDropDown(Data.birthDay.Month.ToString());
            //DayDropDown.SetDropDown(Data.birthDay.Day.ToString());
            setBirthdayAtDropDownChange = true;
        }

    }


}

