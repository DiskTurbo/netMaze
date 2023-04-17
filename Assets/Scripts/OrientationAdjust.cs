using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class OrientationAdjust : MonoBehaviour
{
    PhotonView PV;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Update()
    {
        PV.RPC(nameof(RPC_Adjust), RpcTarget.All, transform.localRotation.eulerAngles);
    }

    [PunRPC]
    void RPC_Adjust(Vector3 rot) //Server run
    {
        if (PV.IsMine) //Adjusts to Local run
            return;

        transform.localRotation = Quaternion.Euler(rot);
    }
}
