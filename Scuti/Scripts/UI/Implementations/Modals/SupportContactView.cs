
using Scuti.Net;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Scuti
{
    public class SupportContactView : View
    {
        public event Action<string> OnSubmit;

        [SerializeField] InputField inputField;
        [SerializeField] InputField emailInputField;
        [SerializeField] Text EmailPlaceholder;

        private string defaultText;
        private Color defaultColor;


        Action<string> m_Callback;

        protected override void Awake()
        {
            defaultText = EmailPlaceholder.text;
            defaultColor = EmailPlaceholder.color;
        }

        public void Submit()
        {
            if (inputField.text.Length > 0)
            {
                if (emailInputField.text.Length < 1)
                {
                    EmailPlaceholder.text = "EMAIL REQUIRED";
                    EmailPlaceholder.color = Color.red;
                }
                else
                {
                    ScutiAPI.SendEmail(ScutiUtils.GetSupportInfo(emailInputField.text), inputField.text, ScutiUtils.GetSupportEmail());
                    OnSubmit?.Invoke(inputField.text);
                    m_Callback?.Invoke(inputField.text);
                    Clear();
                    Close();
                }
            }
        }


        public void Clear()
        {
            inputField.text = string.Empty;

            EmailPlaceholder.text = defaultText;
            EmailPlaceholder.color = defaultColor;
        }
 

        public Task Show()
        {
            var source = new TaskCompletionSource<bool>();
            Show((string input) => source.SetResult(true));
            return source.Task;
        }

        public void Show(Action<string> callback)
        {
            Open();
            m_Callback = callback;
        }

        //public override void Close()
        //{
        //    base.Close();
        //    Clear();
        //}
    }
}