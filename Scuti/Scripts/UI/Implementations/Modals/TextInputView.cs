 
﻿using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Scuti
{
    public class TextInputView : View
    {
        public event Action<string> OnSubmit;

        [SerializeField] InputField inputField;

        [SerializeField] Text bodyText;
        [SerializeField] Text buttonText;

        Action<string> m_Callback;

        public void Submit()
        {
            OnSubmit?.Invoke(inputField.text);
            m_Callback?.Invoke(inputField.text);
            Close();
        }

        public TextInputView Clear()
        {
            inputField.text = string.Empty;
            if(bodyText!=null) bodyText.text = string.Empty;
            buttonText.text = string.Empty;
            return this;
        }


        public TextInputView SetBody(string body)
        {
            bodyText.text = body;
            return this;
        }

        public TextInputView SetButtonText(string buttonText)
        {
            this.buttonText.text = buttonText;
            return this;
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

        public override void Close()
        {
            base.Close();
            Clear();
        }
    }
}