using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using TMPro;

public class gameManagerScript : MonoBehaviour
{

    public int amountOfTeams = 2;
    private int currentTeam = 0;
    private int charsPerTeam = 3;

    private float offsetRange = 3f;

    private GameObject[][] teams;
    public Transform[] spawnPoints;
    private GameObject currentChar = null;

    public GameObject character;

    public Camera mainCam;
    public CinemachineFreeLook characterCam;
    public CinemachineVirtualCamera mapCam;

    public Canvas canvas;
    public TextMeshProUGUI teamText;
    public Image characterIcon;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        teams = new GameObject[amountOfTeams][];
        
        //spawn characters
        for (int i = 0; i < amountOfTeams; i++){
            teams[i] = new GameObject[charsPerTeam];

            for (int j = 0; j < charsPerTeam; j++){
                //spawn the character at a random position around the teams spawnpoint
                Vector3 offset = new Vector3(Random.Range(spawnPoints[i].position.x - offsetRange, spawnPoints[i].position.x + offsetRange), spawnPoints[i].position.y, Random.Range(spawnPoints[i].position.z - offsetRange, spawnPoints[i].position.z + offsetRange));
                teams[i][j] = Instantiate(character, offset, Quaternion.identity);
            }
        }

    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F)){
            StartCoroutine(SwitchTeam());
        }

        if(Input.GetKeyDown(KeyCode.G)){
            FocusOnChar(teams[0][1]);
        }
        if(Input.GetKeyDown(KeyCode.H)){
            FocusOnChar(teams[0][2]);
        }
    }



    private IEnumerator SwitchTeam(){

        currentTeam++;
        if(currentTeam > amountOfTeams){
            currentTeam = 1;
        }

        SwitchCamera(1);

        teamText.text = "Team " + (currentTeam + 1) + "'s turn";

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < teams[currentTeam].Length; i++){
            
            Vector3 charPos = mainCam.WorldToScreenPoint(teams[currentTeam][i].transform.position);

            Image tempImg = Instantiate(characterIcon, new Vector3(charPos.x, charPos.y, 0), Quaternion.identity);
            tempImg.GetComponentInChildren<TextMeshProUGUI>().text = (i + 1).ToString();
            tempImg.transform.SetParent(canvas.transform);
        }

    }



    private void SwitchCamera(int camNr){
        if(camNr == 0){
            characterCam.m_Priority = 11;
            mapCam.m_Priority = 10;
        }else{
            characterCam.m_Priority = 10;
            mapCam.m_Priority = 11;
        }
    }



    private void FocusOnChar(GameObject newChar){
        if(currentChar != null){
            currentChar.GetComponent<characterScript>().isInFocus = false;
        }

        currentChar = newChar;
        newChar.GetComponent<characterScript>().isInFocus = true;
        characterCam.m_Follow = newChar.transform;
        characterCam.m_LookAt = newChar.transform;
    }




}
