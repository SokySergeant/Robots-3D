using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class characterScript : MonoBehaviour
{
    //movement
    private CharacterController controller;
    public float speed = 6f;
    private float turnTime = 0.05f;
    private float turnVelocity;

    private Transform cam;

    //jump / gravity
    public Transform groundTrans;
    private float groundRadius = 0.1f;
    public LayerMask whatIsGround;
    public float jumpPower = 10f;
    private float gravity = 18f;
    private float velocity = 0f;
    private Vector3 dir = Vector3.zero;

    private bool isInFocus = false;
    private bool canMove = false;
    private bool canShoot = false;
    public bool isDead = false;


    //weapon 
    public enum weapon{
        Gun,
        Grenade
    }
    public weapon[] weapons = new weapon[2];
    private int currentWeapon = 0;
    public GameObject bullet;
    public GameObject grenade;
    public Transform shootPoint;


    //turn limitations
    private int actionAmount = 2;
    private int currentActionAmount;
    public float maxRange = 20f;
    public float currentRange = 0f;


    //hp and knockback
    private float maxHp = 100f;
    public float currentHp;
    private Vector3 impact = Vector3.zero;

    public delegate void OnDeath();
    public static event OnDeath onDeath;


    //hud
    public Canvas canvas;
    public GameObject hpBar;
    public GameObject distanceBar;
    public TextMeshProUGUI actionsText;
    public Image weaponIcon;
    private Image[] weaponIcons;



    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentHp = maxHp;

        cam = GameObject.Find("Main Camera").transform;

        currentActionAmount = actionAmount;
    }



    void Update()
    {
        //movement
        if(canMove){
            //get position before potentially moving
            Vector2 currentPos = new Vector2(transform.position.x, transform.position.z);

            //get movement input
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            dir = new Vector3(h, 0f, v).normalized;

            //movement
            if(dir.magnitude >= 0.1f){
                //turn player with movement
                float newAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                float tempAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, newAngle, ref turnVelocity, turnTime);
                transform.rotation = Quaternion.Euler(0f, tempAngle, 0f);

                //adjust the movement to which way the camera is pointing
                Vector3 moveDir = Quaternion.Euler(0f, newAngle, 0f) * Vector3.forward;
                controller.Move(moveDir.normalized * speed * Time.deltaTime);
            }

            //get position after potentially moving
            Vector2 newPos = new Vector2(transform.position.x, transform.position.z);
            //adding the amount of space moved in this frame to the current range
            currentRange += (newPos - currentPos).magnitude;
            //stop ability to move if you're over the max range
            if(currentRange >= maxRange){
                canMove = false;
            }

            //update distance bar
            distanceBar.GetComponent<Slider>().value = 1 - (currentRange / maxRange);

        }



        //jumping and gravity
        velocity -= gravity * Time.deltaTime;
        //clamp velocity so it doesn't infinitely increase your speed while falling
        velocity = Mathf.Clamp(velocity, -50, Mathf.Infinity);

        if(IsGrounded() && canResetVelocity){
            velocity = -(gravity / 2);
            
            if(canMove && Input.GetButtonDown("Jump")){
                velocity = jumpPower;

                canResetVelocity = false;
                StartCoroutine(VelocityTimer()); //resetting the velocity is set on a timer after jumping to allow jumping on steep slopes without the chance of the character getting caught on the ground (because velocity gets reset on touching ground)
            }
        }
        
        controller.Move(new Vector3(0f, velocity, 0f) * Time.deltaTime); //velocity (and gravity) is applied even when the character isn't in focus incase of knockback



        //changing weapon
        if(canShoot){
            if(Input.GetKeyDown(KeyCode.Alpha1)){
                currentWeapon = 0;
                UpdateWeaponIcons();
            }else if(Input.GetKeyDown(KeyCode.Alpha2)){
                currentWeapon = 1;
                UpdateWeaponIcons();
            }
        }

        //using weapon
        if(canShoot && Input.GetMouseButtonDown(0) && currentActionAmount > 0){ //only a certain amount of actions can be performed in one turn
            //turn character towards the direction the camera is pointing
            transform.rotation = Quaternion.Euler(0f, cam.eulerAngles.y, 0f);

            //use weapon
            switch (weapons[currentWeapon]){
                case weapon.Gun:
                GameObject tempBullet = Instantiate(bullet, shootPoint.position, cam.rotation);
                tempBullet.GetComponent<bulletScript>().shooter = this.gameObject;
                break;

                case weapon.Grenade:
                Instantiate(grenade, shootPoint.position, cam.rotation);
                break;

                default:
                break;
            }

            currentActionAmount--;
            //update action counter ui
            actionsText.text = "Actions left:\n" + currentActionAmount;
        }



        //throw the character
        if(impact.magnitude > 0.1f){
            controller.Move(impact * Time.deltaTime);
            impact = Vector3.Lerp(impact, Vector3.zero, 5 * Time.deltaTime);
        }



    }



    //check if player is standing on ground
    private bool IsGrounded(){
        return Physics.CheckSphere(groundTrans.position, groundRadius, whatIsGround);
    }



    private bool canResetVelocity = true;
    private IEnumerator VelocityTimer(){
        yield return new WaitForSeconds(0.1f);
        canResetVelocity = true;
    }



    public void TakeDamage(float dmg){
        if(!isDead){
            currentHp -= dmg;

            //update hp bar
            if(isInFocus){
                hpBar.GetComponent<Slider>().value = currentHp / maxHp;
            }

            if(currentHp <= 0f){
                SetFocus(false);
                isDead = true;

                //visuals
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z - 90);
                controller.height = 0f;

                //invoke ondeath if it isn't null
                onDeath?.Invoke();
            }
        }
    }



    public void Knockback(Vector3 forceSource, float force){
        //get direction to throw character in (away from source of knockback)
        Vector3 dir = (transform.position - forceSource).normalized;

        impact = dir * force;
        impact = new Vector3(impact.x, impact.y * (force / 2), impact.z); //increase the knockback upwards for effect
    }



    public void SetFocus(bool turnOn){
        if(turnOn){
            //reset amount of actions
            currentActionAmount = actionAmount;
            //reset distance traveled
            currentRange = 0f;

            canMove = true;
            canShoot = true;
            isInFocus = true;

            //set this characters hud info
            //set the hp bar's value to show THIS characters hp
            hpBar.GetComponent<Slider>().value = currentHp / maxHp;
            distanceBar.GetComponent<Slider>().value = 1;
            actionsText.text = "Actions left:\n" + currentActionAmount;

            //create weapon icons
            weaponIcons = new Image[weapons.Length];

            for (int i = 0; i < weapons.Length; i++){
                //set location, name, and assigned button of weapon icon
                weaponIcons[i] = Instantiate(weaponIcon, new Vector3(70 + (i * 100), Screen.height - 70, 0), Quaternion.identity);
                weaponIcons[i].transform.Find("itemName").GetComponent<TextMeshProUGUI>().text = weapons[i].ToString();
                weaponIcons[i].transform.Find("itemNr").GetComponent<TextMeshProUGUI>().text = (i + 1).ToString();
                weaponIcons[i].transform.SetParent(canvas.transform);
            }

            //update weapon icons to show the current weapon
            UpdateWeaponIcons();

        }else{
            canMove = false;
            canShoot = false;
            isInFocus = false;

            //delete all weapon icons if there are any
            if(weaponIcons != null){
                for (int i = 0; i < weaponIcons.Length; i++){
                    if(weaponIcons[i] != null){
                        Destroy(weaponIcons[i].gameObject);
                    }
                }
            }
        }
    }



    public void UpdateWeaponIcons(){
        //turn all icons gray
        for (int i = 0; i < weaponIcons.Length; i++){
            weaponIcons[i].color = Color.gray;
        }

        //turn the icon of the selected weapon white
        weaponIcons[currentWeapon].color = Color.white;
    }



    //hazards
    void OnControllerColliderHit(ControllerColliderHit hit){
        if(hit.gameObject.layer == LayerMask.NameToLayer("Hazard")){
            //instantly kill the character
            TakeDamage(maxHp);
        }
    }



}
