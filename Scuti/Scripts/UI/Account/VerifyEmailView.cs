
using Scuti.GraphQL;
using Scuti.Net;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Scuti.UI
{
    public class VerifyEmailView : View
    {
        public Action VerificationConfirmed;

        public Text Description;
        public Text Title;
        public Button SkipButton;
        public Button VerifyButton;

        private string _defaultTitle;
        private string _defaultDescription;


        private string _cachedEmail;
        private string _cachedPwd;

        protected override void Awake()
        {
            base.Awake();
            _defaultTitle = Title.text;
            _defaultDescription = Description.text;
        }

        public override void Open()
        {
            base.Open();
            Title.text = _defaultTitle;
            Description.text = _defaultDescription;
            Description.color = Color.white;
            //SkipButton.gameObject.SetActive(true);
        }
        public void SetCredentials(string email, string pwd)
        {
            _cachedEmail = email;
            _cachedPwd = pwd;
        }

        public void CheckVerification()
        {
            CheckVerificationHelper();
        }

        private async void CheckVerificationHelper()
        {
            // Attempt login
            try
            {
                await ScutiNetClient.Instance.AuthenticateUser(_cachedEmail, _cachedPwd);

                var diff = await ScutiNetClient.TryToActivateRewards();

                if (diff > 0)
                {
                    UIManager.Rewards.SetData(new RewardPresenter.Model() { reward = (int)diff,  subtitle = "Collect your account creation rewards!", title = "CONGRATULATIONS!" });
                    UIManager.Open(UIManager.Rewards);
                } 
                //VerifyButton.interactable = true;
                Verified();
            }
            catch (Exception ex)
            {
                ScutiLogger.LogError(ex);
                //VerifyButton.interactable = true;
                //string message = ex.Message;
                //if (ex is ScutiNetClient.UserAuthenticationException)
                //{
                //    var authException = ex as ScutiNetClient.UserAuthenticationException;
                //    if (authException.InnerException != null && authException.InnerException is GQLException)
                //    {
                //        var gqlException = authException.InnerException as GQLException;
                //        Debug.LogError(gqlException.error + " " + gqlException.responseCode);

                //        message = "Response Code: " + gqlException.responseCode;
                //        switch (gqlException.responseCode)
                //        {
                //            case 400:
                //                message = "Please check your username/password.";
                //                break;
                //            case 401:
                //                message = "Invalid Credentials. Please check your username/password.";
                //                break;
                //            default:
                //                Debug.LogError("TODO: Add more response code messages here. -mg");
                //                break;
                //        }
                //    }
                //}
                //UIManager.Alert.SetHeader("Login Failed").SetBody(message).SetButtonText("OK").Show(() => { });
                Failed();
            }
        }

        private void Verified()
        {
            VerificationConfirmed?.Invoke();
        }

        private void Failed()
        {
            //SkipButton.gameObject.SetActive(false);
            Description.text = "Your e-mail has not been verfied. Please check your e-mail from Scuti before continuing.";
            Description.color = Color.red;
        }


    }
}
