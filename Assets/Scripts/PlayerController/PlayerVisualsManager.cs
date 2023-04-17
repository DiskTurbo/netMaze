using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class PlayerVisualsManager : MonoBehaviour
{
    public GameObject physicalPlayer;
    public GameObject playerObject;
    public Animator animator;
    public Transform orientation;
    public Transform hips;
    public SkinnedMeshRenderer playerModel;
    public MeshRenderer head;
    public Material[] playerColors = new Material[12];
    public Sprite[] playerFaceColors = new Sprite[12];
    public Image funnyFace;
    public TMP_Text username;
    public GameObject overheadCanvas;

    [Space(25)]
    public GameObject CurrentEquip;
    public GameObject Head;

    public PhysicMaterial playerPhyx_Base;
    public PhysicMaterial playerPhyx_Rigid;

    public CameraRigManager CRM;

    public List<GameObject> playerUI;

    PhotonView PV;

    private void Awake()
    {
        PV = GetComponentInParent<PhotonView>();
    }

    private void Start()
    {
        if(PV.IsMine)
        {
            DisableModel();
            SetUpPlayer();
        }
        DR();
    }

    public void DisableModel()
    {
        playerModel.enabled = false;
        head.enabled = false;

        CurrentEquip.SetActive(false);
}
    public void EnableModel()
    {
        playerModel.enabled = true;
        head.enabled = true;

        CurrentEquip.SetActive(true);
    }

    public void SetUpPlayer() //Local run
    {
        PV.RPC(nameof(RPC_SetUpPlayer), RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_SetUpPlayer() //Server run
    {
        playerModel.material = playerColors[(int)PV.Owner.CustomProperties["PlayerColor"]];
        head.material = playerColors[(int)PV.Owner.CustomProperties["PlayerColor"]];
        funnyFace.sprite = playerFaceColors[(int)PV.Owner.CustomProperties["PlayerColor"]];

        username.text = PV.Owner.NickName;

        if(!PV.IsMine)
        {
            return;
        }

        PlayerManager.Find(PhotonNetwork.LocalPlayer)?.SetUpArms(1, playerColors[(int)PV.Owner.CustomProperties["PlayerColor"]]);
        PlayerManager.Find(PhotonNetwork.LocalPlayer)?.SetUpArms(2, playerColors[(int)PV.Owner.CustomProperties["PlayerColor"]]);
    }

    public void DR()
    {
        PV.RPC(nameof(RPC_DisableRagdoll), RpcTarget.AllBuffered);
    }
    public void ER(Vector3 vel)
    {
        PV.RPC(nameof(RPC_EnterRagdoll), RpcTarget.All, vel);
    }

    private void Update()
    {
        transform.rotation = orientation.rotation;

        overheadCanvas.transform.LookAt(Camera.main.transform.position + Camera.main.transform.rotation * Vector3.back);
        overheadCanvas.transform.Rotate(0f, 180f, 0f);
    }

    [PunRPC]
    void RPC_DisableRagdoll()
    {
        Collider[] c = GetComponentsInChildren<Collider>();
        for(int i = 0; i < c.Length; i++)
        {
            c[i].enabled = false;
            c[i].material = playerPhyx_Base;
        }
        Rigidbody[] rbc = GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < rbc.Length; i++)
        {
            rbc[i].isKinematic = true;
            rbc[i].gameObject.tag = "Interactable";
        }

        CurrentEquip.GetComponent<MeshCollider>().enabled = false;
        Head.GetComponent<MeshCollider>().enabled = false;
    }

    public void StopAnimation()
    {
        physicalPlayer.GetPhotonView()?.RPC("EndEmote", RpcTarget.AllBuffered);
    }


    [PunRPC]
    void RPC_EnterRagdoll(Vector3 vel)
    {
        if (PV.IsMine)
        {
            CRM.RandomDeathCamera();
            foreach (GameObject g in playerUI)
            {
                g.SetActive(false);
            }
        }

        StopAnimation();

        playerObject.SetActive(false);
        EnableModel();
        animator.enabled = false;

        overheadCanvas.SetActive(false);

        JSAM.AudioManager.PlaySound(Sounds.death1, hips);
        if(physicalPlayer.GetComponent<Rigidbody>())
            physicalPlayer.GetComponent<Rigidbody>().isKinematic = true;

        Collider[] c = GetComponentsInChildren<Collider>();
        for (int i = 0; i < c.Length; i++)
        {
            c[i].enabled = true;
            c[i].material = playerPhyx_Rigid;
        }
        Rigidbody[] rbc = GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < rbc.Length; i++)
        {
            rbc[i].isKinematic = false;
            rbc[i].AddForce(vel * 2f, ForceMode.Impulse);
        }

        CurrentEquip.GetComponent<MeshCollider>().enabled = true;
        CurrentEquip.AddComponent<Rigidbody>();
        CurrentEquip.AddComponent<KillAfterDelay>();
        CurrentEquip.transform.parent = null;

        Head.GetComponent<MeshCollider>().enabled = true;
        Head.AddComponent<Rigidbody>();
        Head.GetComponent<Rigidbody>().AddForce(vel, ForceMode.Impulse);
        Head.AddComponent<KillAfterDelay>();
        Head.transform.parent = null;
    }
}
