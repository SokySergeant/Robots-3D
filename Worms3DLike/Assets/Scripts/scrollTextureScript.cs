using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scrollTextureScript : MonoBehaviour
{
    public float scrollX = 0.01f;
    public float scrollZ = 0.01f;

    private Renderer rend;

    void Start(){
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        float offsetX = Time.time * scrollX;
        float offsetZ = Time.time * scrollZ;
        rend.material.mainTextureOffset = new Vector2(offsetX, offsetZ);
    }
}
