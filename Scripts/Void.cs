using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Void : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        collision.gameObject.transform.position = new Vector3(0f, 5f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
