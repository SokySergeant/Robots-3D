using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameManagerScript : MonoBehaviour
{

    public int amountOfTeams = 2;
    private int currentTeam = 1;
    private int charsPerTeam = 3;

    private GameObject[][] teams;
    public Transform[] spawnPoints;

    public GameObject character;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        teams = new GameObject[amountOfTeams][];
        

        for (int i = 0; i < amountOfTeams; i++){
            teams[i] = new GameObject[charsPerTeam];

            for (int j = 0; j < charsPerTeam; j++){
                teams[i][j] = Instantiate(character, spawnPoints[i].position, Quaternion.identity);
            }
        }

        Debug.Log(teams);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F)){
            Debug.Log(currentTeam);
            
            currentTeam++;
            if(currentTeam > amountOfTeams){
                currentTeam = 1;
            }
        }
    }




}
