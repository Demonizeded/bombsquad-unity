using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    public GameObject loadingscreen;
    void Start()
    {
        loadingscreen.SetActive(true);
        StartCoroutine(waitandturnoff());
        

    }

    IEnumerator waitandturnoff()
    {
        yield return new WaitForSeconds(2);
        loadingscreen.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
