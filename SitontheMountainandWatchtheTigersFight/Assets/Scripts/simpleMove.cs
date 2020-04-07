using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMove : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 move = new Vector3 (
			Input.GetAxis ( "Horizontal" ),
			0.0f,
			Input.GetAxis("Vertical")
		);
		move *= 5;
		transform.position += move * Time.deltaTime;
	}
}
