using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class WeeblWobble : MonoBehaviour {

    public Vector3 center;
    public float radius;
    public Transform target;

    private Rigidbody thisRB;
    public Vector3 input;
    public Vector3 swingInput;

    public int isGrounded = 0;

    public float nonNewtonianSwingPower;

    /*
    public AudioSource audioSource;
    public AudioClip slap;
    */

    // Use this for initialization
    void Start ()
    {
        thisRB = GetComponent<Rigidbody> ();
    }

    void Update ()
    {
        //take a generic input vector and rotate it so that it faces the arbitrary target of the weebl
		//also determines which player is being controlled via a tag
		if (transform.tag == "Player1") {
			
			input = Quaternion.LookRotation (transform.position - target.position, Vector3.up) *
			new Vector3 (
				-Input.GetAxis ("Horizontal1"),
				0.0f,
				-Input.GetAxis ("Vertical1")
            );
            /*swingInput = Quaternion.LookRotation ( transform.position - target.position, Vector3.up ) *
                new Vector3 (
                    -Input.GetAxis ( "WeaponHorizontal1" ) * 2,
                    0.0f,
                    0.0f
                );*/
			if ((Input.GetKeyDown ( KeyCode.Space ) || Input.GetKeyDown ("joystick 1 button 0")) && isGrounded > 0)
            {
				//handles jumping
                thisRB.AddForce ( transform.up * 800 );
            }
        } else if (transform.tag == "Player2") {
			input = Quaternion.LookRotation (transform.position - target.position, Vector3.up) *
			new Vector3 (
				-Input.GetAxis ("Horizontal2"),
				0.0f,
				-Input.GetAxis ("Vertical2")
			);
            /*swingInput = Quaternion.LookRotation(transform.position - target.position, Vector3.up) *
            new Vector3(
                -Input.GetAxis("WeaponHorizontal2") * 2,
                0.0f,
                0.0f
            );*/
			if ((Input.GetKeyDown ( KeyCode.Keypad0 ) || Input.GetKeyDown ("joystick 2 button 0")) && isGrounded > 0)
            {
				//handles jumping
                thisRB.AddForce ( transform.up * 800 );
            }
        }

    }

    // Update is called once per frame
    void FixedUpdate ()
    {

        //wobble center debugging
        Vector3 currentUp = transform.up * radius;
        Debug.DrawRay ( transform.position, currentUp, Color.black );
        Debug.DrawRay ( transform.position, center * radius, Color.black);

        //calculate adjustment to weeble center based on input
        Vector3 inputForce =
            ((center.normalized + input) * radius) - (currentUp);
        thisRB.AddForceAtPosition (inputForce, currentUp + transform.position );
        Debug.DrawRay ( currentUp + transform.position, inputForce, Color.blue );

        //construct a force to orient the weeble to their arbitrary target on the xz plane
        Vector3 orientingForce = (((target.position - transform.position).normalized * radius) - (transform.forward * radius)) * .8f;
        orientingForce = orientingForce - (Vector3.Project(orientingForce, (target.position - transform.position).normalized ));
        orientingForce = new Vector3 ( orientingForce.x, 0.0f, orientingForce.z );
        //orientingForce += swingInput;
        thisRB.AddForceAtPosition ( orientingForce, ((transform.forward * radius) + transform.position) );

        //let's turn this shit into a porcupine
        Debug.DrawRay ( ((transform.forward * radius) + transform.position), orientingForce, Color.red);
        Debug.DrawRay ( transform.position, transform.forward * radius, Color.black );
        Debug.DrawRay ( transform.position, (target.position - transform.position).normalized * radius, Color.black );

    }

    private void OnCollisionEnter ( Collision collision )
    {
        if (collision.collider.tag == "Ground")
        {
            isGrounded++;
        }
        if ( collision.collider.tag == "Player1" || collision.collider.tag == "Player2" )
        {
            collision.collider.GetComponent<Rigidbody> ().AddForce ( collision.impulse * nonNewtonianSwingPower );
            //audioSource.clip = slap;
            //audioSource.Play(0);
            //Debug.Log ( "hit the player!" );
        }
    }
    private void OnCollisionExit ( Collision collision )
    {
        if (collision.collider.tag == "Ground")
        {
            isGrounded--;
        }
    }

}
