using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using EZCameraShake;
using TMPro;

public class GameController : MonoBehaviour {
    private enum State {
        Menu,
        InProgress,
        Gameover,
        Setting
    }

    [System.Serializable]
    private struct Difficulty {
        public uint scoreThreshold;
        public float spawnTimeGap;
        public float arrowFlyDuration;
        public int allowedColorCount;
    }

    [System.Serializable]
    private struct Audio {
        public string reference;
        public AudioClip clip;
        public AudioMixerGroup mixerGroup;
        public bool loop;
        public bool autoplay;
    }

    public static GameController Instance;

    private const string RebindsKey = "color_rebinds_";
    private const string VolumeKey = "volume_";
    private const string CamShakeKey = "camera_shake_";
    private const float MinVolume = -60;

    [FoldoutGroup ("Player")][SerializeField] private GameObject player;
    [FoldoutGroup ("Player")][SerializeField] private ParticleSystem particle;

    [FoldoutGroup ("Arrow")][SerializeField] private Spawner[] arrowSpawners;
    [FoldoutGroup ("Arrow")][SerializeField] private Arrow prefab;
    [FoldoutGroup ("Arrow")][SerializeField] private Color[] colors;

    [FoldoutGroup ("Difficulty")][SerializeField] private Difficulty[] difficulties;

    [FoldoutGroup ("Audio")][SerializeField] private Audio[] audios;

    [FoldoutGroup ("UI")][SerializeField] private GameObject menuPanel;
    [FoldoutGroup ("UI")][SerializeField] private Button menuStartButton;
    [FoldoutGroup ("UI")][SerializeField] private Button menuSettingsButton;
    [FoldoutGroup ("UI")][SerializeField] private GameObject gameoverPanel;
    [FoldoutGroup ("UI")][SerializeField] private TMP_Text gameoverScoreText;
    [FoldoutGroup ("UI")][SerializeField] private Button gameoverRestartButton;
    [FoldoutGroup ("UI")][SerializeField] private Button gameoverMenuButton;
    [FoldoutGroup ("UI")][SerializeField] private GameObject settingsPanel;
    [FoldoutGroup ("UI")][SerializeField] private Button settingsReturnButton;
    [FoldoutGroup ("UI")][SerializeField] private ColorRemap settingsColorRemapPrefab;
    [FoldoutGroup ("UI")][SerializeField] private Transform settingsColorRemapParent;
    [FoldoutGroup ("UI")][SerializeField] private Toggle settingsCamShakeToggle;

    private State state;
    private float timer;
    private ColorRemap[] colorRemaps;
#if UNITY_EDITOR
    [ReadOnly][FoldoutGroup ("Debug")][SerializeField]
#endif 
    private uint score;

    private Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource> ();
    private Dictionary<string, AudioMixerGroup> mixerGroups = new Dictionary<string, AudioMixerGroup> ();
    private float volumeBeforeMute = 0;

    public float ArrowFlyDuration { get => GetCurrectDifficulty ().arrowFlyDuration; }
    public Arrow Prefab { get => prefab; }
    public Color[] Color {
        get {
            Color[] overriddenColors = colors;
            for (int i = 0; i < colors.Length; i++) {
                overriddenColors[i] = colorRemaps[i].GetColor ();
            }

            return overriddenColors;
        }
    }

#if UNITY_EDITOR
    [ReadOnly][FoldoutGroup ("Debug")][SerializeField] private float debugSpawnTimeGap;
    [ReadOnly][FoldoutGroup ("Debug")][SerializeField] private float debugArrowFlyDuration;
#endif

    public void AddScore () {
        Play ("Blocked");

        if (PlayerPrefs.GetInt (CamShakeKey, 1) != 0) {
            CameraShaker.Instance.ShakeOnce (0.1f, 0.5f, 0.2f, 0.3f);
        }

        particle.Play ();

        score++;
    }

    public void Menu () {
        state = State.Menu;

        player.SetActive (false);

        menuPanel.SetActive (true);
        gameoverPanel.SetActive (false);
        settingsPanel.SetActive (false);
    }

    public void StartGame () {
        score = 0;

        state = State.InProgress;

        player.SetActive (true);

        menuPanel.SetActive (false);
        gameoverPanel.SetActive (false);
        settingsPanel.SetActive (false);
    }

    public void Setting () {
        state = State.Setting;

        player.SetActive (false);

        menuPanel.SetActive (false);
        gameoverPanel.SetActive (false);
        settingsPanel.SetActive (true);
    }

    public void Gameover () {
        Play ("Gameover");

        gameoverScoreText.text = $"You survived {score} arrows.";

        state = State.Gameover;

        player.SetActive (false);

        menuPanel.SetActive (false);
        gameoverPanel.SetActive (true);
        settingsPanel.SetActive (false);

        Arrow[] remainingArrows = FindObjectsOfType<Arrow> ();
        foreach (Arrow arrow in remainingArrows) {
            arrow.Destroy ();
        };
    }

