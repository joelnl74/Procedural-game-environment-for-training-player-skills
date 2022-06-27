using UnityEngine;

public class Coin : MonoBehaviour 
{
	private LevelManager t_LevelManager;

	// Use this for initialization
	void Start () 
	{
		t_LevelManager = FindObjectOfType<LevelManager> ();
	}
	
	void OnTriggerEnter2D(Collider2D other) 
	{
		if (t_LevelManager == null)
        {
			return;
        }

		if (other.gameObject.tag == "Player") 
		{
			PCGEventManager.Instance.onCollectedCoin?.Invoke();
			t_LevelManager.AddCoin ();
			Destroy (gameObject);
		}
	}
}
