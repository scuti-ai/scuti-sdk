using Scuti.UI;
using Scuti.UISystem;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Scuti {
    [DisallowMultipleComponent]
    [Serializable]
    public class View<T> : View {
        protected T m_Data;
        public virtual T Data {
            get { return m_Data; }
            set { m_Data = value; }
        }
    }

    [DisallowMultipleComponent]
    [Serializable]
    public class View : MonoBehaviour {
        public enum State {
            Opening,
            Opened,
            Closing,
            Closed
        }

        public event Action OnDestroyed;

        protected bool _destroyed;

        [BoxGroup("View")] public bool isModal;
        [BoxGroup("View")] [SerializeField] protected State m_State = State.Closed;
        [BoxGroup("View")] public bool requiresLogin;
        [BoxGroup("View")] public bool alwaysEnabled = false;

        //[Header("Only set initial state. DONT change at runtime.")]
        public bool IsOpenOrOpening {
            get { return m_State == State.Opened || m_State == State.Opening; }
        }

        [BoxGroup("View")] public bool displayEvents;
        [ShowIf("displayEvents")] [BoxGroup("View")] public UnityEvent onStartOpening;
        [ShowIf("displayEvents")] [BoxGroup("View")] public UnityEvent onOpened;
        [ShowIf("displayEvents")] [BoxGroup("View")] public UnityEvent onStartClosing;
        [ShowIf("displayEvents")] [BoxGroup("View")] public UnityEvent onClosed;
        public Action<View> OnViewClosed;

        protected bool firstOpen = true;

        protected float inDuration;
        protected float outDuration;

        Coroutine _openingRoutine;
        Coroutine _closingRoutine;

        public Transition[] Transitions;
        // For a selected UI element when opening
        public GameObject firstSelection;

        protected virtual void Awake()
        {
            if(Transitions==null || Transitions.Length<1)
            {
                Transitions = GetComponents<Transition>(); 
            }

            foreach(var t in Transitions)
            {
                inDuration = Mathf.Max(inDuration, t.GetInDuration());
                outDuration = Mathf.Max(outDuration, t.GetOutDuration());
            }
        }

        public void Toggle() {
            if (IsOpenOrOpening)
                Close();
            else
                Open();
        }

        [ContextMenu("Open")]
        public virtual void Open()
        {
            gameObject.SetActive(true);
            firstOpen = false;
            bool wasClosed = m_State == State.Closing || m_State == State.Closed;
            m_State = State.Opening;

            if (wasClosed)
            {
                onStartOpening?.Invoke();
                if (_openingRoutine!=null) StopCoroutine(_openingRoutine);

                // The parent may be inactive, only delay opening if we are active
                if (gameObject.activeInHierarchy) _openingRoutine = StartCoroutine(OpenHelper());
                else SetOpened();
            }

            // Is this possible?
            if(firstSelection != null)
            {
                UIManager.SetFirstSelected(firstSelection);
            }
             
        }

        private IEnumerator OpenHelper()
        {
            yield return new WaitForSeconds(inDuration);
            if (m_State == State.Opening)
            {
                SetOpened();
            }
        }

        [ContextMenu("Close")]
        public virtual void Close() {
            if (_destroyed) return;

            bool wasOpen = m_State == State.Opened || m_State == State.Opening;
                       
            m_State = State.Closing;

            if (wasOpen)
            {
                onStartClosing?.Invoke();
                if (_closingRoutine != null) StopCoroutine(_closingRoutine);
                _closingRoutine = StartCoroutine(CloseHelper());
            } else
            {
                SetClosed(false);
            }
        }

        private IEnumerator CloseHelper()
        {
            yield return new WaitForSeconds(outDuration);
            if(m_State == State.Closing)
            {
                SetClosed(true);
            }
        }

        protected virtual void SetClosed(bool triggerEvent)
        {
            gameObject.SetActive(false || alwaysEnabled);
            m_State = State.Closed;
            if (triggerEvent)
            {
                // Ugly to have 2 but the first is being used in the editor and adding the View property seems to break that. 
                onClosed?.Invoke();
                OnViewClosed?.Invoke(this);
            }
        }

        protected virtual void SetOpened() {
            m_State = State.Opened;
            onOpened?.Invoke();
        }

        protected virtual void OnDestroy() {
            _destroyed = true;
            OnDestroyed?.Invoke();
        }
    }
}