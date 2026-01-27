using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using TMPro;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.IO.LowLevel.Unsafe;

public class RelayManager : MonoBehaviour
{

    public TMP_InputField roomcodeinput;
    public Button HostGameBtn;
    public Button JoinGameBtn;
    public GameObject menucanvas;
    public TMP_InputField PlayerUsername;
    public TMP_InputField ServerNameIPTF;
    public Toggle IsPrivateToggle;

    public Transform LobbyList;
    public GameObject LobbyBtnPrefab;


    //public GameObject PlayerListComp;
    public static string ServerName;
    public static string LocalPlayerUsername;

    private Lobby CurrentLobby;
    private CancellationTokenSource heartbeatCts;




    async void Start()
    {
        HostGameBtn.onClick.AddListener(HostGame);
        JoinGameBtn.onClick.AddListener(() => JoinGame(roomcodeinput.text));
        await InitializeServices();
        InvokeRepeating(nameof(RefreshLobbyList), 2f, 5f);

    }
    async Task InitializeServices()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }


    async void HostGame()
    {

        try
        {
          
            if (!string.IsNullOrEmpty(ServerNameIPTF.text))
            {
                ServerName = ServerNameIPTF.text;
            }
            else
            {
                ServerName = "Server" + UnityEngine.Random.Range(100, 1000);
            }


            
            if (!string.IsNullOrEmpty(PlayerUsername.text))
            {
                LocalPlayerUsername = PlayerUsername.text;
            }
            else
            {
                LocalPlayerUsername = "Player" + UnityEngine.Random.Range(100, 1000);
            }

            PlayerPrefs.SetString("PlayerName", LocalPlayerUsername);

         

            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(20);
            string joincode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.SetHostRelayData(
                alloc.RelayServer.IpV4,
                (ushort)alloc.RelayServer.Port,
                alloc.AllocationIdBytes,
                alloc.Key,
                alloc.ConnectionData
                );


            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            UpdatePlayersCountList();
            var options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = new Unity.Services.Lobbies.Models.Player(
                    AuthenticationService.Instance.PlayerId,
                    data: new Dictionary<string, PlayerDataObject>
                    {
                    { "name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, LocalPlayerUsername) }
                    }),
                Data = new Dictionary<string, DataObject>
                {
                    { "joinCode", new DataObject(DataObject.VisibilityOptions.Public, joincode) },
                    { "hostName", new DataObject(DataObject.VisibilityOptions.Public, LocalPlayerUsername) },
                    { "serverName", new DataObject(DataObject.VisibilityOptions.Public, ServerName)},
                    { "isprivatesrv", new DataObject(DataObject.VisibilityOptions.Public, IsPrivateToggle.isOn.ToString())},
                    { "playerCount", new DataObject(DataObject.VisibilityOptions.Public, "1/20") }
                }
            };

            CurrentLobby = await Lobbies.Instance.CreateLobbyAsync(ServerName, 20, options);
            UpdatePlayersCountList();
            if(heartbeatCts != null){
                heartbeatCts.Cancel();
            }

            heartbeatCts = new CancellationTokenSource();
            _ = SendingHeartBeat(heartbeatCts.Token); //discarduje wynik bo kompilator narzeka


            RoomUI roomUI = FindObjectOfType<RoomUI>();
            if (roomUI != null && NetworkManager.Singleton.IsHost) roomUI.SetRoomCodeServerRpc(joincode);
            

            if (menucanvas != null) menucanvas.gameObject.SetActive(false);

        }catch(Exception ex)
        {
            Debug.LogException(ex);
        }

    }

   

    async void JoinGame(string joinCode)
    {
        try
        {
            if (!string.IsNullOrEmpty(PlayerUsername.text))
            {
                LocalPlayerUsername = PlayerUsername.text;
            }
            else
            {
                LocalPlayerUsername = "Player" + UnityEngine.Random.Range(100, 1000);
            }


            PlayerPrefs.SetString("PlayerName", LocalPlayerUsername);

           
            JoinAllocation joinalloc = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetClientRelayData(
                joinalloc.RelayServer.IpV4,
                (ushort)joinalloc.RelayServer.Port,
                joinalloc.AllocationIdBytes,
                joinalloc.Key,
                joinalloc.ConnectionData,
                joinalloc.HostConnectionData
                );


            NetworkManager.Singleton.StartClient();


            if (menucanvas != null) menucanvas.gameObject.SetActive(false);
        }
        catch (Exception e) { 
            Debug.LogException(e);
        }

    }

    private async Task SendingHeartBeat(CancellationToken token)
    {

        while (!token.IsCancellationRequested && CurrentLobby != null)
        {
            try
            {
                await Lobbies.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            await Task.Delay(15000, token);

        }

    }

    async Task RefreshLobbyList()
    {
        try
        {
            if (LobbyList == null || LobbyBtnPrefab == null) return;

            var queryopt = new QueryLobbiesOptions
            {
                Count = 25,
                Order = new List<QueryOrder>
            {
                new QueryOrder(true, QueryOrder.FieldOptions.MaxPlayers)
            }
            };

            var resp = await Lobbies.Instance.QueryLobbiesAsync(queryopt);

            for (int i = LobbyList.childCount - 1; i >= 0; i--)
            {
                Destroy(LobbyList.GetChild(i).gameObject);
            }
            /*

        { "joinCode", new DataObject(DataObject.VisibilityOptions.Public, joincode) },
                  { "hostName", new DataObject(DataObject.VisibilityOptions.Public, LocalPlayerUsername) },
                  { "serverName", new DataObject(DataObject.VisibilityOptions.Public, ServerName)},
                  { "isprivatesrv", new DataObject(DataObject.VisibilityOptions.Public, IsPrivateToggle.isOn.ToString())},
                  { "playerCount", new DataObject(DataObject.VisibilityOptions.Public, "1/20") }

            */

            foreach (var srv in resp.Results)
            {
                if (!srv.Data.ContainsKey("joinCode")) continue;

                string joincode = srv.Data["joinCode"].Value;
                string hostname = srv.Data.ContainsKey("hostName") ? srv.Data["hostName"].Value : "UnknownPlayer";
                string servname = srv.Data.ContainsKey("serverName") ? srv.Data["serverName"].Value : "UnknownServerName";
                string plrcount = srv.Data.ContainsKey("playerCount") ? srv.Data["playerCount"].Value : "?/20";
                string ispriv = srv.Data["isprivatesrv"].Value;


                if (ispriv == "True") continue;

                var createdSrvBtn = Instantiate(LobbyBtnPrefab, LobbyList);
                var DispSrvData = createdSrvBtn.GetComponentInChildren<TMP_Text>();


                DispSrvData.text = $"Host:{hostname} Servername: {servname} Joincode: {joincode} PlayerCount: {plrcount}  ";

                var srvbtn = createdSrvBtn.GetComponentInChildren<Button>();
                if (srvbtn != null) srvbtn.onClick.AddListener(() => JoinGame(joincode));



            }
        }
        catch (Exception ex) {
            Debug.LogWarning("Blad Odswiezania Listy " + ex.Message); 
        }

       

     
     


    }



    async void UpdatePlayersCountList()
    {
        if (CurrentLobby == null || !NetworkManager.Singleton.IsHost)
        {
            return;
        }
        int plrcount = NetworkManager.Singleton.ConnectedClients.Count;

        var data = new Dictionary<string, DataObject> {

            { "playerCount", new DataObject(DataObject.VisibilityOptions.Public, plrcount.ToString() + "/20") }
        };
        try
        {
            await Lobbies.Instance.UpdateLobbyAsync(CurrentLobby.Id, new UpdateLobbyOptions { Data = data });
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Nie udalo sie zaaktualizowac liczby graczt" + ex.Message);
        }


    }

    void OnClientDisconnected(ulong clientid)
    {
        if (NetworkManager.Singleton.IsHost) UpdatePlayersCountList();

    }

    void OnClientConnected(ulong clientid)
    {
        if (NetworkManager.Singleton.IsHost) UpdatePlayersCountList();
    }




    private async void OnApplicationQuit()
    {
        try
        {
            heartbeatCts?.Cancel();
            if (CurrentLobby != null)
                await Lobbies.Instance.DeleteLobbyAsync(CurrentLobby.Id);
        }
        catch { }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
