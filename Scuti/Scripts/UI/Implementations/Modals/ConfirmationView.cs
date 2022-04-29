 
﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Scuti {
    public class ConfirmationView : View {
        [Flags]
        public enum ButtonFlags {
            Cross,
            Positive,
            Negative
        }

        static Dictionary<string, ConfirmationView> instances = new Dictionary<string, ConfirmationView>();

        [SerializeField] string id = "default";
        [SerializeField] Sprite defaultGraphic;

        [Header("Fields")]
        [SerializeField] Image graphic;
        [SerializeField] Text headerText;
        [SerializeField] Text bodyText;
        [SerializeField] Text negativeText;
        [SerializeField] Text positiveText;

        [Header("Interaction")]
        [SerializeField] GameObject negativeButton;
        [SerializeField] GameObject positiveButton;
        [SerializeField] GameObject crossButton;

        Action<bool> m_Callback;
        ButtonFlags m_ButtonFlags;

        // ================================================
        // CONFIGURATION
        // ================================================
        public ConfirmationView Clear() {
            graphic.sprite = null;
            headerText.text = string.Empty;
            bodyText.text = string.Empty;
            if (negativeText) negativeText.text = string.Empty;
            if (positiveButton) positiveText.text = string.Empty;
            return this;
        }

        public ConfirmationView SetGraphic(Sprite sprite) {
            graphic.sprite = sprite;
            return this;
        }

        public ConfirmationView SetButtonFlags(ButtonFlags flags = ButtonFlags.Cross | ButtonFlags.Negative | ButtonFlags.Positive) {
            m_ButtonFlags = flags;

            crossButton.SetActive(flags.HasFlag(ButtonFlags.Cross));
            if (positiveButton) negativeButton.SetActive(flags.HasFlag(ButtonFlags.Negative));
            if(positiveButton) positiveButton.SetActive(flags.HasFlag(ButtonFlags.Positive));
            return this;
        }

        public ConfirmationView SetHeader(string header) {
            headerText.text = header;
            return this;
        }

        public ConfirmationView SetBody(string body) {
            bodyText.text = body;
            return this;
        }

        public ConfirmationView SetPositive(string positive) {
            positiveText.text = positive;
            return this;
        }

        public ConfirmationView SetNegative(string negative) {
            negativeText.text = negative;
            return this;
        }

        // ================================================
        // DISPLAY
        // ================================================
        public Task<bool> Show() {
            var source = new TaskCompletionSource<bool>();
            Show(response => source.SetResult(response));
            return source.Task;
        }

        public async void Show(Action<bool> callback) {
            m_Callback = callback;
            await SanitizeFields();
            Open();
        }

        // ================================================
        // INTERNAL
        // ================================================
        public static ConfirmationView Get(string requestedId = "default")
        {
            if (instances.ContainsKey(requestedId))
                throw new Exception($"No ConfirmationBox instance found with ID {requestedId}");
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
            onClosed.AddListener(() => m_Callback = null);
            if (negativeButton) negativeButton.GetComponent<Button>().onClick.AddListener(OnClickNegative);
            if (positiveButton) positiveButton.GetComponent<Button>().onClick.AddListener(OnClickPositive);
            if (crossButton) crossButton.GetComponent<Button>().onClick.AddListener(OnClickClose); //before OnClickNegative
        }

        void OnClickNegative() {
            GetComponent<CanvasGroup>().Hide();
            Clear();
            Close();
            m_Callback?.Invoke(false);
        }

        void OnClickPositive() {
            GetComponent<CanvasGroup>().Hide();
            Clear();
            Close();
            m_Callback?.Invoke(true);
        }

        void OnClickClose()
        {
            GetComponent<CanvasGroup>().Hide();
            Close();
            m_Callback?.Invoke(false);
        }



        async Task SanitizeFields() {
            // Ensure a graphic
            if (graphic.sprite == null)
                graphic.sprite = defaultGraphic;

            // Ensure a header
            if (headerText && headerText.text.IsNullOrEmpty())
                headerText.text = "Confirmation required...";

            // Ensure a positive text. There should ALWAYS be a positive text
            if (positiveText && positiveText.text.IsNullOrEmpty())
                positiveText.text = "Yes";

            // Ensure a negative text if the flags contain negative button and no text has been set
            if (negativeText && negativeText.text.IsNullOrEmpty() && m_ButtonFlags.HasFlag(ButtonFlags.Negative))
                negativeText.text = "No";

            await Task.WhenAll(
                bodyText.gameObject.Refresh(),
                headerText.gameObject.Refresh()
            );

            await Task.WhenAll(
                negativeButton?.Refresh(),
                positiveButton?.Refresh()
            );

            await negativeButton?.transform.parent.gameObject.Refresh();
        }
    }
} 
