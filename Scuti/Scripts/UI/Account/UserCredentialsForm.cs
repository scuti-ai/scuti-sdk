using UnityEngine.UI;
using UnityEngine;
using System.Threading.Tasks;
using Scuti.Net;
using System;

//#if UNITY_IOS
//using Unity.Advertisement.IosSupport;
//#endif

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
/*
        private void RequestTracking()
        {
#if UNITY_IOS
            var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            switch(status)
            {
                case ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED:
                    ATTrackingStatusBinding.RequestAuthorizationTracking();
                    break;
                case ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED:
                    Register();
                    break;
                case ATTrackingStatusBinding.AuthorizationTrackingStatus.RESTRICTED:
                    UIManager.Alert.SetHeader("Permission Needed").SetButtonText("Ok").SetBody("Apple requires permission for Scuti you to earn rewards across all games on the Scuti network. Please visit your device's Settings->Privacy->Tracking and enable the setting before registering.").Show(() => { });
                    break;
                case ATTrackingStatusBinding.AuthorizationTrackingStatus.DENIED:
                    ATTrackingStatusBinding.RequestAuthorizationTracking();
                    break;

            }
#else
            Register();
#endif
        }
        */

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
                await ScutiNetClient.Instance.RegisterUser(Data.Email, Data.Password, Data.fullName == null ? "" : Data.fullName, "Male", "1980");
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
        }
    }
}
