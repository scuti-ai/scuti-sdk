using UnityEngine;
using UnityEngine.UI;

namespace Scuti.UISystem {
    [ExecuteInEditMode]
    [RequireComponent(typeof(CanvasScaler))]
    public class CanvasScalerUtility : MonoBehaviour {
        CanvasScaler m_Scaler;
        [SerializeField] Vector2 m_ReferenceScreenResolution;

        private void Awake() {
            m_Scaler = GetComponent<CanvasScaler>();
            m_Scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        }

        void Update() {
            m_ReferenceScreenResolution = m_Scaler.referenceResolution;

            var currentResolution = new Resolution() {
                width = ScreenX.Width,
                height = ScreenX.Height
            };

            var referenceRatio = m_ReferenceScreenResolution.x / m_ReferenceScreenResolution.y;
            var currentRatio = (float)currentResolution.width / currentResolution.height;

            if (currentRatio < referenceRatio)
                m_Scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;
            else
                m_Scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        }
    }
}
