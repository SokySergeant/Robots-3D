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
    private ParticleSystem explosionParticles;

    public LayerMask whatIsCharacter;


    void Start()
    {
        explosionParticles = GetComponentInChildren<ParticleSystem>();
        explosionParticles.Stop();

        body = GetComponent<Rigidbody>();
        body.AddRelativeForce(Vector3.forward * throwForce);

        StartCoroutine(Blow());
    }



    private IEnumerator Blow(){

        yield return new WaitForSeconds(3f);

        //turn on explosion particles
        explosionParticles.Play();

        //stop rigidbody from moving and rotate it to point straight
        body.isKinematic = true;
        body.transform.rotation = Quaternion.identity;

        //get all characters within radius
        Collider[] characterHits = Physics.OverlapSphere(transform.position, blowRadius, whatIsCharacter);

        //damage and throw characters
        for (int i = 0; i < characterHits.Length; i++){
            characterHits[i].GetComponent<characterScript>().Knockback(transform.position, blowForce);
            characterHits[i].GetComponent<characterScript>().TakeDamage(blowDmg);
        }

        yield return new WaitForSeconds(1f);

        Destroy(gameObject);
    }



}
