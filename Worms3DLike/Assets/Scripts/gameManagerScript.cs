using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using TMPro;

public class gameManagerScript : MonoBehaviour
{
    //teams
    public int amountOfTeams = 2;
    private int currentTeam = 0;
    private int charsPerTeam = 3;

    private float offsetRange = 3f;

    private GameObject[][] teams;
    public Transform[] spawnPoints;

    //character
    private GameObject currentChar = null;
    public GameObject character;

    //cameras
    public Camera mainCam;
    public CinemachineFreeLook characterCam;
    public CinemachineVirtualCamera mapCam;

    //UI 
    public Canvas canvas;
    public GameObject mainMenuObj;
    public TextMeshProUGUI teamText;
    public TextMeshProUGUI errorText;
    public GameObject backToMenuBtn;
    public TextMeshProUGUI controlsText;
    public GameObject hpBar;
    public GameObject distanceBar;
    public TextMeshProUGUI actionsText;

    //character icons
    public Image characterIcon;
    private Image[] icons;
    public Sprite iconImg;
    public Sprite chosenIconImg;
    private Color[] teamColors;

    private bool doCharChoosing = false;
    private int currentChosenCharIndex;

    private bool canSwitchTeam = true;
    



    void Start()
    {
        
    }



    void Update()
    {
        //switching team
        if(Input.GetKeyDown(KeyCode.F) && canSwitchTeam && !doCharChoosing){
            canSwitchTeam = false;
            SwitchTeam();
        }



        //choosing character
        if(doCharChoosing){
            //cycle through characters
            if(Input.GetKeyDown(KeyCode.LeftArrow)){
                icons[currentChosenCharIndex].sprite = iconImg;

                currentChosenCharIndex--;
                if(currentChosenCharIndex < 0){
                    currentChosenCharIndex = charsPerTeam - 1;
                }

                icons[currentChosenCharIndex].sprite = chosenIconImg;
            }

            if(Input.GetKeyDown(KeyCode.RightArrow)){
                icons[currentChosenCharIndex].sprite = iconImg;

                currentChosenCharIndex++;
                if(currentChosenCharIndex >= charsPerTeam){
                    currentChosenCharIndex = 0;
                }

                icons[currentChosenCharIndex].sprite = chosenIconImg;
            }

            //confirming character chosen, if the character isn't dead
            if(Input.GetKeyDown(KeyCode.F) && !teams[currentTeam][currentChosenCharIndex].GetComponent<characterScript>().isDead){
                //switch focus to chosen character
                FocusOnChar(teams[currentTeam][currentChosenCharIndex]);
                SwitchCamera(0);
                
                //delete character icons
                for (int i = 0; i < icons.Length; i++){
                    Destroy(icons[i].gameObject);
                }

                //turn on character hud
                hpBar.SetActive(true);
                distanceBar.SetActive(true);

                //change controls info
                controlsText.text = "F: Finish Turn";

                doCharChoosing = false;
            }
        }
    }



    public void SwitchTeam(){
        StartCoroutine(SwitchTeamRoutine());
    }

    private IEnumerator SwitchTeamRoutine(){

        currentTeam++;
        if(currentTeam >= amountOfTeams){
            currentTeam = 0;
        }

        //turn off character hud
        hpBar.SetActive(false);
        distanceBar.SetActive(false);
        actionsText.text = "";

        //stop current player movement
        FocusOnChar(null);
        //switch to map camera
        SwitchCamera(1);

        //change controls info
        controlsText.text = "← / →: Cycle\nF: Confirm";


        int deadChars = 0;

        //count up dead characters
        for (int i = 0; i < teams[currentTeam].Length; i++){
            if(teams[currentTeam][i].GetComponent<characterScript>().isDead){
                deadChars++;
            }
        }

        //if not all characters on this team are dead
        if(!(deadChars >= charsPerTeam)){

            //set team info text and color
            teamText.text = "Team " + (currentTeam + 1) + "'s turn"; //shift team numbers up by one as to not have a Team 0
            teamText.color = teamColors[currentTeam];

            yield return new WaitForSeconds(1f);

            //spawn icons for the team's characters
            icons = new Image[charsPerTeam];

            for (int i = 0; i < teams[currentTeam].Length; i++){
                //get position of characters on screen
                Vector3 charPos = mainCam.WorldToScreenPoint(teams[currentTeam][i].transform.position);
                //create icons
                icons[i] = Instantiate(characterIcon, new Vector3(charPos.x, charPos.y, 0), Quaternion.identity);

                //set icon color to teams color
                icons[i].color = teamColors[currentTeam];

                //set icon's text to the characters number if they're not dead, otherwise it is X
                if(teams[currentTeam][i].GetComponent<characterScript>().isDead){
                    icons[i].GetComponentInChildren<TextMeshProUGUI>().text = "X";
                }else{
                    icons[i].GetComponentInChildren<TextMeshProUGUI>().text = (i + 1).ToString();
                }

                //make the icon a child of the canvas since it is a UI element
                icons[i].transform.SetParent(canvas.transform);
            }

            //set the first icon in focus
            currentChosenCharIndex = 0;
            icons[currentChosenCharIndex].sprite = chosenIconImg;

            doCharChoosing = true;

        }else{ //if all the characters on this team are dead, skip this team
            SwitchTeam();
        }


        canSwitchTeam = true;
    }



