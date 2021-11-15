using UnityEngine;
using UnityEngine.SceneManagement;

public class KillPlane : MonoBehaviour {
	private LevelManager t_LevelManager;

	// Use this for initialization
	void Start () {
		t_LevelManager = FindObjectOfType<LevelManager> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.tag == "Player") {
			PCGEventManager.Instance.onFallDeath?.Invoke();
			t_LevelManager.MarioRespawn ();
		} else {
			Destroy (other.gameObject);
		}
	}
}
