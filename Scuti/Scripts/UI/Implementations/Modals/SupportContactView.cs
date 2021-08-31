 
﻿using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Scuti
{
    public class SupportContactView : View
    {
        public event Action<string> OnSubmit;

        [SerializeField] InputField inputField;

        Action<string> m_Callback;

        public void Submit()
        {
            OnSubmit?.Invoke(inputField.text);
            m_Callback?.Invoke(inputField.text);
            Close();
        }

        public void Clear()
        {
            inputField.text = string.Empty;
        }
 

        public Task Show()
        {
            Clear();
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