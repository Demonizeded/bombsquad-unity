using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Components;
using Unity.Netcode;

public class PlayerHud : NetworkBehaviour
{
    [SerializeField] private Canvas playerCanvas;
    private Canvas playerHudCanvas;
    public GameObject escphud;
    bool isehshowed = false;

    public override void OnNetworkSpawn()
    {
        //playerHudCanvas = Instantiate(playerCanvas);
        //funkcje!
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)){
            //if (isinsrv)
           //{
                if (!isehshowed)
                {
                    isehshowed = true;
                    escphud.SetActive(true);
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    isehshowed = false;
                    escphud.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                }
           // }
        } 
    }
}
