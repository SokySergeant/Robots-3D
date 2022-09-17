using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spinObjScript : MonoBehaviour
{
    public float spinSpeed = 1f;

    void Update()
    {
        transform.Rotate(new Vector3(0f, spinSpeed, 0f));
    }
}
