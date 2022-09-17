using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameManagerScript : MonoBehaviour
{

    public int amountOfTeams = 2;
    private int currentTeam = 1;


    void Start()
    {
        
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
