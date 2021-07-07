using System;
using UnityEngine;
using UnityEngine.UI;
using Scuti;
using System.Threading.Tasks;
using Scuti.Net;
using System.Collections.Generic;
using Scuti.GraphQL;

namespace Scuti.UI
{
    public class LoginForm : Form<LoginForm.Model>
    {
        public class Model : Form.Model
        {
            public string Email;
            public string Password;
        }


        [SerializeField] InputField emailInput;
        [SerializeField] InputField passwordInput;
        [SerializeField] Button loginButton;


        public override void Bind()
        {
            emailInput.onValueChanged.AddListener(value => Data.Email = value);
            passwordInput.onValueChanged.AddListener(value => Data.Password = value);
            loginButton.onClick.AddListener(async () => await Login());
        }

        public override void Refresh()
        {
            emailInput.text = Data.Email;
            passwordInput.text = Data.Password;
        }

        public override Model GetDefaultDataObject()
        {
            return new Model();
        }

        public async Task Login()
        {
            if (!Evaluate())
            {
                UIManager.Alert.SetHeader("Invalid Field").SetBody("Please ensure all form fields are filled in correctly.").SetButtonText("OK").Show(() => { });
                return;
            }


            UIManager.ShowLoading();
            string fullName = null;
            try
            {
                loginButton.interactable = false;
                await ScutiNetClient.Instance.AuthenticateUser(Data.Email, Data.Password);
                var response = await ScutiNetClient.Instance.GetAccountInfo();
                fullName = response.fullName;
                UIManager.TopBar.Refresh();
                UIManager.HideLoading();
            }
            catch(Exception ex)
            {
                UIManager.HideLoading();
                ScutiLogger.LogError(ex);
                loginButton.interactable = true;
                string message = ex.Message;
                if (ex is ScutiNetClient.UserAuthenticationException)
                {
                    var authException = ex as ScutiNetClient.UserAuthenticationException;
                    if(authException.InnerException!=null && authException.InnerException is GQLException)
                    {
                        var gqlException = authException.InnerException as GQLException;

                        message = "Response Code: "+gqlException.responseCode;
                        switch (gqlException.responseCode)
                        {
                            case 400:
                                message = "Please check your username/password.";
                                break;
                            case 401:
                                if(gqlException.response.ToLower().Contains("email is not verified"))
                                {
                                    UIManager.Open(UIManager.Onboarding);
                                    UIManager.Onboarding.ShowAccountVerification(Data.Email, Data.Password);
                                    Close();
                                    return;
                                } else 
                                    message = "Invalid Credentials. Please check your username/password.";
                                break;
                            default:
                                Debug.LogError("TODO: Add more response code messages here. -mg "+message);
                                break;
                        }
                    }
                }  
                UIManager.Alert.SetHeader("Login Failed").SetBody(message).SetButtonText("OK").Show(() => { });
                return;
            }

            //TODO: Check if user profile is completed after user make a purchase not before
            /*if (string.IsNullOrEmpty(fullName))
            {
                UIManager.Open(UIManager.Onboarding);
                UIManager.Onboarding.ShowDetails();
                UIManager.Alert.SetHeader("Finish Creating Account").SetBody("Please finish creating your account on the following screen.").SetButtonText("OK").Show(() => { });
            }
            else
            {*/
                try
                {
                    var diff = await ScutiNetClient.TryToActivateRewards();

                    if (diff > 0)
                    {
                        UIManager.Rewards.SetData(new RewardPresenter.Model() { reward = (int)diff, subtitle = "Collect your daily login rewards!", title = "CONGRATULATIONS!" });
                        UIManager.Open(UIManager.Rewards);
                    }
                   
                } catch { }

            //}

            Submit();
            loginButton.interactable = true;
            Close();
        }

        public override void Cancel()
        {
            base.Cancel();
            UIManager.Open(UIManager.Offers);
        }
    }
}
