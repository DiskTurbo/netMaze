using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.Networking;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class ScoreboardItem : MonoBehaviourPunCallbacks
{
    public TMP_Text usernameText;
    public TMP_Text killsText;
    public TMP_Text deathsText;
    public RawImage pfpImage;

    Player player;

    public void Initialize(Player player)
    {
        usernameText.text = player.NickName;
        this.player = player;
        StartCoroutine(DownloadProfileImage(player.CustomProperties["ProfilePicture"].ToString()));
        updateStats();
    }

    void updateStats()
    {
        if(player.CustomProperties.TryGetValue(("kills"), out object kills))
        {
            killsText.text = kills.ToString();
        }
        if (player.CustomProperties.TryGetValue(("deaths"), out object deaths))
        {
            deathsText.text = deaths.ToString();
        }
    }


    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if(targetPlayer == player)
        {
            if(changedProps.ContainsKey("kills") || changedProps.ContainsKey("deaths"))
            {
                updateStats();
            }
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
            pfpImage.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
    }
}