    //0: character Camera
    //1: map Camera
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
            currentChar.GetComponent<characterScript>().SetFocus(false);
        }

        if(newChar != null){
            currentChar = newChar;
        
            newChar.GetComponent<characterScript>().SetFocus(true);

            //move camera to new character
            characterCam.m_Follow = newChar.transform;
            characterCam.m_LookAt = newChar.transform;
        }
    }



    public void StartGame(){

        //get team amount entered, check if it is a number, and is between 2-4. If everything is ok, start the game
        int tempAmountOfTeams;
        string input = mainMenuObj.GetComponentInChildren<TMP_InputField>().text;

        if(int.TryParse(input, out tempAmountOfTeams)){
            if(tempAmountOfTeams >= 2 && tempAmountOfTeams <= 4){
                //start game 

                amountOfTeams = tempAmountOfTeams;

                Cursor.lockState = CursorLockMode.Locked;

                teams = new GameObject[amountOfTeams][];

                //create random colors for each team
                teamColors = new Color[amountOfTeams];
                for (int i = 0; i < teamColors.Length; i++){
                    //create random bright colors
                    teamColors[i] = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);
                }
                
                //spawn characters
                for (int i = 0; i < amountOfTeams; i++){
                    teams[i] = new GameObject[charsPerTeam];

                    for (int j = 0; j < charsPerTeam; j++){
                        //spawn the character at a random position around the teams spawnpoint
                        Vector3 offset = new Vector3(Random.Range(spawnPoints[i].position.x - offsetRange, spawnPoints[i].position.x + offsetRange), spawnPoints[i].position.y, Random.Range(spawnPoints[i].position.z - offsetRange, spawnPoints[i].position.z + offsetRange));
                        teams[i][j] = Instantiate(character, offset, Quaternion.identity);

                        //give character hud elements
                        teams[i][j].GetComponent<characterScript>().canvas = this.canvas;
                        teams[i][j].GetComponent<characterScript>().hpBar = this.hpBar;
                        teams[i][j].GetComponent<characterScript>().distanceBar = this.distanceBar;
                        teams[i][j].GetComponent<characterScript>().actionsText = this.actionsText;
                    }
                }

                //remove main menu options
                mainMenuObj.SetActive(false);

                SwitchTeam();

                //check if there is only one team left standing whenever a character dies
                characterScript.onDeath += CheckIfWin;

                //exit function
                return;
            }
        }

        //if the inputted value isn't a number, or isn't between 2-4, give error message
        errorText.text = "Please input a team amount between 2-4.";
    }



    public void EndGame(){
        //delete old characters
        for (int i = 0; i < teams.Length; i++){
            for (int j = 0; j < teams[i].Length; j++){
                Destroy(teams[i][j]);
            }
        }

        currentTeam = 0;
        teamText.text = "";

        //show main menu options
        backToMenuBtn.SetActive(false);
        mainMenuObj.SetActive(true);
        //reset error message
        errorText.text = "";
    }



    private void CheckIfWin(){

        int liveTeams = 0;

        for (int i = 0; i < teams.Length; i++){

            int liveChars = 0;
            //check if any of this teams characters are alive
            for (int j = 0; j < teams[i].Length; j++){
                if(!(teams[i][j].GetComponent<characterScript>().isDead)){
                    liveChars++;
                }
            }

            //if at least one character is alive, the team is live
            if(liveChars > 0){
                liveTeams++;
            }

        }

        //if there is one or less teams live, win
        if(liveTeams <= 1){

            int liveTeamInd = -1;

            //find the last living team
            for (int i = 0; i < teams.Length; i++){
                for (int j = 0; j < teams[i].Length; j++){
                    if(!(teams[i][j].GetComponent<characterScript>().isDead)){ //the last living team must have at least one live character
                        liveTeamInd = i;
                        break;
                    }
                }
            }

            canSwitchTeam = false;
            SwitchCamera(1);

            //stop current character
            currentChar.GetComponent<characterScript>().SetFocus(false);

            //if the live team index stays at -1, it means all the teams are dead, meaning its a tie
            if(liveTeamInd == -1){
                //set the text's color to white if no team won
                teamText.color = Color.white;
                teamText.text = "Tie!";
            }else{
                //set the text's color to the team that won
                teamText.color = teamColors[liveTeamInd];
                teamText.text = "Team " + (liveTeamInd + 1) + " won!";
            }

            //turn off character hud elements
            hpBar.SetActive(false);
            distanceBar.SetActive(false);
            //empty control info
            controlsText.text = "";
            actionsText.text = "";

            //show back to menu button
            Cursor.lockState = CursorLockMode.None;
            backToMenuBtn.SetActive(true);
            
        }


        
    }



}
