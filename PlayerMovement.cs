using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	[SerializeField]
	private float speed;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float horizontal = Input.GetAxis ("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        GetComponent<Rigidbody2D>().velocity = new Vector2(horizontal * speed, vertical * speed);
	}
}
