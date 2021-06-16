using UnityEngine.UI;
using UnityEngine;
using System.Threading.Tasks;
using Scuti.Net;
using System;

using Scuti.GraphQL;

namespace Scuti.UI
{
    public class UserCredentialsForm : Form<UserCredentialsForm.Model>
    {
        public class Model : Form.Model
        {
            public string Email;
            public string Password;
            public string fullName;
        }

        [SerializeField] InputField emailInput;
        [SerializeField] InputField passwordInput;
        [SerializeField] Button registerButton;


        public override Model GetDefaultDataObject()
        {
            return new Model();
        }

        public override void Bind()
        {
            emailInput.onValueChanged.AddListener(value => Data.Email = value);
            passwordInput.onValueChanged.AddListener(value => Data.Password = value);
            registerButton.onClick.AddListener(async () => await Register());
        }

        public async Task Register()
        {
            if (!Evaluate())
                return;

            UIManager.ShowLoading();
            try
            {
                await ScutiNetClient.Instance.RegisterUser(Data.Email, Data.Password, Data.fullName == null ? "" : Data.fullName);
                Submit();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);

                string message = ex.Message;
                if (ex is UserRegistrationException)
                {
                    var authException = ex as UserRegistrationException;
                    if (authException.InnerException != null && authException.InnerException is GQLException)
                    {
                        var gqlException = authException.InnerException as GQLException;
                        Debug.LogError(gqlException.error + " " + gqlException.responseCode);

                        message = "Error Response Code: " + gqlException.responseCode;
                        switch (gqlException.responseCode)
                        {
                            case 409:
                                message = "User already exists. Try logging in or request a new password.";
                                break;
                            default:
                                Debug.LogError("TODO: Add more response code messages here. -mg");
                                break;
                        }
                    } 
                }
                UIManager.Alert.SetHeader("Create Account Failed").SetBody(message).SetButtonText("OK").Show(() => { });
            }

            UIManager.HideLoading();
        }
        public override void Refresh()
        {
            emailInput.text = Data.Email;
            passwordInput.text = Data.Password;
        }
    }
}
