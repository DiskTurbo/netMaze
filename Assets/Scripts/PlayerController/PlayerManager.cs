using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using Photon.Realtime;
using System.Linq;
using Photon.Pun;

using JSAM;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerManager : MonoBehaviour
{
    PhotonView PV;

    public GameObject controller;
    public GameObject currentMap;

    PlayerVisualsManager visualManager;
    GunHolderManager gunManager;
    CameraRigManager CRM;

    [Header("Pure For Testing")]
    public bool isPaused = false;

    public GameObject Primary;
    public GameObject Secondary;
    public GameObject Melee;

    [HideInInspector] public int primaryIndex = 0;
    [HideInInspector] public int secondaryIndex = 0;
    [HideInInspector] public int meleeIndex = 0;

    int netPower;
    int deaths;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("kills", 0);
            hash.Add("deaths", 0);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            if (PhotonNetwork.MasterClient.CustomProperties["GameMode"].ToString() == "Sandbox" && PV.IsMine)
            {
                activateMap(PhotonNetwork.LocalPlayer.CustomProperties["Map"].ToString());
                //CreateController();
                return;
            }
            else
            {
                activateMap(PhotonNetwork.LocalPlayer.CustomProperties["Map"].ToString());
            }
        }
    }

    void CreateController()
    {
        Time.timeScale = 1f;

        Debug.Log("GAME: Building Player Controller: 03");

        Transform spawnPoint = GameObject.Find("Maps").GetComponentInChildren<SpawnManager>().GetSpawnpoint();
        //Transform spawnPoint = SpawnManager.instance.GetSpawnpoint();

        /*if (!PV.IsMine)
            return;*/

        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnPoint.position, Quaternion.identity, 0, new object[] { PV.ViewID });

        controller.name = PhotonNetwork.NickName;

        controller.GetComponentInChildren<PlayerMovement>().orientation.rotation = spawnPoint.rotation;

        visualManager = controller.GetComponentInChildren<PlayerVisualsManager>();
        visualManager.DR();

        CRM = controller.GetComponentInChildren<CameraRigManager>();

        gunManager = controller.GetComponentInChildren<GunHolderManager>();
        gunManager.Gun_1 = Instantiate(Primary, gunManager.gameObject.transform.position, Quaternion.identity);
        gunManager.Gun_1.transform.parent = gunManager.transform;
        gunManager.Gun_1.transform.localScale = Vector3.one;
        gunManager.Gun_1.GetComponent<GunSystem>().playerManager = this;
        gunManager.Gun_1.GetComponent<GunSystem>().pm = controller.GetComponentInChildren<PlayerMovement>();

        gunManager.Gun_2 = Instantiate(Secondary, gunManager.gameObject.transform.position, Quaternion.identity);
        gunManager.Gun_2.transform.parent = gunManager.transform;
        gunManager.Gun_2.transform.localScale = Vector3.one;
        gunManager.Gun_2.GetComponent<GunSystem>().playerManager = this;
        gunManager.Gun_2.GetComponent<GunSystem>().pm = controller.GetComponentInChildren<PlayerMovement>();

        gunManager.Melee = Instantiate(Melee, gunManager.gameObject.transform.position, Quaternion.identity);
        gunManager.Melee.transform.parent = gunManager.transform;
        gunManager.Melee.transform.localScale = Vector3.one;
        gunManager.Melee.GetComponent<MeleeSystem>().playerManager = this;
        gunManager.Melee.GetComponent<MeleeSystem>().pm = controller.GetComponentInChildren<PlayerMovement>();

        gunManager.SetWeaponsStart();

        isPaused = false;
    }

    public void SetUpArms(int gunNum, Material color)
    {
        if(!PV.IsMine)
        {
            return;
        }
        GameObject arms;
        switch (gunNum)
        {
            case 1:
                arms = gunManager.Gun_1.transform.Find("Arms").gameObject;
                break;
            case 2:
                arms = gunManager.Gun_2.transform.Find("Arms").gameObject;
                break;
            default:
                arms = gunManager.Gun_1.transform.Find("Arms").gameObject;
                break;
        }

        MeshRenderer[] armMeshes = arms.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer armM in armMeshes)
        {
            armM.material = color;
        }
    }

    public static PlayerManager Find(Player player)
    {
        return FindObjectsOfType<PlayerManager>().SingleOrDefault(x => x.PV.Owner == player);
    }

    public void getDamage(int dmg)
    {
        PV.RPC(nameof(RPC_GetDamage), PV.Owner, dmg);
    }

    [PunRPC]
    void RPC_GetDamage(int dmg)
    {
        netPower += dmg;

        Hashtable hash = new Hashtable();
        hash.Add("kills", netPower);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    [PunRPC]
    public void playShootSound(Vector3 position, byte soundEffect)
    {
        JSAM.AudioManager.PlaySound((Sounds)soundEffect, position);
    }

    public void KillPlayer(Vector3 vel)
    {
        StartCoroutine(OnPlayerDeathSet(vel));

        deaths++;

        Hashtable hash = new Hashtable();
        hash.Add("deaths", deaths);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public void activateMap(string MapName)
    {
        GameObject mapsFolder = GameObject.Find("Maps");
        if (MapName.EndsWith(".json"))
        {
            StreamReader sr = new StreamReader(Application.dataPath + "\\CustomMaps\\" + MapName);
            string json = sr.ReadToEnd();
            sr.Close();

            CustomMapData loadMap = JsonUtility.FromJson<CustomMapData>(json);

            currentMap = mapsFolder.transform.Find(loadMap.mapID).gameObject;
            currentMap.SetActive(true);
            if(PhotonNetwork.IsMasterClient)
            {
                SaveLoadSandbox.instance.LoadBlocks(MapName.ToString());
            }
            if (PV.IsMine)
            {
                CreateController();
            }
            return;
        }
        else
        {
            currentMap = mapsFolder.transform.Find(MapName).gameObject;
            currentMap.SetActive(true);
            if (PV.IsMine)
            {
                CreateController();
            }
        }
    }
    IEnumerator OnPlayerDeathSet(Vector3 vel)
    {
        CRM.RandomDeathCamera();
        visualManager.ER(vel);

        Destroy(gunManager.Gun_1);
        Destroy(gunManager.Gun_2);

        //Await death timer
        yield return new WaitForSeconds(5f);

        //Kill that mf and bring them back!
        PhotonNetwork.Destroy(controller);
        CreateController();
    }

}
