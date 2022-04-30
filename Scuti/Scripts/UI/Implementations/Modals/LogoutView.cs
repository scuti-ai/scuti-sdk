using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Scuti
{

    public class LogoutView : View
    {
        [Header("Interaction")]
        [SerializeField] Button acceptButton;
        [SerializeField] Button closeButton;

        System.Action<bool> m_Callback;

        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        void Init()
        {
            onClosed.AddListener(() => m_Callback = null);
            if (acceptButton) acceptButton.onClick.AddListener(OnClickAccept);
            if (closeButton) closeButton.onClick.AddListener(OnClickClose);
        }

        public void Show(System.Action<bool> callback)
        {
            m_Callback = callback;
            Open();
        }

        void OnClickAccept()
        {
            Close();
            m_Callback?.Invoke(true);
        }

        void OnClickClose()
        {
            Close();
            m_Callback?.Invoke(false);
        }



    }


}
