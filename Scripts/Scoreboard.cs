using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class Scoreboard : MonoBehaviour
{
    public Transform ScrollList;
    public GameObject PlayerStasPrefab;

    Dictionary<ulong, GameObject> addedplrs = new Dictionary<ulong, GameObject>();

    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;

        // na starcie odœwie¿ listê
        Invoke(nameof(RefreshPlayers), 1f);
    }

    void RefreshPlayers()
    {
        foreach (var obj in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            if (obj.TryGetComponent<PlayerHealth>(out var plrhp) &&
                obj.TryGetComponent<PlayerData>(out var plrnick))
            {
                AddPlayerstoScrb(obj.OwnerClientId, plrhp, plrnick);
            }
        }
    }

    void OnClientDisconnect(ulong clientid)
    {
        if (addedplrs.TryGetValue(clientid, out var plrcreatedrow))
        {
            Destroy(plrcreatedrow);
            addedplrs.Remove(clientid);
        }
    }

    void OnClientConnect(ulong clientId)
    {
        // znajdŸ obiekt gracza po clientId
        foreach (var obj in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            if (obj.OwnerClientId == clientId &&
                obj.TryGetComponent<PlayerHealth>(out var plrhp) &&
                obj.TryGetComponent<PlayerData>(out var plrnick))
            {
                AddPlayerstoScrb(clientId, plrhp, plrnick);
                break;
            }
        }
    }

    void AddPlayerstoScrb(ulong clientid, PlayerHealth plrhp, PlayerData plrnick)
    {
        if (addedplrs.ContainsKey(clientid)) return;

        var plrcreated = Instantiate(PlayerStasPrefab, ScrollList);
        var texts = plrcreated.GetComponentsInChildren<TMP_Text>();

        texts[0].text = plrnick.PlayerNickname.Value.ToString();
        texts[1].text = plrhp.kills.Value.ToString();

        plrhp.kills.OnValueChanged += (oldValue, newValue) => texts[1].text = newValue.ToString();
        plrnick.PlayerNickname.OnValueChanged += (oldValue, newValue) => texts[0].text = newValue.ToString();

        addedplrs[clientid] = plrcreated;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }
}