    private void Awake () {
        if (Instance == null) {
            Instance = this;

            foreach (Audio audio in audios) {
                AudioSource audioSource = gameObject.AddComponent<AudioSource> ();
                audioSource.clip = audio.clip;
                audioSource.outputAudioMixerGroup = audio.mixerGroup;
                audioSource.loop = audio.loop;

                mixerGroups[audio.mixerGroup.name] = audio.mixerGroup;

                if (audio.autoplay) {
                    audioSource.Play ();
                }

                audioSources.Add (audio.reference, audioSource);
            }
        } else {
            Destroy (gameObject);
        }
    }

    private void Start () {
        menuStartButton.onClick.AddListener (StartGame);
        menuSettingsButton.onClick.AddListener (Setting);
        gameoverRestartButton.onClick.AddListener (StartGame);
        gameoverMenuButton.onClick.AddListener (Menu);
        settingsReturnButton.onClick.AddListener (Menu);
        settingsCamShakeToggle.onValueChanged.AddListener (CamShakeToggle);

        settingsCamShakeToggle.isOn = PlayerPrefs.GetInt (CamShakeKey, 1) != 0;

        colorRemaps = new ColorRemap[colors.Length];
        for (int i = 0; i < colors.Length; i++) {
            colorRemaps[i] = Instantiate (settingsColorRemapPrefab, settingsColorRemapParent);
            colorRemaps[i].Init (RebindsKey + i, colors[i]);
        }

        Menu ();

        if (PlayerPrefs.HasKey (VolumeKey + "BGM")) {
            SetVolume ("BGM", PlayerPrefs.GetFloat (VolumeKey + "BGM", 0));
        }
        if (PlayerPrefs.HasKey (VolumeKey + "SFX")) {
            SetVolume ("SFX", PlayerPrefs.GetFloat (VolumeKey + "SFX", 0));
        }
        PlayBGM ();
    }

    private void CamShakeToggle (bool camShake) {
        PlayerPrefs.SetInt (CamShakeKey, camShake ? 1 : 0);
    }

    private void Update () {
        if (state == State.InProgress) {
            timer -= Time.deltaTime;

            if (timer < 0) {
                Difficulty difficulty = GetCurrectDifficulty ();

                timer = difficulty.spawnTimeGap;
                int limit = difficulty.allowedColorCount;

#if UNITY_EDITOR
                debugSpawnTimeGap = timer;
#endif

                Spawner spawner = arrowSpawners[Random.Range (0, arrowSpawners.Length)];
                spawner.Spawn (limit);

            }
        }

#if UNITY_EDITOR
        debugArrowFlyDuration = ArrowFlyDuration;
#endif
    }

    private Difficulty GetCurrectDifficulty () {
        for (int i = 0; i < difficulties.Length; i++) {
            if (score >= difficulties[i].scoreThreshold) {
                return difficulties[i];
            }
        }

        return default;
    }

#if UNITY_EDITOR
    [Button ("Reset Player Prefs")]
    private void ResetPlayerPrefs () {
        PlayerPrefs.DeleteAll ();
        Debug.Log ("Player Prefs Deleted");
    }
#endif

    private void PlayBGM () {
        string[] bgmReferences = { "BGM_1", "BGM_2" };
        string selectedReference = bgmReferences[Random.Range (0, bgmReferences.Length)];

        Play (selectedReference, onComplete : PlayBGM);
    }

    #region Audio
    public void Play (string reference, float delay = 0, float pitch = 1, System.Action onComplete = null) {
        StartCoroutine (PlayCoroutine (reference, delay, pitch, onComplete));
    }

    private IEnumerator PlayCoroutine (string reference, float delay = 0, float pitch = 1, System.Action onComplete = null) {
        if (IsPlaying (reference)) {
            yield break;
        }

        if (audioSources.TryGetValue (reference, out AudioSource sourceToPlay)) {
            sourceToPlay.pitch = pitch;
            sourceToPlay.PlayDelayed (delay);

            yield return new WaitForSeconds (sourceToPlay.clip.length);

            onComplete?.Invoke ();
        } else {
            Debug.LogWarning (reference + " does not exist! Skip playing.");
        }
    }

    public bool IsPlaying (string reference) {
        if (audioSources.TryGetValue (reference, out AudioSource sourceToPlay)) {
            return sourceToPlay.isPlaying;
        }

        return false;
    }

    public bool ToggleMute (string mixerGroupName) {
        if (!mixerGroups.ContainsKey (mixerGroupName)) {
            Debug.LogWarning ("Mixer Group " + mixerGroupName + " does not exist");
            return false;
        }

        float currentVolume = GetVolume (mixerGroupName);
        bool muted = currentVolume <= MinVolume;
        SetVolume (mixerGroupName, muted ? volumeBeforeMute : MinVolume);
        volumeBeforeMute = currentVolume;

        return IsMute (mixerGroupName);
    }

    public bool IsMute (string mixerGroupName) {
        return GetVolume (mixerGroupName) <= MinVolume;
    }

    public float GetVolume (string mixerGroupName) {
        mixerGroups[mixerGroupName].audioMixer.GetFloat (mixerGroupName + "Volume", out float currentVolume);
        return currentVolume;
    }

    public void SetVolume (string mixerGroupName, float volume) {
        // TODO: have listener so that every visual updates when one set volume
        mixerGroups[mixerGroupName].audioMixer.SetFloat (mixerGroupName + "Volume", volume);

        PlayerPrefs.SetFloat (VolumeKey + mixerGroupName, volume);
    }
    #endregion
}