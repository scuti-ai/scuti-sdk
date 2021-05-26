 
﻿// ================================================
// NOTE: Not working right now
// ================================================

using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Scuti {
    [RequireComponent(typeof(View))]
    public class HovercardWidget : MonoBehaviour {
        public UnityEvent onClick;

        [Serializable]
        public class ViewModel {
            public Sprite sprite;
            public string headerText;
            [TextArea(3, 20)]
            public string bodyText;
        }

        [SerializeField] Image m_HeaderIcon;
        [SerializeField] Text m_HeaderText;
        [SerializeField] Text m_BodyText;

        RectTransform RT;
        View view;

        void Awake() {
            RT = GetComponent<RectTransform>();
            view = GetComponent<View>();
        }

        // ================================================
        // CONFIGURATION
        // ================================================
        public HovercardWidget SetData(ViewModel vm) {
            SetIcon(vm.sprite);
            SetHeaderText(vm.headerText);
            SetBodyText(vm.bodyText);
            return this;
        }

        public HovercardWidget SetIcon(Sprite sprite) {
            m_HeaderIcon.sprite = sprite;
            return this;
        }

        public HovercardWidget SetHeaderText(string text) {
            m_HeaderText.text = text;
            return this;
        }

        public HovercardWidget SetBodyText(string text) {
            m_BodyText.text = text;
            return this;
        }

        // ================================================
        // DISPLAY
        // ================================================
        public void Show() {
            view.Open();
        }

        public void Hide() {
            view.Close();
        }

        public Task Refresh() {
            var source = new TaskCompletionSource<bool>();
            Refresh(() => source.SetResult(true));
            return source.Task;
        }

        async public void Refresh(Action callback) {
            // First we refresh the text elements
            await Task.WhenAll(
                m_HeaderText.gameObject.Refresh(),
                m_BodyText.gameObject.Refresh()
            );
            // Then we refresh the entire object to recalculate the layout
            await gameObject.Refresh();
            callback?.Invoke();
        }

        // ================================================
        // POSITIONING
        // ================================================
        public void SetPosition(Vector2 targetPos) {
            targetPos = ScreenX.VectorToResolution(targetPos);

            if (TryTopLeft(targetPos)) return;
            if (TryBottomLeft(targetPos)) return;
            if (TryBottomRight(targetPos)) return;
            if (TryTopRight(targetPos)) return;
        }

        #region ORIENTATION
        bool TryTopLeft(Vector2 pos) {
            var resolution = Screen.currentResolution;

            //if (pos.x + Width > resolution.width || pos.y - Height < 0)
            if (pos.x + Width > ScreenX.Width || pos.y - Height < 0)
                return false;

            var displacement = new Vector2(Width / 2, -Height / 2);
            RT.position = ScreenX.VectorToWindow(pos + displacement);
            return true;
        }

        bool TryBottomLeft(Vector2 pos) {
            var resolution = Screen.currentResolution;

            //if (pos.x + Width > resolution.width || pos.y + Height > resolution.height)
            if (pos.x + Width > ScreenX.Width || pos.y + Height > resolution.height)
                return false;

            var displacement = new Vector2(Width / 2, Height / 2);
            RT.position = ScreenX.VectorToWindow(pos + displacement);
            return true;
        }

        bool TryBottomRight(Vector2 pos) {
            var resolution = Screen.currentResolution;

            //if (pos.x - Width < 0 || pos.y + Height > resolution.height)
            if (pos.x - Width < 0 || pos.y + Height > ScreenX.Height)
                return false;

            var displacement = new Vector2(-Width / 2, Height / 2);
            RT.position = ScreenX.VectorToWindow(pos + displacement);
            return true;
        }

        bool TryTopRight(Vector2 pos) {
            if (pos.x - Width < 0 || pos.y - Height < 0)
                return false;

            var displacement = new Vector2(-Width / 2, -Height / 2);
            RT.position = ScreenX.VectorToWindow(pos + displacement);
            return true;
        }
        #endregion

        // ================================================
        // MISC
        // ================================================
        public float Width {
            get { return GetComponent<RectTransform>().sizeDelta.x; }
        }

        public float Height {
            get { return GetComponent<RectTransform>().sizeDelta.y; }
        }

        public void Click() {
            onClick.Invoke();
        }
    }
} 