using System.Collections;
using System.Collections.Generic;
using PathCreation;
using UnityEngine;

public class Arrow : MonoBehaviour {
    [SerializeField] private SpriteRenderer sprite;

    private Rigidbody2D rigidbody2;
    private PathCreator pathCreator;
    private float timePassed;

    public void SetPath (PathCreator pathCreator, Color color) {
        this.pathCreator = pathCreator;
        sprite.color = color;
    }

    public void Destroy () {
        Destroy (gameObject);
    }

    private void Start () {
        rigidbody2 = GetComponent<Rigidbody2D> ();
        timePassed = 0;
    }

    private void FixedUpdate () {
        if (pathCreator != null) {
            timePassed += Time.fixedDeltaTime / GameController.Instance.ArrowFlyDuration;
            rigidbody2.MovePosition (pathCreator.path.GetPointAtTime (timePassed, EndOfPathInstruction.Stop));
            rigidbody2.SetRotation (pathCreator.path.GetRotation (timePassed, EndOfPathInstruction.Stop));
        }
    }

    private void OnTriggerEnter2D (Collider2D other) {
        if (other.GetComponent<Player> ()) {
            GameController.Instance.Gameover ();
        } else if (other.GetComponent<Block> ()) {
            GameController.Instance.AddScore ();
        } else {
            return;
        }

        Destroy ();
    }
}