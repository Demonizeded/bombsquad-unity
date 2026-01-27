using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallProtection : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.transform.position.y < -3 || gameObject.transform.position.x > 110 || gameObject.transform.position.x < -110 || gameObject.transform.position.z > 110 || gameObject.transform.position.z < -110)
        {
            gameObject.transform.position = new Vector3(0,5,0);  
        }
    }
}
