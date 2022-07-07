
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

        public void SetMessage(string message)
        {
            Description.text = message;
        }

        public override void Open()
        {
            UIManager.SetFirstSelected(firstSelection);
            base.Open();
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
