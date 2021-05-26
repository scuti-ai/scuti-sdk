using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Scuti.UI {


    [Serializable]
    public class FloatUnityEvent : UnityEvent<float> { }

    public class Timer : MonoBehaviour {
        public enum State {
            Ready,
            Running,
            Paused,
            Finished
        }

        public enum Mode {
            CountUp,
            CountDown
        }

        public bool showEvents;
        [ShowIf("showEvents")] public UnityEvent onFinished;


        [ShowIf("showEvents")] public FloatUnityEvent onTick;
        public Action<string> onCustomEvent;

        [Serializable]
        public class CustomTimeEvent
        {
            public string ID;
            public float Expiration;
        }

        public Mode mode;
        public bool useUnscaledTime;

        [ShowNativeProperty] public float Duration { get; private set; }
        [ReadOnly] [SerializeField] State m_State = State.Paused;

        private List<CustomTimeEvent> CustomEvents = new List<CustomTimeEvent>();

        float m_TimeLeft;

        public void ResetTime(float duration) {
            Duration = duration;
            m_TimeLeft = duration;
            m_State = State.Ready;
            CustomEvents.Clear();
        }

        public void SoftReset()
        {
            m_TimeLeft = Duration;
        }

        public void AddCustomEvent(string id, float duration)
        {
            CustomEvents.Add(new CustomTimeEvent() { Expiration = duration, ID = id });
        }

        public void Begin() {
            m_State = State.Running;
        }

        public void Pause() {
            m_State = State.Paused;
        }

        void Update() {
            if (m_State == State.Running) {
                m_TimeLeft -= DeltaTime;

                for(var i = 0; i<CustomEvents.Count; i++)
                {
                    var evt = CustomEvents[i];
                    evt.Expiration -= DeltaTime;
                    if(evt.Expiration<=0)
                    {
                        onCustomEvent?.Invoke(evt.ID);

                        CustomEvents.RemoveAt(i);
                        i--;
                    }
                }

                if (m_TimeLeft <= 0) {
                    if (mode == Mode.CountDown)
                        onTick.Invoke(0);
                    else
                        onTick.Invoke(1);

                    m_TimeLeft = 0;
                    m_State = State.Finished;
                    onFinished.Invoke();
                }
                else {
                    if (mode == Mode.CountDown)
                        onTick.Invoke(m_TimeLeft / Duration);
                    else
                        onTick.Invoke(1 - m_TimeLeft / Duration);
                }
            }
        }

        float DeltaTime {
            get {
                if (useUnscaledTime) return Time.unscaledDeltaTime;
                else return Time.deltaTime;
            }
        }
    }
}
