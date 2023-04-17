using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

using TMPro;
public class ChatManager : MonoBehaviourPunCallbacks 
{
    public bool isChatting = false;
    string chatInput = "";
    public Font font;
    PlayerMovement pm;

    public static ChatManager Instance;

    [System.Serializable]
    public class ChatMessage
    {
        public string sender = "";
        public string message = "";
        public float timer = 0;
    }


    List<ChatMessage> chatMessages = new List<ChatMessage>();
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {

        if (gameObject.GetComponent<PhotonView>() == null)
        {
            PhotonView photonView = gameObject.AddComponent<PhotonView>();
            photonView.ViewID = 8349;
        }
        else
        {
            photonView.ViewID = 8349;
        }
    }

    void Update()
    {
        if(pm == null)
        {
            pm = GameObject.Find(PhotonNetwork.LocalPlayer.NickName).GetComponentInChildren<PlayerMovement>();
        }
        if (Input.GetKeyUp(KeyCode.Return) && !isChatting && !pm.isInPauseMenu)
        {
            isChatting = true;
            chatInput = "";
        }

        for (int i = 0; i < chatMessages.Count; i++)
        {
            if (chatMessages[i].timer > 0)
            {
                chatMessages[i].timer -= Time.deltaTime;
            }
        }
    }


    public void ChatOn()
    {
        if (!isChatting)
        {
            isChatting = true;
            chatInput = "";
        }
    }
    void OnGUI()
    {
        GUIStyle inputStyle = GUI.skin.GetStyle("box");
        inputStyle.alignment = TextAnchor.MiddleLeft;
        inputStyle.font = font;
        inputStyle.fontSize = 12;

        if (!isChatting)
        {
            GUI.Label(new Rect(25, Screen.height - 200, 200, 25), "Press Enter to start chatting...", inputStyle);
        }
        else
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                isChatting = false;
                if (chatInput.Replace(" ", "") != "")
                {
                    photonView.RPC("SendChat", RpcTarget.All, PhotonNetwork.LocalPlayer, chatInput);
                }
                chatInput = "";
            }

            GUI.SetNextControlName("ChatField");
            chatInput = GUI.TextField(new Rect(25, Screen.height - 200, 300, 22), chatInput, 60, inputStyle);

            GUI.FocusControl("ChatField");
        }

        for (int i = 0; i < chatMessages.Count; i++)
        {
            if (chatMessages[i].timer > 0 || isChatting)
            {
                if (chatMessages[i].sender == "{SYSTEM}")
                {
                    inputStyle.normal.textColor = Color.red;
                    GUI.Label(new Rect(25, Screen.height - 225 - 25 * i, 300, 25), chatMessages[i].sender + ": " + chatMessages[i].message, inputStyle);
                    inputStyle.normal.textColor = Color.white;
                }
                else
                {
                    inputStyle.normal.textColor = Color.white;
                    GUI.Label(new Rect(25, Screen.height - 225 - 25 * i, 300, 25), chatMessages[i].sender + ": " + chatMessages[i].message, inputStyle);
                }
            }
        }
    }

    [PunRPC]
    void SendChat(Player sender, string message)
    {
        switch(message)
        {
            case "/seizure":
                pm = GameObject.Find(PhotonNetwork.LocalPlayer.NickName).GetComponentInChildren<PlayerMovement>();
                if(!pm.isEmote && pm.PV.Owner == sender|| !pm.grounded && pm.PV.Owner == sender)
                    pm.PV.RPC("PlayEmoteAnimation", RpcTarget.AllBuffered, "Seizure");
                return;
            case "/familyguyfunny":
                pm = GameObject.Find(PhotonNetwork.LocalPlayer.NickName).GetComponentInChildren<PlayerMovement>();
                if (!pm.isEmote && pm.PV.Owner == sender || !pm.grounded && pm.PV.Owner == sender)
                    pm.PV.RPC("PlayEmoteAnimation", RpcTarget.AllBuffered, "Stewie");
                return;
            case "/kill":
                pm = GameObject.Find(PhotonNetwork.LocalPlayer.NickName).GetComponentInChildren<PlayerMovement>();
                if(pm.PV.Owner == sender)
                {
                    pm.TakeDamage(250);
                }
                return;
            default:
                break;
        }
        ChatMessage m = new ChatMessage();
        m.sender = sender.NickName;
        m.message = message;
        m.timer = 15.0f;

        chatMessages.Insert(0, m);
        if (chatMessages.Count > 8)
        {
            chatMessages.RemoveAt(chatMessages.Count - 1);
        }
    }
    [PunRPC]
    public void SendSystemMessage(string message)
    {
        ChatMessage m = new ChatMessage();
        m.sender = "{SYSTEM}";
        m.message = message;
        m.timer = 15.0f;
        
        chatMessages.Insert(0, m);
        if (chatMessages.Count > 8)
        {
            chatMessages.RemoveAt(chatMessages.Count - 1);
        }
    }
    public void SendSystemMessageGlobal(string _message)
    {
        photonView.RPC("SendSystemMessage", RpcTarget.All, _message);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("SendSystemMessage", RpcTarget.All, newPlayer.NickName + " has joined the room.");
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if(PhotonNetwork.IsMasterClient)
            photonView.RPC("SendSystemMessage", RpcTarget.All, otherPlayer.NickName + " has left the room.");
    }
}
