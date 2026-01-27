using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.VisualScripting;

public class HostGameMenu : MonoBehaviour
{
    public Button OpenHostMenu;
    public Button CloseHostMenu;

    public GameObject HostMenu;
    void Start()
    {
        
    }

    void OpenMenu()
    {
        HostMenu.SetActive(true);
    }

    void CloseMenu()
    {
        HostMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        OpenHostMenu.onClick.AddListener(OpenMenu);
        CloseHostMenu.onClick.AddListener(CloseMenu);

    }
}
