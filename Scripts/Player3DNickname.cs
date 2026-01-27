using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;



public class Player3DNickname : MonoBehaviour
{
    PlayerData playerData;
    public TMPro.TextMeshPro nickname;
    void Start()
    {

    }

    public void UpdateNickname(string newNick)
    {
        nickname.text = newNick;
    }
}
