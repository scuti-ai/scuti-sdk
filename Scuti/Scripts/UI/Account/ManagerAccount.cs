
using Scuti.GraphQL;
using Scuti.Net;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Scuti.UI
{
    public class ManagerAccount : View
    {

        public Action DeletedConfirmed;

        protected override void Awake()
        {
            base.Awake();
        }
        public override void Open()
        {
            base.Open();
        }


        public void DeleteAccountButton()
        {

            UIManager.Confirmation.SetHeader("Delete account").SetBody("Are you sure you want to delete your account??").SetPositive("Yes").SetNegative("No").Show((bool callback) => {
                if (callback)
                    DeleteAccount();
                else
                    return;
            });

        }

        private async void DeleteAccount()
        {
            try
            {
                var response = await ScutiNetClient.Instance.DeleteAccount();
                DeletedConfirmed?.Invoke();

            }
            catch (Exception e)
            {
                ScutiLogger.LogError(e + "" + e.InnerException);
            }
        }



    }

}
