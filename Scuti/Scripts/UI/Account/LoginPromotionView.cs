
using Scuti.GraphQL;
using Scuti.Net;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scuti.UI
{
    public class LoginPromotionView : View
    {
        public TextMeshProUGUI Description;
        public string DefaultText = "Create an Account to Earn Rewards";

        public override void Open()
        {
            base.Open();
            SetMessage(DefaultText);
        }

        public void SetMessage(string message)
        {
            Description.text = message;
        }

        public void ShowLogin()
        {
            UIManager.Open(UIManager.Login);
            Close();
        }

        public void ShowAccountCreation()
        {
            UIManager.Open(UIManager.Onboarding);
            Close();
        }

    }
}
