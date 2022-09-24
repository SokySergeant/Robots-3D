using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class characterScript : MonoBehaviour
{

    private CharacterController controller;
    public float speed = 6f;
    private float turnTime = 0.05f;
    private float turnVelocity;

    private Transform cam;

    public Transform groundTrans;
    private float groundRadius = 0.1f;
    public LayerMask whatIsGround;
    public float jumpPower = 10f;
    private float gravity = 18f;
    private float velocity = 0f;

    private Vector3 dir = Vector3.zero;
    public bool isInFocus = false;
    public bool isDead = false;



    public enum weapon{
        Gun,
        Grenade
    }
    public weapon[] weapons = new weapon[2];
    private int currentWeapon = 0;
    public GameObject bullet;
    public GameObject grenade;
    public Transform shootPoint;

    private int maxHp = 100;
    public int currentHp;
    private Vector3 impact = Vector3.zero;

    public delegate void OnDeath();
    public static event OnDeath onDeath;



    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentHp = maxHp;

        cam = GameObject.Find("Main Camera").transform;
    }



    void Update()
    {
        if(isInFocus){
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
        }





        //jumping and gravity
        velocity -= gravity * Time.deltaTime;
        //clamp velocity so it doesn't infinitely increase your speed while falling
        velocity = Mathf.Clamp(velocity, -50, Mathf.Infinity);

        if(IsGrounded() && canResetVelocity){
            velocity = -(gravity / 2);
            
            if(isInFocus && Input.GetButtonDown("Jump")){
                velocity = jumpPower;

                canResetVelocity = false;
                StartCoroutine(VelocityTimer()); //resetting the velocity is set on a timer after jumping to allow jumping on steep slopes without the chance of the character getting caught on the ground (because velocity gets reset on touching ground)
            }
        }
        
        controller.Move(new Vector3(0f, velocity, 0f) * Time.deltaTime); //velocity (and gravity) is applied even when the character isn't in focus incase of knockback



        //changing weapon
        if(isInFocus){
            if(Input.GetKeyDown(KeyCode.Alpha1)){
                currentWeapon = 0;
            }else if(Input.GetKeyDown(KeyCode.Alpha2)){
                currentWeapon = 1;
            }
        }

        //using weapon
        if(isInFocus && Input.GetMouseButtonDown(0)){
            //turn character towards the direction the camera is pointing
            float newAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            transform.rotation = Quaternion.Euler(0f, newAngle, 0f);

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



    public void TakeDamage(int dmg){
        if(!isDead){
            currentHp -= dmg;
            if(currentHp <= 0){
                isInFocus = false;
                isDead = true;

                //visuals
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z - 90);
                controller.height = 0f;

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







}
