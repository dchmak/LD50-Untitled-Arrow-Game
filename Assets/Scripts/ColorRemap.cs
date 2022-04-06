using System;
using UnityEngine;
using UnityEngine.UI;

public class ColorRemap : MonoBehaviour {
    [SerializeField] private Image colorDisplay;
    [SerializeField] private Slider redSlider;
    [SerializeField] private Slider greenSlider;
    [SerializeField] private Slider blueSlider;
    [SerializeField] private Button resetButton;

    private string key;
    private Color defaultColor;
    private Color currentColor;

    public void Init (string key, Color defaultColor) {
        this.key = key;
        this.defaultColor = defaultColor;

        redSlider.onValueChanged.AddListener (SetRedValue);
        greenSlider.onValueChanged.AddListener (SetGreenValue);
        blueSlider.onValueChanged.AddListener (SetBlueValue);

        resetButton.onClick.AddListener (Reset);

        string rebinds = PlayerPrefs.GetString (key, string.Empty);
        if (string.IsNullOrEmpty (rebinds)) {
            currentColor = defaultColor;
        } else {
            currentColor = JsonUtility.FromJson<Color> (rebinds);
        }
    }

    private void OnEnable () {
        UpdateDisplay ();
    }

    public Color GetColor () {
        return currentColor;
    }

    private void Reset () {
        currentColor = defaultColor;

        UpdateDisplay ();
    }

    private void SetRedValue (float r) {
        currentColor.r = r;

        SaveOverride ();
        UpdateDisplay ();

    }

    private void SetGreenValue (float g) {
        currentColor.g = g;

        SaveOverride ();
        UpdateDisplay ();

    }

    private void SetBlueValue (float b) {
        currentColor.b = b;

        SaveOverride ();
        UpdateDisplay ();
    }

    private void UpdateDisplay () {
        colorDisplay.color = GetColor ();

        redSlider.value = currentColor.r;
        greenSlider.value = currentColor.g;
        blueSlider.value = currentColor.b;
    }

    private void SaveOverride () {
        string rebinds = JsonUtility.ToJson (GetColor ());
        PlayerPrefs.SetString (key, rebinds);
    }
}