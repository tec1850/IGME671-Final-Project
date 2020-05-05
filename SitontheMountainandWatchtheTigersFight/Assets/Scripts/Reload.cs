using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Reload : MonoBehaviour
{

    private GameMaster gameMaster;
    private float timeLimit;
    private float intensity;
    private PlayerManager player1;
    private PlayerManager player2;

    // Use this for initialization
    void Start()
    {
        gameMaster = GameObject.Find("EventSystem").GetComponent<GameMaster>();
        player1 = GameObject.Find("Player1").GetComponent<PlayerManager>();
        player2 = GameObject.Find("Player2").GetComponent<PlayerManager>();

        timeLimit = gameMaster.TimeLimit;
        intensity = gameMaster.Intensity;
    }

    private void OnTriggerEnter(Collider other)
    {
        intensity += 0.2f;
        gameMaster.Intensity = intensity;

        if (gameMaster.StockOn)
        {
            if (other.tag == "Player1")
            {
                player1.Stock -= 1;
                if (player1.Stock > 0)
                    player1.Spawn();
            }
            else if (other.tag == "Player2")
            {
                player2.Stock -= 1;
                if (player2.Stock > 0)
                    player2.Spawn();
            }
        }
        else if (gameMaster.TimerOn)
        {
            if (other.tag == "Player1")
            {
                player2.Kills += 1;
                if (timeLimit > 0)
                    player1.Spawn();
            }
            else if (other.tag == "Player2")
            {
                player1.Kills += 1;
                if (timeLimit > 0)
                    player2.Spawn();
            }
        }

        gameMaster.UpdateStats();
    }
}
