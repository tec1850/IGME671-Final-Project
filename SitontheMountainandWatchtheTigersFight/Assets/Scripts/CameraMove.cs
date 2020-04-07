using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour {

    public GameObject target1;
    public GameObject target2;
    public GameObject focus;

    private float distance;
    public float multiplier;
    public float angle;
	public float heightPow;
	public float min;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        UpdateFocus();
        UpdateCamera();
	}

    //Updates the position of the camera's focus
    void UpdateFocus()
    {
        Vector3 newPos = (target1.transform.position + target2.transform.position) / 2.0f;
		focus.transform.position = Vector3.Lerp (focus.transform.position, newPos, Time.deltaTime);
    }

    void UpdateCamera()
    {
		Vector3 targetPos;
        distance = (target1.transform.position - target2.transform.position).magnitude / 2.0f;
		distance = Mathf.Clamp (distance, min, 100f);
		targetPos = focus.transform.position + (-focus.transform.forward * distance * Mathf.Pow(multiplier, 2.0f)) +
			(Mathf.Tan(angle) * distance * Mathf.Pow(multiplier, heightPow) * Vector3.up);

		transform.position = Vector3.Lerp (transform.position, targetPos, Time.deltaTime);

        transform.LookAt(focus.transform.position);
    }
}
