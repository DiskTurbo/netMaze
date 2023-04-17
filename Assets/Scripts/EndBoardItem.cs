using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.Networking;
using UnityEngine.UI;
using JSAM;
public class EndBoardItem : MonoBehaviour
{
    public TMP_Text rankingText;
    public TMP_Text playerNameText;
    public TMP_Text scoreText;
    public RawImage pfp;
    public void Initialize(Player _player, int ranking)
    {
        switch(ranking)
        {
            case 1:
                rankingText.text = "1st";
                break;
            case 2:
                rankingText.text = "2nd";
                break;
            case 3:
                rankingText.text = "3rd";
                break;
        }
        if(_player == PhotonNetwork.LocalPlayer)
        {
            playerNameText.text = _player.NickName + " (You!)";
            JSAM.AudioManager.PlaySound(Sounds.tada);
        }
        else
        {
            playerNameText.text = _player.NickName;
        }
        scoreText.text = _player.CustomProperties["kills"].ToString();
        StartCoroutine(DownloadProfileImage(_player.CustomProperties["ProfilePicture"].ToString()));
    }

    IEnumerator DownloadProfileImage(string PFPURL)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(PFPURL);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
            pfp.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
    }
}
