using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class grenadeScript : MonoBehaviour
{
    private Rigidbody body;
    public float throwForce = 300f;

    private float blowRadius = 3f;
    private float blowForce = 10f;
    private float blowDmg = 40f;

    public LayerMask whatIsCharacter;


    void Start()
    {
        body = GetComponent<Rigidbody>();
        body.AddRelativeForce(Vector3.forward * throwForce);

        StartCoroutine(Blow());
    }



    private IEnumerator Blow(){

        yield return new WaitForSeconds(3f);

        //get all characters within radius
        Collider[] characterHits = Physics.OverlapSphere(transform.position, blowRadius, whatIsCharacter);

        //damage and throw characters
        for (int i = 0; i < characterHits.Length; i++){
            characterHits[i].GetComponent<characterScript>().Knockback(transform.position, blowForce);
            characterHits[i].GetComponent<characterScript>().TakeDamage(blowDmg);
        }

        Destroy(gameObject);
    }

}
