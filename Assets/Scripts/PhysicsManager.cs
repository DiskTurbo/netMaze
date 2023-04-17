using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class PhysicsManager : MonoBehaviour
{
    PhotonView PV;

    public static PhysicsManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this);
        else
            instance = this;

        PV = GetComponent<PhotonView>();
    }

    public void ApplyForce(Vector3 point, float force, float radius) //Local Run
    {
        Debug.Log("LOCAL BOMBA");
        PV.RPC(nameof(RPC_ApplyForce), RpcTarget.All, point, force, radius);
    }

    [PunRPC]
    void RPC_ApplyForce(Vector3 point, float force, float radius) //Server run
    {
        Debug.Log("SERVER BOMBA");
        Vector3 explosionPos = point;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
                rb.AddExplosionForce(force, explosionPos, radius, 3.0F);
        }
    }
}
