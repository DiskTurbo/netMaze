using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

using TMPro;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] TMP_Text roomName;
    [SerializeField] TMP_Text playerText;

    public RoomInfo info;

    public void SetUp(RoomInfo _info)
    {
        info = _info;
        roomName.text = _info.Name;
        this.gameObject.name = _info.Name;
        if(_info.PlayerCount == 1)
        {
            playerText.text = _info.PlayerCount + " Player in Room";
        }
        else
        {
            playerText.text = _info.PlayerCount + " Players in Room";
        }
    }
    public void OnClick()
    {
        Launcher.instance.JoinRoom(info);
    }
}
