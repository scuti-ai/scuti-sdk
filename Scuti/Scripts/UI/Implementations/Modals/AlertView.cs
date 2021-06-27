 
﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Scuti {
    [RequireComponent(typeof(CanvasGroup))]
    public class AlertView : View {
        [Flags]
        public enum ButtonFlags {
            Cross,
            OK
        }

        static Dictionary<string, AlertView> instances = new Dictionary<string, AlertView>();

        [SerializeField] string id = "default";
        [SerializeField] Sprite defaultGraphic;

        [Header("Fields")]
        [SerializeField] Image graphicImage;
        [SerializeField] Text headerText;
        [SerializeField] Text bodyText;
        [SerializeField] Text buttonText;

        [Header("Interaction")]
        [SerializeField] GameObject crossButton;
        [SerializeField] GameObject okButton;

        Action m_Callback;

        // ================================================
        // CONFIGURATION
        // ================================================
        public AlertView Clear() {
            headerText.text = string.Empty;
            bodyText.text = string.Empty;
            buttonText.text = "OK";
            m_Callback = null;
            SetButtonsEnabled(true);
            return this;
        }

        public AlertView SetButtonFlags(ButtonFlags flags = ButtonFlags.Cross | ButtonFlags.OK) {
            crossButton.SetActive(flags.HasFlag(ButtonFlags.Cross));
            okButton.SetActive(flags.HasFlag(ButtonFlags.OK));
            return this;
        }

        public AlertView SetHeader(string header) {
            headerText.text = header;
            return this;
        }

        public AlertView SetBody(string body) {
            bodyText.text = body;
            return this;
        }

        public AlertView SetButtonText(string buttonText) {
            this.buttonText.text = buttonText;
            return this;
        }

        public AlertView SetButtonsEnabled(bool value)
        {
            this.crossButton.gameObject.SetActive(value);
            this.okButton.gameObject.SetActive(value);
            return this;
        }

        // ================================================
        // DISPLAY
        // ================================================
        public Task Show() {
            var source = new TaskCompletionSource<bool>();
            Show(() => source.SetResult(true));
            return source.Task;
        }

        public async void Show(Action callback) {
            m_Callback = callback;
            await SanitizeFields();
            Open();
        }

        // ================================================
        // INTERNAL
        // ================================================
        public static AlertView Get(string requestedId = "default")
        {
            if (instances.ContainsKey(requestedId))
                throw new Exception($"No AlertBox instance found with ID {requestedId}");
            else
                return instances[requestedId];
        }

        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        void Init() {
            if (!instances.ContainsKey(id))
                instances.Add(id, this);
            else
                instances[id] = this;
            okButton.GetComponent<Button>().onClick.AddListener(OnClick);
            crossButton.GetComponent<Button>().onClick.AddListener(Close);
        }

        void OnClick() {
            m_Callback?.Invoke();
            Close();
        }

        public override void Close()
        {
            Clear();
            base.Close();
        }

        async Task SanitizeFields() {
            // Ensure a graphic
            if (graphicImage.sprite == null)
                graphicImage.sprite = defaultGraphic;

            // Ensure a header
            if (headerText.text.IsNullOrEmpty())
                headerText.text = "MESSAGE";

            // Ensure positive text
            if (buttonText.text.IsNullOrEmpty())
                buttonText.text = "OK";

            await Task.WhenAll(
                bodyText.gameObject.Refresh(),
                headerText.gameObject.Refresh()
            );

            await buttonText.gameObject.Refresh();
        }
    }
} 
