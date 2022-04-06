using UnityEngine;
using PathCreation;

public class Spawner : MonoBehaviour {
    [SerializeField] private PathCreator[] paths;

    public void Spawn (int limit) {
        Arrow spawned = Instantiate (GameController.Instance.Prefab, transform.position, transform.rotation);

        int i = Random.Range (0, Mathf.Min (limit, paths.Length));
        spawned.SetPath (paths[i], GameController.Instance.Color[i]);
    }
}