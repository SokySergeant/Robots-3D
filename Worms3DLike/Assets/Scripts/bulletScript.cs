using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletScript : MonoBehaviour
{
    public float speed = 10f;
    public GameObject shooter;

    private Vector3 startPos;
    private float range = 50f;

    private int dmg = 20;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
    }


    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        //destroy the bullet if it gets far enough without hitting anything, i.e. by shooting upwards
        if((transform.position - startPos).magnitude > range){
            Destroy(gameObject);
        }
    }


    void OnTriggerEnter(Collider other){
        if(other.gameObject != shooter){ //make sure not to damage the character that shot the bullet
            if(other.gameObject.layer == LayerMask.NameToLayer("Character")){
                other.GetComponent<characterScript>().TakeDamage(dmg);
            }

            Destroy(gameObject);
        }
    }
}
