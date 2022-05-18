using Scuti;
using Scuti.Net;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Scuti.UI
{
    public class PasswordEditForm : Form<PasswordEditForm.Model>
    {
        public class Model : Form.Model
        {
            public string CurrentPassword;
            public string NewPassword;
            public string confirmNewPassword;
        }

        [SerializeField] InputField currentPasswordInput;
        [SerializeField] InputField newPasswordInput;
        [SerializeField] Button changeButton;


        public bool isPassChangedSuccess;

        public override void Bind()
        {
            currentPasswordInput.onValueChanged.AddListener(value => Data.CurrentPassword = value);
            newPasswordInput.onValueChanged.AddListener(value => Data.NewPassword = value);
            changeButton.onClick.AddListener(async () => await ChangePassword());
        }

        public override void Refresh()
        {
            currentPasswordInput.text = Data.CurrentPassword;
            newPasswordInput.text = Data.NewPassword;
        }

        private async Task ChangePassword()
        {
            if (!Evaluate())
            {
                UIManager.Alert.SetHeader("Invalid Field").SetBody("Please ensure all form fields are filled in correctly.").SetButtonText("OK").Show(() => { });
                return;
            }
            UIManager.ShowLoading(false);

            isPassChangedSuccess = false;
            changeButton.interactable = false;
            try
            {
                await ScutiNetClient.Instance.ChangePassword(Data.CurrentPassword, Data.NewPassword);
                isPassChangedSuccess = true;
                Submit();
                Close();
            }
            catch(Exception ex)
            {
                isPassChangedSuccess = false;
                UIManager.Alert.SetHeader("Failed to change password").SetBody($"Failed to change password").SetButtonText("Ok").Show(() => { });
                ScutiLogger.LogException(ex);
            }

            UIManager.HideLoading(false);
            changeButton.interactable = true;
        }

        public override Model GetDefaultDataObject()
        {
            return new Model();
        }

        public void OnChangePassword()
        {
            ChangePassword();
        }
    }
}
