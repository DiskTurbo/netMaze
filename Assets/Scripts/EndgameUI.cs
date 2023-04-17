using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;
using System.Linq;

using JSAM;

public class EndgameUI : MonoBehaviour
{
    public CanvasGroup EndgameUICanvas;
    public CanvasGroup IngameUICanvas;
    public RawImage background;
    public List<double> list = new List<double>();
    public List<double> listInOrder = new List<double>();
    PhotonView PV;
    float first, second, third = 0;
    public List<Player> playerList = new List<Player>();
    public GameObject endBoardItem;
    public CanvasGroup playerUI;
    public Transform endBoardTrans;
    public GameObject header;

    private void Awake()
    {
        PV = GetComponentInParent<PhotonView>();
    }

    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Y) && !ChatManager.Instance.isChatting)
        {
            EndOfGame();
        }*/
    }

    public void EndOfGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC(nameof(RPC_EndOfGame), RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void RPC_EndOfGame()
    {
        StartCoroutine(nameof(EndUI));
    }

    [PunRPC]
    void RPC_BackToLobby()
    {
        PhotonNetwork.LocalPlayer.CustomProperties["MatchEnd"] = true;
        PhotonNetwork.CurrentRoom.CustomProperties.Clear();
        PhotonNetwork.LoadLevel(0);
    }
    void BackToLobby()
    {
        PV.RPC(nameof(RPC_BackToLobby), RpcTarget.All);
    }
    IEnumerator EndUI()
    {
        GameObject.Find(PhotonNetwork.NickName + "/Player UI").GetComponent<CanvasGroup>().alpha = 0;
        Destroy(GameObject.Find(PhotonNetwork.NickName).GetComponentInChildren<CameraShake>());
        GameObject.Find(PhotonNetwork.NickName).GetComponentInChildren<PlayerMovement>().isEmote = true;

        ChatManager.Instance.enabled = false;
        EndgameUICanvas.alpha = 1;

        JSAM.AudioManager.StopAllSounds();
        JSAM.AudioManager.StopMusic();

        EndgameUICanvas.gameObject.GetComponentInChildren<Animator>().Play("Finish");
        JSAM.AudioManager.PlaySound(Sounds.timeover);

        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(4f);

        UpdateSortList();
        if (!JSAM.AudioManager.IsSoundPlaying(Sounds.tada))
        {
            JSAM.AudioManager.PlaySound(Sounds.defeat);
        }
        StartCoroutine(BackToLobbyDelay(10f));
    }
    private IEnumerator BackToLobbyDelay(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        PV.RPC(nameof(RPC_BackToLobby), RpcTarget.All);
    }
    public void UpdateSortList()
    {
        header.SetActive(true);
        endBoardTrans.gameObject.SetActive(true);

        foreach (Player _player in PhotonNetwork.PlayerList)
        {
            playerList.Add(_player);
        }

        playerList.Sort(SortByScore);

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Instantiate(endBoardItem, endBoardTrans).GetComponent<EndBoardItem>().Initialize(PhotonNetwork.LocalPlayer, 1);
        }
        else if(PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            for (int i = 0; i < 2; i++)
            {
                Instantiate(endBoardItem, endBoardTrans).GetComponent<EndBoardItem>().Initialize(playerList[playerList.Count - (i + 1)], i + 1);
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                Instantiate(endBoardItem, endBoardTrans).GetComponent<EndBoardItem>().Initialize(playerList[playerList.Count - (i + 1)], i + 1);
            }
        }
    }

    static int SortByScore(Player p1, Player p2)
    {
        int p1kill = (int)p1.CustomProperties["kills"];
        int p2kill = (int)p2.CustomProperties["kills"];
        return p1kill.CompareTo(p2kill);
        //return p1.CustomProperties["kills"].ToString().CompareTo(p2.CustomProperties["kills"].ToString());
    }
}
