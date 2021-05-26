using System;

namespace Scuti {
    public partial class Presenter {
        [Serializable]
        public abstract class Model {
            /// <summary>
            /// Event fired by the <see cref="Model"/> object.
            /// Can be used to alert listeners for changes in the state
            /// </summary>
            public event Action<string, object> OnEvent;

            /// <summary>
            /// Invokes the <see cref="OnEvent"/> event. Can be used to
            /// notify listeners for changes in the state 
            /// </summary>
            public void InvokeEvent(string notification = "") {
                OnEvent?.Invoke(notification, null);
            }

            /// <summary>
            /// Invokes the <see cref="OnEvent"/> event along with a payload. 
            /// Can be used to notify listeners for changes in the state 
            /// </summary>
            public void InvokeEvent(string notification, object payload = null) {
                OnEvent?.Invoke(notification, payload);
            }

            /// <summary>
            /// Override this to handle dispose logic
            /// </summary>
            public virtual void Dispose() { }
        }
    }
}