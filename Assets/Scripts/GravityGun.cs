using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using JSAM;

public class GravityGun : MonoBehaviour
{

    [SerializeField] Camera cam;
    [SerializeField] float maxGrabDistance = 10f, throwForce = 20f, lerpSpeed = 10f;
    [SerializeField] Transform objectHolder;

    public Rigidbody grabbedRB;
    float scrollSpeed = 250f;

    public bool stopGrabbing = false;
    public bool isGrabbing = false;

    PhotonView PV;

    private void Awake()
    {
        PV = GetComponentInParent<PhotonView>();
        if(PhotonNetwork.LocalPlayer.CustomProperties["GameMode"].ToString() != "Sandbox")
        {
            this.enabled = false;
        }
    }

    void Update()
    {
        if(!PV.IsMine)
        {
            return;
        }
        if (grabbedRB)
        {
            grabbedRB.MovePosition(Vector3.Lerp(grabbedRB.position, objectHolder.transform.position, Time.deltaTime * lerpSpeed));

            objectHolder.transform.position = objectHolder.transform.position + cam.transform.forward * Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * Time.deltaTime;

            if (Input.GetMouseButtonDown(0))
            {
                grabbedRB.isKinematic = false;
                PV.RPC(nameof(SetRigidBody), RpcTarget.Others, grabbedRB.gameObject.GetPhotonView().ViewID, grabbedRB.isKinematic);
                grabbedRB.AddForce(cam.transform.forward * throwForce, ForceMode.VelocityChange);
                grabbedRB = null;
                isGrabbing = false;
            }
        }

        else if(grabbedRB == null)
        {
            isGrabbing = false;
        }

        if(stopGrabbing == true)
        {
            grabbedRB.isKinematic = false;
            PV.RPC(nameof(SetRigidBody), RpcTarget.Others, grabbedRB.gameObject.GetPhotonView().ViewID, grabbedRB.isKinematic);
            grabbedRB = null;
            isGrabbing = false;
            stopGrabbing = false;
        }


        if (Input.GetKeyDown(KeyCode.E))
        {
            if (grabbedRB)
            {
                grabbedRB.isKinematic = false;
                PV.RPC(nameof(SetRigidBody), RpcTarget.Others, grabbedRB.gameObject.GetPhotonView().ViewID, grabbedRB.isKinematic);
                grabbedRB = null;
                isGrabbing = false;
            }
            else
            {
                RaycastHit hit;
                Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
                if (Physics.Raycast(ray, out hit, maxGrabDistance))
                {
                    Debug.Log(hit.collider.name);
                    if(hit.collider.name == "haider" || hit.collider.name == "baked_mesh")
                    {
                        if(JSAM.AudioManager.IsSoundPlaying(Sounds.haider, hit.collider.transform) == false)
                        {
                            JSAM.AudioManager.PlaySound(Sounds.haider, hit.collider.transform);
                        }
                        return;
                    }
                    if(hit.collider.gameObject.GetComponent<Rigidbody>() && !hit.collider.gameObject.GetComponent<Rigidbody>().isKinematic && hit.collider.gameObject.tag == "Interactable")
                    {
                        grabbedRB = hit.collider.gameObject.GetComponent<Rigidbody>();
                        Debug.Log(grabbedRB.name);
                    }

                    if (grabbedRB)
                    {
                        grabbedRB.isKinematic = true;
                        grabbedRB.gameObject.GetComponentInParent<PhotonView>().RequestOwnership();
                        PV.RPC(nameof(SetRigidBody), RpcTarget.Others, grabbedRB.gameObject.GetPhotonView().ViewID, grabbedRB.isKinematic);
                        isGrabbing = true;
                    }
                }
            }
        }
    }

    [PunRPC]
    public void SetRigidBody(int viewID, bool status)
    {
        Rigidbody rb = PhotonView.Find(viewID).gameObject.GetComponentInChildren<Rigidbody>();
        rb.isKinematic = (status);
    }
}
