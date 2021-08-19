using System;

using UnityEngine;

namespace Scuti {
    public abstract class Presenter<T> : View<T> where T : Presenter.Model {
        /// <summary>
        /// The state object for the <see cref="StatefulUI"/> instance
        /// </summary> 
        public new T Data {
            get {
                return GetData();
            }
            set {
                SetData(value); 
            }
        }

        public T GetData() {
            return m_Data;
        }


        public virtual void SetData(T data) {
            if (_destroyed) return;
            if (data == null)
                throw new Exception("Cannot set Presenter.Data to null");

            m_Data?.Dispose();
            m_Data = data;
            if (m_Data != null) m_Data.OnEvent += OnEvent;
            OnSetState();
        }

        /// <summary>
        /// Callback for when the state object fires 
        /// </summary>
        /// <param name="name">Name of the event fired, if any</param>
        /// <param name="payload">Payload object associated with the event, if any</param>
        public virtual void OnEvent(string name, object payload) { }

        /// <summary>
        /// Callback for when the state object changes
        /// </summary>
        protected virtual void OnSetState() { }

        [ContextMenu("Refresh State")]
        public void Refresh() {
            OnSetState(); 
        }

        protected override void OnDestroy()
        {
            if (m_Data != null) m_Data.OnEvent -= OnEvent;
            m_Data?.Dispose();
            base.OnDestroy();
        }

    }
}