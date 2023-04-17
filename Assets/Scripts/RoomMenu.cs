using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class RoomMenu : MonoBehaviour
{
    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItemPrefab;

    private void OnEnable()
    {
        foreach(KeyValuePair<string, RoomInfo> room in Launcher.instance.cachedRoomList)
        {
            if (room.Value.RemovedFromList || !room.Value.IsOpen || !room.Value.IsVisible)
                continue;
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(room.Value);
        }
    }
    private void OnDisable()
    {
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }
    }
}
