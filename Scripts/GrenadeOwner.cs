using Unity.Netcode;
using UnityEngine;

public class GrenadeOwner : NetworkBehaviour
{

    public NetworkVariable<ulong> ownerId = new NetworkVariable<ulong>(0,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
}
