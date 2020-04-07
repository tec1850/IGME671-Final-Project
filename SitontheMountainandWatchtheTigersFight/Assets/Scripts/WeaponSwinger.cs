using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwinger : MonoBehaviour {
    Quaternion startRot;
    Vector3 startPos;

    public float swingArc;
    // Use this for initialization
    void Start () {
        startRot = transform.localRotation;
        startPos = transform.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
        if (transform.tag == "Player1")
        {
            transform.localRotation = startRot * Quaternion.Euler(0.0f, Input.GetAxis ( "WeaponHorizontal1" ) * swingArc, 0.0f);
            transform.localPosition = startPos + new Vector3 ( Input.GetAxis ( "WeaponHorizontal1" ) * swingArc / 50, 0.0f, Input.GetAxis ( "WeaponVertical1" ) * swingArc / 40 );
        }
        //Input.GetAxis("WeaponHorizontal1") * swingArc
        if (transform.tag == "Player2")
        {
            transform.localRotation = startRot * Quaternion.Euler(0.0f, Input.GetAxis ( "WeaponHorizontal2" ) * swingArc, 0.0f);

            transform.localPosition = startPos + new Vector3 (  Input.GetAxis ( "WeaponHorizontal2" ) * swingArc / 50, 0.0f, Input.GetAxis ( "WeaponVertical2" ) * swingArc / 40);
        }
        //Input.GetAxis("WeaponHorizontal2") * swingArc

    }

    
}
