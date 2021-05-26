 
﻿using System;
using System.Threading.Tasks;

namespace Scuti {
    public class AttentionView : View {
        Action m_Callback;

        public Task Show() {
            var source = new TaskCompletionSource<bool>();
            Show(() => source.SetResult(true));
            return source.Task;
        }

        public void Show(Action callback) {
            Open();
            m_Callback = callback;
        }

        public void OnClick() {
            Close();
            m_Callback?.Invoke();
            m_Callback = null;
        }
    }
} 
