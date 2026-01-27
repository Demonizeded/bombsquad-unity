using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.UI;
using TMPro;
using Unity.Collections;

public class PlayerData : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> PlayerNickname =
        new NetworkVariable<FixedString64Bytes>("Unknown",
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    private TMP_InputField NicknameText;

    public override void OnNetworkSpawn()
    {
        PlayerNickname.OnValueChanged += OnNicknameChange;

        if (IsOwner)
        {
            StartCoroutine(WaitForUiToLoad());
        }
        else
        {
            // od razu ustaw nick na starcie, jeœli ju¿ jest
            OnNicknameChange("", PlayerNickname.Value);
        }
    }

    private IEnumerator WaitForUiToLoad()
    {
        yield return new WaitForSeconds(0.2f);
        string savedname = PlayerPrefs.GetString("PlayerName", "");
        SetNick3dServerRpc(savedname);
    }

    private void OnNicknameChange(FixedString64Bytes oldValue, FixedString64Bytes newValue)
    {
        var nick3d = GetComponentInChildren<Player3DNickname>();
        if (nick3d != null)
        {
            nick3d.UpdateNickname(newValue.ToString());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetNick3dServerRpc(string savedname)
    {
        if (string.IsNullOrEmpty(savedname))
        {
            savedname = "Player" + Random.Range(1, 1001);
        }
        PlayerNickname.Value = savedname;
    }
}
