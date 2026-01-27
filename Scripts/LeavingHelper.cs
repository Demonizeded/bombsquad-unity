using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LeavingHelper : MonoBehaviour
{
    public static LeavingHelper instance;
    public GameObject MainMenu;
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
