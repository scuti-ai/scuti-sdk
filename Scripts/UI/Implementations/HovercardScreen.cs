// ================================================
// NOTE: Not working right now
// ================================================
 
using UnityEngine;

namespace Scuti { 
    // TODO: Introduce some object pooling for the prefab instances
    // Introduce some more widget management features if requiread.
    public class HovercardScreen : MonoBehaviour {
        [SerializeField] HovercardWidget m_WidgetPrefab;
         
        public HovercardWidget CreateWidget() {
            var instance = Instantiate(m_WidgetPrefab, transform);
            instance.gameObject.hideFlags = HideFlags.DontSave; 
            return instance;
        }
    }
}
