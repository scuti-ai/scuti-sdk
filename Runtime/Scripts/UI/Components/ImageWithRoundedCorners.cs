using System;
using UnityEngine;
using UnityEngine.UI;

// Credit to NObinator : https://github.com/Nobinator/Unity-UI-Rounded-Corners/

namespace Scuti.UI
{
    [ExecuteInEditMode]
    public class ImageWithRoundedCorners : MonoBehaviour
    {
        private static readonly int Props = Shader.PropertyToID("_WidthHeightRadius");

        public Image image;
        public float radius;

        private void Awake()
        {
#if UNITY_EDITOR
            // duplicate it to prevent changes from saving and causing git conflicts
            image.material = new Material(image.material);
#endif
        }

        void OnRectTransformDimensionsChange()
        {
            Refresh();
        }

        private void OnValidate()
        {
            Refresh();
        }

        private void Refresh()
        {
            var rect = ((RectTransform)transform).rect;

            image?.material.SetVector(Props, new Vector4(rect.width, rect.height, radius, 0));
        }
    }
}
