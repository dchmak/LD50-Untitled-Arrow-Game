using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour {

    [SerializeField] private Button muteButton;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private GameObject onIcon;
    [SerializeField] private GameObject offIcon;
    [SerializeField] private AudioMixerGroup mixerGroup;

    private void Start () {
        muteButton.onClick.AddListener (ToggleMute);
        volumeSlider.onValueChanged.AddListener (SetVolume);

        UpdateDisplay ();
    }

    private void ToggleMute () {
        GameController.Instance.ToggleMute (mixerGroup.name);

        UpdateDisplay ();
    }

    private void SetVolume (float volume) {
        GameController.Instance.SetVolume (mixerGroup.name, volume);

        UpdateDisplay ();
    }

    private void UpdateDisplay () {
        bool muted = GameController.Instance.IsMute (mixerGroup.name);

        onIcon.SetActive (!muted);
        offIcon.SetActive (muted);

        volumeSlider.value = GameController.Instance.GetVolume (mixerGroup.name);
    }
}