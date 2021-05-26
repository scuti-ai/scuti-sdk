using System;
using UnityEngine;
using UnityEngine.UI;

public static class DataBindingExtensions {
    // TEXT
    // Text from String
    public static void BindTextTo(this Text uiText, StringBinding binding) {
        binding.OnValueChanged += value => uiText.text = value;
    }

    public static void BindTextTo(this Text uiText, StringBinding binding, Func<string, string> generator) {
        binding.OnValueChanged += value => uiText.text = generator(value);
    }

    // Text from Float
    public static void BindTextTo(this Text uiText, FloatBinding binding) {
        binding.OnValueChanged += value => uiText.text = value.ToString();
    }

    public static void BindTextTo(this Text uiText, FloatBinding binding, Func<float, string> generator) {
        binding.OnValueChanged += value => uiText.text = generator(value);
    }

    // Text from Int
    public static void BindTextTo(this Text uiText, IntBinding binding) {
        binding.OnValueChanged += value => uiText.text = value.ToString();
    }

    public static void BindTextTo(this Text uiText, IntBinding binding, Func<int, string> generator) {
        binding.OnValueChanged += value => uiText.text = generator(value);
    }

    // Text from Bool
    public static void BindTextTo(this Text uiText, BoolBinding binding) {
        binding.OnValueChanged += value => uiText.text = value.ToString();
    }

    public static void BindTextTo(this Text uiText, BoolBinding binding, Func<bool, string> generator) {
        binding.OnValueChanged += value => uiText.text = generator(value);
    }

    // GAMEOBJECT
    public static void BindSetActiveTo(this GameObject go, BoolBinding binding) {
        binding.OnValueChanged += value => go.SetActive(value);
    }

    // IMAGE
    // Sprite from Object
    public static void BindSpriteTo(this Image image, ObjectBinding binding) {
        if (binding.Value == null) return;
        binding.OnValueChanged += value => image.sprite = binding.Value as Sprite;
    }

    // CUSTOM
    // RATING STARTS WIDGET
    public static void BindValueTo(this Scuti.UI.RatingStarsWidget widget, FloatBinding binding) {
        binding.OnValueChanged += value => widget.Value = value;
    }
}