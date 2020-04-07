using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int Stock { get; set; }
    public int Kills { get; set; }
    public string Name { get; set; }

    private GameMaster gameMaster;

    // Use this for initialization
    void Start()
    {
        gameMaster = GameObject.Find("EventSystem").GetComponent<GameMaster>();

        Kills = 0;
        Stock = gameMaster.stock;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Spawn()
    {
		foreach (Transform cTrans in transform)
		{
			cTrans.localRotation = Quaternion.identity;
			cTrans.localPosition = Vector3.zero;
			cTrans.GetComponent<Rigidbody> ().velocity = Vector3.zero;
		}
		
		
    }
}
