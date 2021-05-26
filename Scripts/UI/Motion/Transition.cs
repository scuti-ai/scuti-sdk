using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Scuti.UISystem {
    [ExecuteInEditMode]
    public abstract class Transition : MonoBehaviour {
        public enum TransitionOnStart {
            None,
            Out,
            In
        }

        public enum State {
            TransitioningIn,
            TransitioningOut,
            IdleAtIn,
            IdleAtOut,
            Away
        }

        // ================================================
        #region INSPECTOR_UTILITY_BUTTONS
        // ================================================

#if UNITY_EDITOR
        [Button("Transition IN")]
#endif
        public void ClickTransitionIn() {
            TransitionIn();

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);

#endif
        }

#if UNITY_EDITOR
        [Button("Transition OUT")]
#endif
        public void ClickTransitionOut() {
            TransitionOut();
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);

#endif
        }
        #endregion

        // ================================================
        #region MEMBERS
        // ================================================
        // --SERIALIZED--
        [BoxGroup("Transition Base")]
        [SerializeField]
        bool m_ShowFields;

        [BoxGroup("Transition Base")]
        [ShowIf("m_ShowFields")]
        [SerializeField]
        bool m_UseUnscaledTime;

        [BoxGroup("Transition Base")]
        [ShowIf("m_ShowFields")]
        [SerializeField]
        protected Vector2 m_ReferenceScreenResolution = new Vector2(1920, 1080);

        [ReadOnly]
        [BoxGroup("Transition Base")]
        [ShowIf("m_ShowFields")]
        [SerializeField]
        protected State m_State;

        [BoxGroup("Transition Base")]
        [ShowIf("m_ShowFields")]
        [SerializeField]
        TransitionOnStart m_TransitionOnStart = TransitionOnStart.Out;

        [BoxGroup("Transition Base")]
        [ShowIf("m_ShowFields")]
        [SerializeField]
        public bool displayEvents;

        [BoxGroup("Transition Base")]
        [ShowIf("displayEvents")]
        public UnityEvent onStartTransitionIn;

        [BoxGroup("Transition Base")]
        [ShowIf("displayEvents")]
        public UnityEvent onEndTransitionIn;

        [BoxGroup("Transition Base")]
        [ShowIf("displayEvents")]
        public UnityEvent onStartTransitionOut;

        [ShowIf("displayEvents")]
        [BoxGroup("Transition Base")]
        public UnityEvent onEndTransitionOut;

        // --HIDDEN--
        Action m_Callback;
        #endregion

        // ================================================
        #region PUBLIC METHODS
        // ================================================
        public void Toggle(){
            Toggle(null);
        }

        public void Toggle(Action callback = null) {
            if (m_State == State.IdleAtIn || m_State == State.TransitioningIn)
                TransitionOut(callback);
            else if (m_State == State.IdleAtOut || m_State == State.TransitioningOut)
                TransitionIn(callback);
        }

        public void TransitionIn(){
            TransitionIn(null);
        }

        public void TransitionIn(Action callback) {
            m_Callback = callback;
            if (Application.isPlaying) {
                ChangeState(State.TransitioningIn);
                OnStartTransitionIn();
                onStartTransitionIn.Invoke();
            }
            else {
                ChangeState(State.IdleAtIn);
                OnEndTransitionIn();
                onEndTransitionIn.Invoke();
            }
        }

        public void TransitionOut(){
            TransitionOut(null);
        }

        public void TransitionOut(Action callback = null)
        {
            m_Callback = callback;
            if (Application.isPlaying) {
                ChangeState(State.TransitioningOut);
                OnStartTransitionOut();
                onStartTransitionOut.Invoke();
            }
            else {
                ChangeState(State.Away);
                OnEndTransitionOut();
                onEndTransitionOut.Invoke();
            }
        }

        public virtual float GetInDuration()
        {
            return 0;
        }

        public virtual float GetOutDuration()
        {
            return 0;
        }
        #endregion

        // ================================================
        #region LIFECYCLE
        // ================================================
        protected virtual void Awake() {
            if (Application.isPlaying)
                Start_Runtime();
        }

        void Start_Runtime() {
            // transition immediately on start so we don't see flashes of content
            if (m_TransitionOnStart == TransitionOnStart.In)
                OnEndTransitionIn(); 
            else// (m_TransitionOnStart == TransitionOnStart.Out)
                OnEndTransitionOut();
        }

        void LateUpdate() {
            if (Application.isPlaying)
                LateUpdate_Runtime();
        }

        // Executes the transition and till the transition is complete
        void LateUpdate_Runtime() {
            switch (m_State) {
                case State.TransitioningIn:
                    if (TransitionInOverTime()) {
                        ChangeState(State.IdleAtIn);
                        OnEndTransitionIn();
                        m_Callback?.Invoke();
                        onEndTransitionIn.Invoke();
                    }
                    break;
                case State.TransitioningOut:
                    if (TransitionOutOverTime()) {
                        ChangeState(State.Away);
                        OnEndTransitionOut();
                        m_Callback?.Invoke();
                        onEndTransitionOut.Invoke();
                    }
                    break;
            }
        }

        #endregion

        // ================================================
        #region INTERNAL
        // ================================================
        void ChangeState(State state) {
            m_State = state;
        }

        protected float CurrentTime {
            get {
                if (m_UseUnscaledTime) return Time.unscaledTime;
                else return Time.time;
            }
        }
        #endregion

        protected abstract void OnStartTransitionIn();
        protected abstract void OnStartTransitionOut();

        protected abstract void OnEndTransitionOut();
        protected abstract void OnEndTransitionIn();

        protected abstract bool TransitionInOverTime();
        protected abstract bool TransitionOutOverTime();
    }
}
