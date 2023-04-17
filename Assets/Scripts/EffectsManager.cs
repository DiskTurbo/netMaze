using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class EffectsManager : MonoBehaviour
{
    PhotonView PV;

    public static EffectsManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this);
        else
            instance = this;

        PV = GetComponent<PhotonView>();
    }

    public void BulletTrail(string path, Vector3 start, Vector3 end)
    {
        PV.RPC(nameof(RPC_BulletTrail), RpcTarget.All, path ,start, end);
    }
    [PunRPC]
    void RPC_BulletTrail(string path, Vector3 start, Vector3 end)
    {
        GameObject temp_t = Instantiate((GameObject)Resources.Load(path), start, Quaternion.identity);
        temp_t.GetComponent<LocalTrail>().SetInfo(start, end);
    }
}
