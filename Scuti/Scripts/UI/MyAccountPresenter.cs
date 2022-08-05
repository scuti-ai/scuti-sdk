using System;
using UnityEngine;
using UnityEngine.UI;
using Scuti;
using Scuti.Net;

namespace Scuti.UI
{
    public class MyAccountPresenter : Presenter<MyAccountPresenter.Model>
    {
        public class Model : Presenter.Model
        {
            public const string PROFILE_PICTURE_CHANGED = "profile-picture-changed";

            public Sprite ProfilePicture;
            public string Name;
            public string Location;
        }
         
        [SerializeField] Image profilePictureImage;
        [SerializeField] Text nameText;
        [SerializeField] Text locationText;

        protected void OnSetData()
        {
            profilePictureImage.sprite = Data.ProfilePicture;
            nameText.text = Data.Name;
            locationText.text = Data.Location;
        }

        public override void Open()
        {
            UIManager.SetFirstSelected(firstSelection);
            base.Open();
        }


        public override void OnEvent(string name, object payload)
        {
            switch (name)
            {
                case Model.PROFILE_PICTURE_CHANGED:
                    profilePictureImage.sprite = (Sprite)payload;
                    break;
            }
        }
  

        public void SelectAccountDetails()
        {
            UIManager.Open(UIManager.Onboarding);
            UIManager.Onboarding.ShowDetails();
        }

        public void SelectManageAccount()
        {
            UIManager.Open(UIManager.Onboarding);
            UIManager.Onboarding.ShowManageAccount();
            Close();
        }


        public void SelectEditAddress()
        {
            UIManager.Open(UIManager.Onboarding);
            UIManager.Onboarding.ShowAddressForm();
        }

        public void SelectManageCategories()
        {
            UIManager.Open(UIManager.Onboarding);
            UIManager.Onboarding.ShowCategories();
            Close();
        }



        public void Logout()
        {
            ScutiNetClient.Instance.Logout();
            Close();
            UIManager.Open(UIManager.Login);
        }

       

    }
}
