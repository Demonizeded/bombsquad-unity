using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ServerListBtn : MonoBehaviour
{
    public GameObject slrawimg;
    public UnityEngine.UI.Button slbtn;
    public GameObject slclosebtngobj;
    public UnityEngine.UI.Button slclosebtn;

    public GameObject slscrollgobj;

    void Start()
    {
        slbtn.onClick.AddListener(ShowServerList);    
        slclosebtn.onClick.AddListener(CloseServerList);
    }

    void ShowServerList()
    {
        slrawimg.SetActive(true);
        slclosebtngobj.SetActive(true);
        slscrollgobj.SetActive(true);
    }

    void CloseServerList()
    {
        slrawimg.SetActive(false);
        slclosebtngobj.SetActive(false);
        slscrollgobj.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
