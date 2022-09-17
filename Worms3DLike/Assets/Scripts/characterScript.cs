using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class characterScript : MonoBehaviour
{
    private CharacterController controller;
    public float speed = 6f;

    private float turnTime = 0.05f;
    private float turnVelocity;

    public Transform cam;


    public Transform groundTrans;
    private float groundRadius = 0.1f;
    public LayerMask whatIsGround;

    public float jumpPower = 10f;
    private float gravity = 18f;
    private float velocity = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }


    void Update()
    {
        //get movement input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 dir = new Vector3(h, 0f, v).normalized;

        //movement
        if(dir.magnitude >= 0.1f){
            //turn player with movement
            float newAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float tempAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, newAngle, ref turnVelocity, turnTime);
            transform.rotation = Quaternion.Euler(0f, tempAngle, 0f);

            //adjust the movement to adhere to which way the camera is pointing
            Vector3 moveDir = Quaternion.Euler(0f, newAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }



        //jumping and gravity
        velocity -= gravity * Time.deltaTime;

        //clamp velocity so it doesn't infinitely increase your speed while falling
        velocity = Mathf.Clamp(velocity, -50, Mathf.Infinity);

        if(IsGrounded() && canResetVelocity){
            velocity = -(gravity / 2);
            
            if(Input.GetButtonDown("Jump")){
                velocity = jumpPower;

                canResetVelocity = false;
                StartCoroutine(velocityTimer()); //resetting the velocity is set on a timer after jumping to allow jumping on slopes without the chance of the character getting caught on the ground
            }
        }
        
        controller.Move(new Vector3(0f, velocity, 0f) * Time.deltaTime);

    }



    //check if player is standing on ground
    private bool IsGrounded(){
        return Physics.CheckSphere(groundTrans.position, groundRadius, whatIsGround);
    }



    private bool canResetVelocity = true;
    private IEnumerator velocityTimer(){
        yield return new WaitForSeconds(0.1f);
        canResetVelocity = true;
    }





}
