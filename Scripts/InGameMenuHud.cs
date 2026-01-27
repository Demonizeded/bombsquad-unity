using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InGameMenuHud : MonoBehaviour
{
    public GameObject escphud;
    public Button Leavebtn;
    public GameObject MainMenu;
    void Start()
    {
        MainMenu = LeavingHelper.instance.MainMenu;
        Leavebtn.onClick.AddListener(LeaveGame);
        
    }

    void LeaveGame()
    {
        escphud.SetActive(false);
        NetworkManager.Singleton.Shutdown();
        MainMenu.SetActive(true);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
