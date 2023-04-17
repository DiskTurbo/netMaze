using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using TMPro;
using Photon.Pun;

using JSAM;

public class TimerTest : MonoBehaviour
{
    TextMeshProUGUI t;

    //private float timer = 0f;

    bool startTimer = false;
    double timerIncrementValue;
    double startTime;
    double timerDuration;
    double timer;
    ExitGames.Client.Photon.Hashtable CustomeValue;

    private void Start()
    {
        t = GetComponent<TextMeshProUGUI>();
        if (PhotonNetwork.LocalPlayer.CustomProperties["Timer"].ToString() == "No Timer" || PhotonNetwork.LocalPlayer.CustomProperties["GameMode"].ToString() == "Sandbox") // change to master if not working
        {
            t.text = "---";
            this.transform.parent.gameObject.SetActive(false);
            Destroy(this);
            return;
        }
        if (/*PhotonNetwork.LocalPlayer.IsMasterClient &&*/ !PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("StartTime"))
        {
            CustomeValue = new ExitGames.Client.Photon.Hashtable();
            startTime = PhotonNetwork.Time;
            timer = double.Parse(PhotonNetwork.MasterClient.CustomProperties["Timer"].ToString());
            startTimer = true;
            CustomeValue.Add("StartTime", startTime);
            CustomeValue.Add("TimeAmount", timer);
            Debug.Log(timer);
            Debug.Log(PhotonNetwork.MasterClient.CustomProperties["Timer"].ToString());
            PhotonNetwork.CurrentRoom.SetCustomProperties(CustomeValue);
        }
        else
        {
            startTime = double.Parse(PhotonNetwork.CurrentRoom.CustomProperties["StartTime"].ToString());
            timer = double.Parse(PhotonNetwork.MasterClient.CustomProperties["Timer"].ToString());
            startTimer = true;
        }

        Debug.Log(timer);
    }

    private void Update()
    {
        //
        // LINK TIMER TO THE MASTER CLIENT ON JOIN
        // OR THE SYNC WILL BE OFF
        //


        if (!startTimer) return;

        timerIncrementValue = PhotonNetwork.Time - startTime;

        if (timerIncrementValue >= timer)
        {
            Debug.Log("Timer done!");
            PlayerManager.Find(PhotonNetwork.MasterClient).controller.GetComponentInChildren<EndgameUI>().EndOfGame();
            this.enabled = false;
        }

        if((timer - timerIncrementValue) <= 3 && !JSAM.AudioManager.IsSoundPlaying(Sounds.timertick))
        {
            JSAM.AudioManager.PlaySound(Sounds.timertick);
        }

        int minutes = Mathf.FloorToInt((float)(timerIncrementValue / 60));
        int seconds = Mathf.FloorToInt((float)timerIncrementValue - minutes * 60);
        string niceTime = string.Format("{0:0}:{1:00}", seconds == 0 ? timer / 60 - minutes : ((timer / 60) - minutes) - 1, seconds == 0 ? "00" : 60 - seconds);
        t.text = niceTime;
    }
}
