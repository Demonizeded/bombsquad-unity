using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System;

public class PlayerHealth : NetworkBehaviour
{
 
    public int maxHealth = 100;
    public Vector3 respawnPos = new Vector3(0, 5, 0);
    int spawnnumber = 0;
    Vector3[] TabSpawns = new Vector3[]
    {
        new Vector3(0f,2f,0f),
        new Vector3(45f,2f,-5f),
        new Vector3(82f,4f,-10f),
        new Vector3(34f,5f,-80f)
    };
    public bool isVamp = false;
    public Camera playerCamera;
    public Camera spectatorCameraPrefab;
    private Camera spectatorCameraInstance;
    public Canvas playerhud;
    bool SpawnProt = false;
    private Rigidbody rb;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isAlive = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> grenadeType = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> kills = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    //public int kills = 0;

    [Header("Elemtnety UI")]
    public GameObject pdz;
    public GameObject sto;
    public GameObject pdzsto;
    public GameObject dwisc;
    public GameObject pdzdwisc;



    private void Awake() => rb = GetComponent<Rigidbody>();

   
    private void Update()
    {
        if (currentHealth.Value >= 50) pdz.SetActive(true);
        else pdz.SetActive(false);

        if (currentHealth.Value >= 100) sto.SetActive(true);
        else sto.SetActive(false);
        
        if(currentHealth.Value>=150) pdzsto.SetActive(true);
        else pdzsto.SetActive(false);

        if(currentHealth.Value>=200) dwisc.SetActive(true);
        else dwisc.SetActive(false);

        if (currentHealth.Value >= 250) pdzdwisc.SetActive(true);
        else pdzdwisc.SetActive(false);

    }

    [ServerRpc(RequireOwnership =false)]
    public void ClassSelectionServerRpc(int idc)
    {
        switch(idc)
        {
            case 1://TANK
            currentHealth.Value = 200;
            break;
            case 2://VAMP
                isVamp = true;
                if (!SpawnProt)
                {
                    currentHealth.Value = 50;
                }
            break;
            
        }
    }

    void SpawnProtection(bool IsProtected)
    {
        if (IsProtected)
        {
            currentHealth.Value = -1;
        }
        else
        {
            if (!isVamp)
            {
                currentHealth.Value = maxHealth;
            }
            else
            {
                currentHealth.Value = 50;
            }
        }
    }

    private void Start()
    {
        if (IsOwner)
        {
            var canv = GetComponentInChildren<Canvas>(true);
            if(canv != null)
            canv.gameObject.SetActive(true);
        }
        else
        {
            var canv = GetComponentInChildren<Canvas>(true);
            if(canv != null)
            canv.gameObject.SetActive(false);
        }


        if (IsOwner && spectatorCameraPrefab != null)
        {
            spectatorCameraInstance = Instantiate(spectatorCameraPrefab);
            spectatorCameraInstance.enabled = false;
        }

        if (IsServer)
        {
            currentHealth.Value = maxHealth;
            isAlive.Value = true;
            grenadeType.Value = UnityEngine.Random.Range(0, 4);
        }
    
        spawnnumber = UnityEngine.Random.Range(0, 4);
        respawnPos = TabSpawns[spawnnumber];
        isAlive.OnValueChanged += OnAliveChanged;
    }

    private void OnDestroy() => isAlive.OnValueChanged -= OnAliveChanged;

    private void OnAliveChanged(bool oldVal, bool newVal)
    {
        if (!IsOwner) return;

        SetVisuals(newVal);
        ToggleCameras(newVal);
    }

    private void SetVisuals(bool enabled)
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = enabled;
        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = enabled;
    }

    private void ToggleCameras(bool alive)
    {
        if (playerCamera != null) playerCamera.enabled = alive;
        if (spectatorCameraInstance != null) spectatorCameraInstance.enabled = !alive;

        if (rb != null)
        {
            rb.isKinematic = !alive;
            if (alive) rb.velocity = Vector3.zero;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int amount, ulong killerId)
    {
        if (!IsServer || !isAlive.Value) return;

        currentHealth.Value -= amount;
        if (currentHealth.Value == 0)
        {
            ExplodeAllC4ClientRpc();
            currentHealth.Value = 0;
            HandleKill(killerId);
            StartCoroutine(HandleDeathServer());
        }
    }


    void HandleKill(ulong killerId)
    {
        var killerObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(killerId);
        if (killerObj != null)
        {
            var killerHlth = killerObj.GetComponent<PlayerHealth>();
            killerHlth.kills.Value++;
            if (killerHlth.isVamp == true)
            {
                if (killerHlth.currentHealth.Value < 250)
                {
                    killerHlth.currentHealth.Value += 50;
                }
            }
            Debug.Log(killerHlth.kills.Value);
        }
    }

    [ClientRpc]
    private void ExplodeAllC4ClientRpc()
    {
        var allC4 = GameObject.FindObjectsOfType<C4Grenade>();
        var grenadeThrw = GetComponent<GrenadeThrower>();
        grenadeThrw.localC4Id = 0;
        foreach(var c4 in allC4){
            var owner = c4.GetComponent<GrenadeOwner>();
            var c4ids = c4.GetComponent<C4Grenade>();
            c4ids.id.Value = 0;
            if (owner != null && owner.ownerId.Value == OwnerClientId)
            {
                c4.ExplodeServerRpc();
            }
        }
    }

    private IEnumerator HandleDeathServer()
    {
        isVamp = false;
        isAlive.Value = false;
        spawnnumber = UnityEngine.Random.Range(0, 4);
        respawnPos = TabSpawns[spawnnumber];
        Physics.IgnoreLayerCollision(7, 8, true);
        var plrthr = GetComponent<GrenadeThrower>();
        plrthr.nadescount = 9;

        rb.isKinematic = true;

        yield return new WaitForSeconds(3f);

        ExecuteSkillDrawServerRpc();
        

        transform.position = respawnPos;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;
        RespawnClientPositionServerRpc(respawnPos);



        currentHealth.Value = maxHealth;
        isAlive.Value = true;


        int newType = UnityEngine.Random.Range(0, 4);
        grenadeType.Value = newType;

        SpawnProt = true;
        SpawnProtection(SpawnProt);
        yield return new WaitForSeconds(4f);
        SpawnProt = false;
        SpawnProtection(SpawnProt);
        Physics.IgnoreLayerCollision(7, 8, false);
        //gameObject.GetComponent<CapsuleCollider>().enabled = true;
    }

    [ServerRpc(RequireOwnership = false)]
    void ExecuteSkillDrawServerRpc()
    {
  
        ExecuteSkillDrawClientRpc(OwnerClientId);
    }

    [ClientRpc]
    void ExecuteSkillDrawClientRpc(ulong targetClientId)
    {

        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        var plrAbilities = playerhud.GetComponent<Abilities>();
        if (plrAbilities != null)
        {
            Cursor.lockState = CursorLockMode.None;
            plrAbilities.SkillDraw();
        }
    }


    [ServerRpc(RequireOwnership = false)]
    void RespawnClientPositionServerRpc(Vector3 newPosition)
    {


        RespawnClientPositionClientRpc(newPosition);
    }

    [ClientRpc]
    void RespawnClientPositionClientRpc(Vector3 newPosition)
    {
        if (IsOwner)
        {
            transform.position = newPosition;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetGrenadeTypeServerRpc(int type)
    {
        grenadeType.Value = Mathf.Clamp(type, 0, 3);
    }

    public void TakeDamage(int amount, ulong killerId)
    {
        if (IsServer)
            TakeDamageServerRpc(amount, killerId);
    }
}
