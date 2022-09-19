using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameManagerScript : MonoBehaviour
{

    public int amountOfTeams = 2;
    private int currentTeam = 1;

    public GameObject character;
    public team[] teams;

    public Transform[] spawnPoints;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        teams = new team[amountOfTeams];

        for (int i = 0; i < teams.Length; i++){
            //teams[i].teamNr = i;

        }
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


public class team
{
    public int teamNr;
    
    characterScript[] chars;
}
