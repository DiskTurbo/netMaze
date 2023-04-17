using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text text;
    [SerializeField] RawImage pfp;
    [SerializeField] Texture2D defaultPFP;
    Player player;

    public void SetUp(Player _player)
    {
        this.gameObject.name = _player.NickName;
        player = _player;
        text.text = _player.NickName;
        StartCoroutine(DownloadProfileImage(_player.CustomProperties["ProfilePicture"]?.ToString()));
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if(player == otherPlayer)
        {
            Destroy(gameObject);
        }
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

    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }
}
