using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputRemap : MonoBehaviour {
    [SerializeField] private PlayerInput playerInput = null;
    [SerializeField] private InputActionReference actionRef = null;

    [Header ("References")]
    [SerializeField] private TMP_Text bindingDisplayNameText = null;
    [SerializeField] private Button startRebindButton = null;
    [SerializeField] private Button resetButton = null;
    [SerializeField] private GameObject waitingForInputObject = null;

    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    private const string RebindsKey = "input_rebinds";

    private void Start () {
        startRebindButton.onClick.AddListener (StartRebinding);
        resetButton.onClick.AddListener (Reset);

        string rebinds = PlayerPrefs.GetString (RebindsKey, string.Empty);

        if (!string.IsNullOrEmpty (rebinds)) {
            playerInput.actions.LoadBindingOverridesFromJson (rebinds);
        }
    }

    private void OnEnable () {
        UpdateBindingDisplay ();
    }

    private void OnDisable () {
        rebindingOperation?.Dispose ();
    }

    private void Save () {
        string rebinds = playerInput.actions.SaveBindingOverridesAsJson ();

        PlayerPrefs.SetString (RebindsKey, rebinds);
    }

    private void StartRebinding () {
        startRebindButton.gameObject.SetActive (false);
        waitingForInputObject.SetActive (true);

        playerInput.SwitchCurrentActionMap ("Menu");

        rebindingOperation = actionRef.action.PerformInteractiveRebinding ()
            .WithControlsExcluding ("Mouse")
            .OnMatchWaitForAnother (0.1f)
            .OnComplete (operation => RebindComplete ())
            .Start ();
    }

    private void Reset () {
        int bindingIndex = actionRef.action.GetBindingIndexForControl (actionRef.action.controls[0]);

        if (actionRef.action.bindings[bindingIndex].isComposite) {
            // It's a composite. Remove overrides from part bindings.
            for (int i = bindingIndex + 1; i < actionRef.action.bindings.Count && actionRef.action.bindings[i].isPartOfComposite; ++i) {
                actionRef.action.RemoveBindingOverride (i);
            }
        } else {
            actionRef.action.RemoveBindingOverride (bindingIndex);
        }

        Save ();
        UpdateBindingDisplay ();
    }

    private void RebindComplete () {
        rebindingOperation.Dispose ();
        Save ();
        playerInput.SwitchCurrentActionMap ("Gameplay");

        UpdateBindingDisplay ();
    }

    private void UpdateBindingDisplay () {
        int bindingIndex = actionRef.action.GetBindingIndexForControl (actionRef.action.controls[0]);

        bindingDisplayNameText.text = InputControlPath.ToHumanReadableString (
            actionRef.action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);

        startRebindButton.gameObject.SetActive (true);
        waitingForInputObject.SetActive (false);
    }
}