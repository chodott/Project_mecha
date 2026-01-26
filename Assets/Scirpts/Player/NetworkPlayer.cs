using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public override void OnNetworkSpawn()
    {
        //Init
        if (IsOwner)
        {

        }
    }


}
