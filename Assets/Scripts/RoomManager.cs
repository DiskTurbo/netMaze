using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("Loading " + SceneManager.GetActiveScene().name);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Debug.Log("Unloading " + SceneManager.GetActiveScene().name);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if(scene.buildIndex == 1 || scene.buildIndex == 2) //We are in the game scene
        {
            /*GameObject mapsFolder = GameObject.Find("Maps");
            switch (PhotonNetwork.CurrentRoom.CustomProperties["Map"].ToString())
            {
                case "Level 1":
                    mapsFolder.transform.Find("Level-1").gameObject.SetActive(true);
                    break;
                case "Level 2":
                    mapsFolder.transform.Find("Level-3").gameObject.SetActive(true);
                    break;
                default:
                    mapsFolder.transform.Find("Level-1").gameObject.SetActive(true);
                    break;
            }*/
            PhotonNetwork.LocalPlayer.CustomProperties["GameMode"] = PhotonNetwork.MasterClient.CustomProperties["GameMode"];
            PhotonNetwork.LocalPlayer.CustomProperties["Map"] = PhotonNetwork.MasterClient.CustomProperties["Map"];
            PhotonNetwork.LocalPlayer.CustomProperties["Timer"] = PhotonNetwork.MasterClient.CustomProperties["Timer"];

            Debug.Log(PhotonNetwork.MasterClient.CustomProperties["GameMode"]);
            Debug.Log(PhotonNetwork.LocalPlayer.CustomProperties["GameMode"]);

                ///

                /*ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
                hash.Add("Map", PhotonNetwork.MasterClient.CustomProperties["Map"]);
                hash.Add("GameMode", PhotonNetwork.MasterClient.CustomProperties["GameMode"]);
                hash.Add("Timer", PhotonNetwork.MasterClient.CustomProperties["Timer"]);
                PhotonNetwork.LocalPlayer.SetCustomProperties(hash);*/
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
        }
    }
}
