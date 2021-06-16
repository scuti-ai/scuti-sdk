using UnityEngine;
using UnityEngine.UI;
using Scuti;
using Scuti.Net;
using System.Threading.Tasks;
using System;

namespace Scuti.UI
{
    public class PasswordResetViaEmailForm : Form<PasswordResetViaEmailForm.Model>
    {
        public class Model : Form.Model
        {
            public string Email;
        }

        [SerializeField] InputField emailInput;
        [SerializeField] Button resetButton;

        public override void Bind()
        {
            resetButton.onClick.AddListener(async () => await ResetWithEmail());
            emailInput.onValueChanged.AddListener(value => Data.Email = value);
        }

        public override void Refresh()
        {
            emailInput.text = Data.Email;
        }

        public override Model GetDefaultDataObject()
        {
            return new Model();
        }

        public async Task ResetWithEmail()
        {
            if (!Evaluate())
                return;
            resetButton.interactable = false;

            UIManager.ShowLoading();
            try
            {
                await ScutiNetClient.Instance.ResetPasswordByEmail(Data.Email);
                UIManager.HideLoading();

                UIManager.Alert.SetHeader("Password Reset Successful").SetBody($"Please check your email for instructions.").SetButtonText("Ok").Show(() => { });
                Submit();
                Close();
            }
            catch (Exception ex)
            {
                UIManager.HideLoading();
                UIManager.Alert.SetHeader("Password reset failed").SetBody($"Failed to reset password: {ex.Message}").SetButtonText("Ok").Show(() => { });
                ScutiLogger.LogError(ex);
            }
            resetButton.interactable = true;
        }

        public void RequestLogin()
        {
            UIManager.Open(UIManager.Login);
            Close();
        }
    }
}