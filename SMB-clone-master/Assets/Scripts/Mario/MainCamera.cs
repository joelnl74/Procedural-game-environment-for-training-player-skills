using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour {
	public GameObject target;
	public float followAhead = 2.6f;
	public float smoothing = 5;
	public bool canMove;
	public bool canMoveBackward = false;

	private float cameraWidth;
	private Vector3 targetPosition;

	// Use this for initialization
	void Start () {
		Mario mario = FindObjectOfType<Mario>();
		target = mario.gameObject;

		GameObject boundary = GameObject.Find ("Level Boundary");
		float aspectRatio = GetComponent<MainCameraAspectRatio> ().targetAspects.x /
		                    GetComponent<MainCameraAspectRatio> ().targetAspects.y;
		cameraWidth = Camera.main.orthographicSize * aspectRatio;

		// Initialize camera's position
		Vector3 spawnPosition = FindObjectOfType<LevelManager>().FindSpawnPosition();
		targetPosition = new Vector3 (spawnPosition.x, transform.position.y, transform.position.z);


		transform.position = new Vector3(targetPosition.x + followAhead, targetPosition.y, targetPosition.z);
		canMove = true;
	}


	// Update is called once per frame
	void Update () 
	{
		targetPosition = new Vector3(target.transform.position.x, transform.position.y, -10);
		targetPosition = new Vector3(targetPosition.x + followAhead, targetPosition.y, -10);
		transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);
	}
}