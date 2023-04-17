using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class PlayerRagdollPassData : MonoBehaviour
{
    PhotonView PV;

    private void Start()
    {
        PV = GetComponentInParent<PhotonView>();
    }

    public void ApplyForce(Rigidbody rb, float force, Vector3 direction)
    {
        if(rb != null)
            PV.RPC(nameof(RPC_ApplyFoce), RpcTarget.All, rb, force, direction);
    }

    [PunRPC]
    void RPC_ApplyFoce(Rigidbody rb, float force, Vector3 direction) //Server run
    {
        //Testing
        if(rb != null)
            rb.AddForce(direction * force, ForceMode.Impulse);
    }
}
