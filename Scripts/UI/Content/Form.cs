using System;
using UnityEngine;

namespace Scuti
{
    public abstract partial class Form<T> : View<T> where T : Form.Model
    {

        public event Action OnCancel;
        public event Action<T> OnSubmit;

        [SerializeField] protected ValidatorGroup validatorGroup;

        public abstract void Bind();
        public abstract void Refresh();
        public abstract T GetDefaultDataObject();
        public virtual void Clear() { }

        public new T Data
        {
            get
            {
                if (m_Data == null)
                    m_Data = GetDefaultDataObject();
                return m_Data;
            }
            set { m_Data = value; }
        }

        protected virtual void Start()
        {
            Bind();
        }

        public void ShowDefault()
        {
            Data = GetDefaultDataObject();
            Refresh();
        }

        public bool Evaluate()
        {
            if (validatorGroup == null)
                return true;
            return validatorGroup.Evaluate();
        }

        public void Submit()
        {
            if (Evaluate())
                OnSubmit?.Invoke(m_Data);
        }

        public virtual void Cancel()
        {
            OnCancel?.Invoke();
        }
    }
}
