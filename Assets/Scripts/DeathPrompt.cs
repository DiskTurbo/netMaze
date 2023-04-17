using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.Networking;

public class DeathPrompt : MonoBehaviour
{
    public TMP_Text killerText;
    public TMP_Text scoreText;
    public RawImage pfp;

    [Space(40)]
    public RenderTexture RendTexture;
    public RawImage KillerCam;

    public void Initialize(Player _killerPlayer)
    {
        if (_killerPlayer == PhotonNetwork.LocalPlayer)
        {
            killerText.text = "You eliminated yourself!";
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(("kills"), out object kills) && (int)PhotonNetwork.LocalPlayer.CustomProperties["kills"] != 0)
            {
                scoreText.text = "You have a netPower of: \n" + kills.ToString();
            }
            else
            {
                scoreText.text = "Maybe try not to do that again!";
            }
        }
        else
        {
            killerText.text = "You were wiped out by " + _killerPlayer.NickName + "!";
            scoreText.text = "They now have a netPower of: \n" + _killerPlayer.CustomProperties["kills"].ToString();
            //GameObject.Find(_killerPlayer.NickName).GetComponentInChildren<CameraRigManager>().ActivateKillerCamera().targetTexture = RendTexture;
        }
        StartCoroutine(DownloadProfileImage(_killerPlayer.CustomProperties["ProfilePicture"].ToString()));
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
