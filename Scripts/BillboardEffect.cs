using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using TMPro;

public class BillboardEffect : NetworkBehaviour
{
    Camera playercam;
    public TextMeshPro textgobj;
    void Start()
    {
        playercam = Camera.main;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        textgobj.gameObject.transform.LookAt(playercam.transform);
        textgobj.gameObject.transform.Rotate(0,180,0);
        if (IsOwner) textgobj.gameObject.SetActive(false);

    }
}
