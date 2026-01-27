using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TMPro;
using System.Collections;

public class RoomUI : NetworkBehaviour
{
    public TMP_Text roomCodeText;

    private NetworkVariable<FixedString32Bytes> roomCode = new NetworkVariable<FixedString32Bytes>(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );


    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        roomCode.OnValueChanged += OnRoomCodeChanged;
        StartCoroutine(EnterCodeRoom());
    }

    IEnumerator EnterCodeRoom()
    {
        yield return new WaitForSeconds(0.2f);
        if (!string.IsNullOrEmpty(roomCode.Value.ToString()))
        {
            roomCodeText.text = "Room Code: " + roomCode.Value.ToString();
        }
    }

    public override void OnNetworkDespawn()
    {
        roomCode.OnValueChanged -= OnRoomCodeChanged;
    }

    private void OnRoomCodeChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        roomCodeText.text = "Room Code: " + newValue.ToString();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetRoomCodeServerRpc(string code)
    {
        roomCode.Value = code;
    }
}
