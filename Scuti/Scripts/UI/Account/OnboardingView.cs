﻿using System;
using System.Threading.Tasks;

using UnityEngine;


using Scuti.Net;

// WIP
namespace Scuti.UI
{
    public class OnboardingView : View
    {

        [SerializeField] Navigation navigation;
        [SerializeField] UserCredentialsForm credentials;
        [SerializeField] UserDetailsForm userDetails;
        [SerializeField] AddressForm addressForm;
        [SerializeField] InterestsSelector interestSelect;
        [SerializeField] VerifyEmailView verifyEmail;
        [SerializeField] ManagerAccount managerAccount;


        protected override void Awake()
        {
            base.Awake();
            onStartOpening.AddListener(() =>
            {
                navigation.Open(credentials);
            });

            credentials.OnSubmit += creds =>
            {
                ShowAccountVerification(creds.Email, creds.Password);
            };

            verifyEmail.VerificationConfirmed += () =>
            {
                //ShowDetails();
                Close();
            };
            credentials.OnCancel += () =>
            {
                // Ask for confirmation
                navigation.Close();
            };

            userDetails.OnSubmit += (details) =>
            {
                if (!ScutiNetClient.Instance.FinishedOnBoarding)
                {
                    ShowAddressForm();
                    //navigation.Close();
                }
                else
                    Close();
            };
            userDetails.OnCancel += () =>
            {
                if (!ScutiNetClient.Instance.FinishedOnBoarding)
                {
                    navigation.Open(credentials);
                    //navigation.Close();
                }
                else
                    Close();
            };

            addressForm.OnSubmit += address =>
            {
                if (!ScutiNetClient.Instance.FinishedOnBoarding)
                {
                    ShowCategories();
                    //navigation.Close();
                }
                else
                    Close();
            };
            addressForm.OnCancel += () =>
            {
                if (!ScutiNetClient.Instance.FinishedOnBoarding)
                {
                    ShowDetails();
                    //navigation.Close();
                }
                else
                    Close();
            };

            interestSelect.OnSubmit += interests =>
            {
                Close();
            };
            interestSelect.OnCancel += () =>
            {
                if (!ScutiNetClient.Instance.FinishedOnBoarding)
                {
                    ShowAddressForm();
                    //navigation.Close();
                }
                else
                    Close();
            };

            managerAccount.DeletedConfirmed += () =>
            {                
                Close();
                ScutiNetClient.Instance.Logout();
                UIManager.Open(UIManager.Login);
            };

        }

        #region For UnitTest

        public InterestsSelector GetInterestsSelector()
        {
            return interestSelect;
        }
        public AddressForm GetAddressForm()
        {
            return addressForm;
        }

        public InterestsSelector GetInterestSelector()
        {
            return interestSelect;
        }

        public UserCredentialsForm GetUserCredentialForm()
        {
            return credentials;
        }


        #endregion

        public void ShowCategories()
        {
            navigation.Open(interestSelect);
        }

        public void ShowAccountVerification(string email, string pwd)
        {
            verifyEmail.SetCredentials(email, pwd);
            navigation.Open(verifyEmail);
        }

        public void ShowDetails()
        {
            navigation.Open(userDetails);
        }

        public void ShowManageAccount()
        {
            navigation.Open(managerAccount);
        }

        public void ShowAddressForm()
        {
            navigation.Open(addressForm);
        }


        public void Cancel()
        {
            UIManager.RefreshLoading();
            Close();
        }

        public void ShowPrivacy()
        {
            Application.OpenURL(ScutiConstants.PRIVACY_POLICY_URL);
        }
    }
}
