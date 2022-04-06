using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {
    private Vector2 direction;

    public void OnUp (InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Performed) {
            direction = Vector2.up;
        }
    }

    public void OnDown (InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Performed) {
            direction = Vector2.down;
        }
    }

    public void OnLeft (InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Performed) {
            direction = Vector2.left;
        }
    }

    public void OnRight (InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Performed) {
            direction = Vector2.right;
        }
    }

    private void Update () {
        transform.right = direction;
    }
}