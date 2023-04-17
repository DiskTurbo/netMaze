using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using JSAM;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher instance;

    [SerializeField] TMP_InputField roomNameInput;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;

    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItemPrefab;

    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject playerListItemPrefab;

    [SerializeField] GameObject startGameButton;

    [SerializeField] TMP_InputField usernameInput;
    [SerializeField] TMP_InputField pfpInput;

    [SerializeField] Texture2D defaultPFP;
    [SerializeField] RawImage pfpPreview;

    [SerializeField] GameObject roomSettingsPanel;
    [SerializeField] GameObject roomManager;

    [SerializeField] GameObject brawlSettings;
    [SerializeField] GameObject sandboxSettings;


    [SerializeField] TMP_Dropdown aspectRatio;
    [SerializeField] Toggle fullscreen;
    [SerializeField] Slider mastervolume;
    [SerializeField] Slider soundvolume;
    [SerializeField] Slider musicvolume;

    public Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();


    //List<RoomInfo> fullRoomList = new List<RoomInfo>();
    //List<RoomListItem> roomListItems = new List<RoomListItem>();

    bool isReady;
    int playersReady;

    PhotonView PV;

    private ExitGames.Client.Photon.Hashtable _myCustomProperties = new ExitGames.Client.Photon.Hashtable();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        if (PV == null)
        {
            PV = GetComponent<PhotonView>();
        }
        Debug.Log(PhotonNetwork.IsConnectedAndReady);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isReady = false;

        if (!PlayerPrefs.HasKey("PlayerColor") || !PlayerPrefs.HasKey("Username")|| !PlayerPrefs.HasKey("ProfilePicURL") || !PlayerPrefs.HasKey("aspectRatio") || !PlayerPrefs.HasKey("fullscreen") || !PlayerPrefs.HasKey("mastervolume") || !PlayerPrefs.HasKey("soundvolume") || !PlayerPrefs.HasKey("musicvolume"))
        {
            setDefaultProperties();
        }

        if(PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("kills"))
        {
            PhotonNetwork.LocalPlayer.CustomProperties["kills"] = 0;
        }

        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Map"))
        {
            PhotonNetwork.LocalPlayer.CustomProperties["kills"] = 0;
        }

        if(PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("MatchEnd") && (bool)PhotonNetwork.LocalPlayer.CustomProperties["MatchEnd"] == true)
        {
            PhotonNetwork.CurrentRoom.IsVisible = true;
            PhotonNetwork.OpRemoveCompleteCache();
            OnJoinedRoom();
            PhotonNetwork.LocalPlayer.CustomProperties["MatchEnd"] = false;
        }

        aspectRatio.value = PlayerPrefs.GetInt("aspectRatio");
        fullscreen.isOn = PlayerPrefs.GetInt("fullscreen") == 1 ? true : false;


        mastervolume.value = (PlayerPrefs.GetFloat("mastervolume"));
        soundvolume.value = (PlayerPrefs.GetFloat("soundvolume"));
        musicvolume.value = (PlayerPrefs.GetFloat("musicvolume"));

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            GameObject.Instantiate(roomManager, Vector3.zero, Quaternion.identity);
            SetUpLocalPlayer();
            MenuManager.instance.OpenMenu("loading");
            Debug.Log("SERVER: Looking For Master: 00");
            PhotonNetwork.ConnectUsingSettings();
        }

    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("SERVER: Connected To Master: 01");
        PhotonNetwork.JoinLobby();

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    { 
        Debug.Log("SERVER: Joined Lobby: 02");

        cachedRoomList.Clear();

        MenuManager.instance.OpenMenu("title");
        usernameInput.text = PhotonNetwork.NickName;
    }

    public void CreateRoom()
    {
        Time.timeScale = 1f;
        if (string.IsNullOrEmpty(roomNameInput.text))
        {
            return;
        }
        PhotonNetwork.CreateRoom(roomNameInput.text);
        MenuManager.instance.OpenMenu("loading");
        PhotonNetwork.LocalPlayer.CustomProperties["GameMode"] = "Brawl";
        PhotonNetwork.LocalPlayer.CustomProperties["Map"] = "Backlink Plains";
        PhotonNetwork.LocalPlayer.CustomProperties["Timer"] = "600";
    }

    public override void OnJoinedRoom()
    {
        Time.timeScale = 1f;
        MenuManager.instance.OpenMenu("room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        JSAM.AudioManager.PlaySound(Sounds.joinroom);

        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < players.Length; i++)
        {
          Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
           if (players[i].CustomProperties.ContainsKey("kills"))
           {
               players[i].CustomProperties["kills"] = 0;
           }
        }

        if(!PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
            hash.Add("Map", PhotonNetwork.MasterClient.CustomProperties["Map"]);
            hash.Add("GameMode", PhotonNetwork.MasterClient.CustomProperties["GameMode"]);
            hash.Add("Timer", PhotonNetwork.MasterClient.CustomProperties["Timer"]);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }

        roomSettingsPanel.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        roomSettingsPanel.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creation Failed: " + message;
        MenuManager.instance.OpenMenu("error");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ReadyUp()
    {
        if (!isReady) // Readying Up
        {
            startGameButton.GetComponentInChildren<TMP_Text>().text = "Unready";
            startGameButton.GetComponent<Image>().color = new Color32(180, 35, 0, 255);
            isReady = true;
            PV.RPC(nameof(PlayerReadyState), RpcTarget.AllBuffered, 1, PhotonNetwork.LocalPlayer.NickName.ToString());
            return;
        }
        else // Unreadying
        {
            startGameButton.GetComponentInChildren<TMP_Text>().text = "Ready";
            startGameButton.GetComponent<Image>().color = new Color32(8, 180, 0, 255);
            isReady = false;
            PV.RPC(nameof(PlayerReadyState), RpcTarget.AllBuffered, -1, PhotonNetwork.LocalPlayer.NickName.ToString());
            return;
        }
    }

    [PunRPC]
    public void PlayerReadyState(int addition, string username)
    {
        GameObject readyIcon = playerListContent.Find(username + "/PFP/Profile Picture/Ready").gameObject;
        if(addition == 1)
            readyIcon.gameObject.SetActive(true);
        if (addition == -1)
            readyIcon.gameObject.SetActive(false);
        
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        playersReady = playersReady + addition;
        if(playersReady == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        if (PhotonNetwork.MasterClient.CustomProperties["GameMode"].ToString() == "Sandbox")
        {
            PhotonNetwork.LoadLevel(2);
        }
        else
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.LoadLevel(1);
        }
    }

    public void LeaveRoom()
    {
        if(isReady)
        {
            PV.RPC(nameof(PlayerReadyState), RpcTarget.MasterClient, -1);
        }
        PhotonNetwork.LeaveRoom();
        MenuManager.instance.OpenMenu("loading");
    }

    public void JoinRoom(RoomInfo info)
    {
        if(string.IsNullOrWhiteSpace(PhotonNetwork.NickName))
        {
            setPlayerUsername("FunnyMan " + Random.Range(0, 100).ToString("00"));
        }
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.instance.OpenMenu("loading");
    }

    public override void OnLeftRoom()
    {
        MenuManager.instance.OpenMenu("title");
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            if (info.RemovedFromList)
            {
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
                Debug.Log(info.Name);
            }
        }
    }
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            UpdateCachedRoomList(roomList);
        }

    /*public override void OnRoomListUpdate(List<RoomInfo> roomList) the normal one
    {
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }

        for(int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList || !roomList[i].IsOpen || !roomList[i].IsVisible)
                continue;
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }*/

    /*public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        RoomInfo newRoom = null;
        foreach (RoomInfo updatedRoom in roomList)
        {
            RoomInfo existingRoom = fullRoomList.Find(x => x.Name.Equals(updatedRoom.Name)); // Check to see if we have that room already
            if (existingRoom == null) // WE DO NOT HAVE IT
            {
                fullRoomList.Add(updatedRoom); // Add the room to the full room list
                if (newRoom == null)
                {
                    newRoom = updatedRoom;
                }
            }
            else if (updatedRoom.RemovedFromList || updatedRoom.PlayerCount == 0) // WE DO HAVE IT, so check if it has been removed
            {
                fullRoomList.Remove(existingRoom); // Remove it from our full room list
            }
        }
        RenderRoomList();

        if (newRoom != null && !PhotonNetwork.InRoom)
        {
            newRoom.CustomProperties.TryGetValue("ver", out object version);
            if (version != null && (string)version == Application.version)
            {
                JoinRoom(newRoom);
            }
        }
    }

    void RenderRoomList()
    {
        RemoveRoomList();
        foreach (RoomInfo roomInfo in fullRoomList)
        {
            if (roomInfo.PlayerCount == 0 || roomInfo.RemovedFromList)
                continue;
            RoomListItem roomListItem = Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>();
            roomListItem.SetUp(roomInfo);
            roomListItems.Add(roomListItem);
        }
    }

    void RemoveRoomList()
    {
        foreach (RoomListItem roomListItem in roomListItems)
        {
            Destroy(roomListItem.gameObject);
        }
        roomListItems.Clear();
    }*/

    private void setDefaultProperties()
    {
        PlayerPrefs.SetInt("PlayerColor", 0);
        PlayerPrefs.SetString("Username", "FunnyMan " + Random.Range(0, 100).ToString("00"));
        PlayerPrefs.SetString("ProfilePicURL", "https://i.imgur.com/WodHrPv.jpg");

        PlayerPrefs.SetFloat("mastervolume", 1);
        PlayerPrefs.SetFloat("soundvolume", 1);
        PlayerPrefs.SetFloat("musicvolume", 0.5f);

        PlayerPrefs.SetInt("aspectRatio", 0);
        PlayerPrefs.SetInt("fullscreen", 0);

        PlayerPrefs.Save();

        PhotonNetwork.LocalPlayer.CustomProperties["PlayerColor"] = PlayerPrefs.GetInt("PlayerColor");
        PhotonNetwork.NickName = PlayerPrefs.GetString("Username");
        PhotonNetwork.LocalPlayer.CustomProperties["ProfilePicture"] = PlayerPrefs.GetString("ProfilePicURL");
        JSAM.AudioManager.SetMasterVolume(PlayerPrefs.GetFloat("mastervolume"));
        JSAM.AudioManager.SetSoundVolume(PlayerPrefs.GetFloat("soundvolume"));
        JSAM.AudioManager.SetMusicVolume(PlayerPrefs.GetFloat("musicvolume"));
        setResolution(PlayerPrefs.GetInt("aspectRatio"));
        setFullsceen(PlayerPrefs.GetInt("fullscreen"));
    }

    void SetUpLocalPlayer()
    {
        isReady = false;
        PhotonNetwork.LocalPlayer.CustomProperties["PlayerColor"] = PlayerPrefs.GetInt("PlayerColor");
        PhotonNetwork.NickName = PlayerPrefs.GetString("Username");
        PhotonNetwork.LocalPlayer.CustomProperties["ProfilePicture"] = PlayerPrefs.GetString("ProfilePicURL");
        StartCoroutine(DownloadProfileImage(PhotonNetwork.LocalPlayer.CustomProperties["ProfilePicture"].ToString()));
        JSAM.AudioManager.SetMasterVolume(PlayerPrefs.GetFloat("mastervolume"));
        JSAM.AudioManager.SetSoundVolume(PlayerPrefs.GetFloat("soundvolume"));
        JSAM.AudioManager.SetMusicVolume(PlayerPrefs.GetFloat("musicvolume"));
        setResolution(PlayerPrefs.GetInt("aspectRatio"));
        setFullsceen(PlayerPrefs.GetInt("fullscreen"));
    }

    public void SetPlayerColor(int choice)
    {
        PlayerPrefs.SetInt("PlayerColor", choice);
        PlayerPrefs.Save();

        PhotonNetwork.LocalPlayer.CustomProperties["PlayerColor"] = PlayerPrefs.GetInt("PlayerColor");
    }

    private void setPlayerUsername(string name)
    {
        PlayerPrefs.SetString("Username", name);
        PlayerPrefs.Save();
        PhotonNetwork.NickName = PlayerPrefs.GetString("Username");
    }

    public void setPlayerUsername(TMP_Text name)
    {
        PlayerPrefs.SetString("Username", name.text.ToString());
        PlayerPrefs.Save();
        PhotonNetwork.NickName = PlayerPrefs.GetString("Username");
    }

    public void setPlayerPicture(string profilePictureURL)
    {
        PlayerPrefs.SetString("ProfilePicURL", profilePictureURL);
        PlayerPrefs.Save();
        PhotonNetwork.LocalPlayer.CustomProperties["ProfilePicture"] = PlayerPrefs.GetString("ProfilePicURL");
        StartCoroutine(DownloadProfileImage(PhotonNetwork.LocalPlayer.CustomProperties["ProfilePicture"].ToString()));
    }

    public void setPlayerPicture(TMP_Text profilePictureURL)
    {
        PlayerPrefs.SetString("ProfilePicURL", profilePictureURL.text.ToString());
        PlayerPrefs.Save();
        PhotonNetwork.LocalPlayer.CustomProperties["ProfilePicture"] = PlayerPrefs.GetString("ProfilePicURL");
        StartCoroutine(DownloadProfileImage(PhotonNetwork.LocalPlayer.CustomProperties["ProfilePicture"].ToString()));
    }

    public void setMap(TMP_Text mapName)
    {
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        hash.Add("Map", mapName.text.ToString());
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }
    public void setTimer(TMP_Text timer)
    {
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();

        Debug.Log("New Time: " + timer.text);
        switch (timer.text)
        {
            case "1 Minute":
                hash.Add("Timer", 60);
                break;
            case "2 Minutes":
                hash.Add("Timer", 120);
                break;
            case "5 Minutes":
                hash.Add("Timer", 300);
                break;
            case "10 Minutes":
                hash.Add("Timer", 600);
                break;
            case "15 Minutes":
                hash.Add("Timer", 900);
                break;
            case "20 Minutes":
                hash.Add("Timer", 1200);
                break;
            case "30 Minutes":
                hash.Add("Timer", 1800);
                break;
            case "60 Minutes":
                hash.Add("Timer", 3600);
                break;
            case "Unlimited":
                hash.Add("Timer", "No Timer");
                break;
        }
        Debug.Log("Post Switch Time: " + timer.text);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public void setGameMode(TMP_Text gameMode)
    {
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        switch (gameMode.text)
        {
            case "Brawl":
                hash.Add("GameMode", "Brawl");
                brawlSettings.SetActive(true);
                sandboxSettings.SetActive(false);
                setMap(brawlSettings.transform.Find("Map Select/Label").GetComponent<TMP_Text>());
                break;
            case "Sandbox":
                hash.Add("GameMode", "Sandbox");
                brawlSettings.SetActive(false);
                sandboxSettings.SetActive(true);
                string[] levels = Directory.GetFiles(Application.dataPath + SaveLoadSandbox.folderPath, "*.json");
                TMP_Dropdown tempDown = sandboxSettings.GetComponentInChildren<TMP_Dropdown>();

                foreach (string s in levels)
                {
                    string[] split = s.Split('\\');
                    tempDown.options.Add(new TMP_Dropdown.OptionData(split[split.Length - 1]));
                }
                setMap(sandboxSettings.transform.Find("Map Select/Label").GetComponent<TMP_Text>());
                break;
        }
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        if (changedProps.ContainsKey("Map") && changedProps["Map"] != PhotonNetwork.LocalPlayer.CustomProperties["Map"]) //Map Update
        {
            Debug.Log(PhotonNetwork.MasterClient.CustomProperties["Map"].ToString());
            hash.Add("Map", PhotonNetwork.MasterClient.CustomProperties["Map"]);
            PhotonNetwork.LocalPlayer.CustomProperties["Map"] = PhotonNetwork.MasterClient.CustomProperties["Map"];
        }
        if (changedProps.ContainsKey("Timer") && changedProps["Timer"] != PhotonNetwork.LocalPlayer.CustomProperties["Timer"]) //Timer Update
        {
            Debug.Log(PhotonNetwork.MasterClient.CustomProperties["Timer"].ToString());
            hash.Add("Timer", PhotonNetwork.MasterClient.CustomProperties["Timer"]);
            PhotonNetwork.LocalPlayer.CustomProperties["Timer"] = PhotonNetwork.MasterClient.CustomProperties["Timer"];
        }
        if (changedProps.ContainsKey("GameMode") && changedProps["GameMode"] != PhotonNetwork.LocalPlayer.CustomProperties["GameMode"]) //GameMode Update
        {
            Debug.Log(PhotonNetwork.MasterClient.CustomProperties["GameMode"].ToString());
            hash.Add("GameMode", PhotonNetwork.MasterClient.CustomProperties["GameMode"]);
            PhotonNetwork.LocalPlayer.CustomProperties["GameMode"] = PhotonNetwork.MasterClient.CustomProperties["GameMode"];
        }
        //PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    IEnumerator DownloadProfileImage(string PFPURL)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(PFPURL);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
            pfpPreview.texture = defaultPFP;
        }
        else
            pfpPreview.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
    }

    public void setResolution(TMP_Dropdown resolution)
    {
        if(resolution.value == 0)
        {
            Screen.SetResolution(1280, 720, PlayerPrefs.GetInt("fullscreen") == 1 ? true : false);
            PlayerPrefs.SetInt("aspectRatio", 0);
            PlayerPrefs.Save();
        }
        if (resolution.value == 1)
        {
            Screen.SetResolution(960, 720, PlayerPrefs.GetInt("fullscreen") == 1 ? true : false);
            PlayerPrefs.SetInt("aspectRatio", 1);
            PlayerPrefs.Save();
        }
    }

    Vector2 getResolution()
    {
        if (PlayerPrefs.GetInt("aspectRatio") == 1)
        {
            return new Vector2(960, 720);
        }
        else
        {
            return new Vector2(1280, 720);
        }
    }

    public void setResolution(int resolution)
    {
        if (resolution == 0)
        {
            Screen.SetResolution(1280, 720, PlayerPrefs.GetInt("fullscreen") == 1 ? true : false);
        }
        if (resolution == 1)
        {
            Screen.SetResolution(960, 720, PlayerPrefs.GetInt("fullscreen") == 1 ? true : false);
        }
    }

    public void setFullsceen(Toggle fullScreen)
    {
        if (fullScreen.isOn)
        {
            Screen.SetResolution((int)getResolution().x, (int)getResolution().y, true);
        }
        else
        {
            Screen.SetResolution((int)getResolution().x, (int)getResolution().y, false);
        }
        PlayerPrefs.SetInt("fullscreen", fullScreen.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void setFullsceen(int fullScreen)
    {
        if (fullScreen == 1)
        {
            Screen.SetResolution((int)getResolution().x, (int)getResolution().y, true);
        }
        else
        {
            Screen.SetResolution((int)getResolution().x, (int)getResolution().y, false);
        }
    }


    public void setMasterVolume(Slider volume)
    {
        JSAM.AudioManager.SetMasterVolume(volume.value);
        PlayerPrefs.SetFloat("mastervolume", volume.value);
        PlayerPrefs.Save();
    }
    public void setMusicVolume(Slider volume)
    {
        JSAM.AudioManager.SetMusicVolume(volume.value);
        PlayerPrefs.SetFloat("musicvolume", volume.value);
        PlayerPrefs.Save();
    }
    public void setSoundVolume(Slider volume)
    {
        JSAM.AudioManager.SetSoundVolume(volume.value);
        PlayerPrefs.SetFloat("soundvolume", volume.value);
        PlayerPrefs.Save();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
        if (newPlayer.CustomProperties.ContainsKey("kills"))
        {
            newPlayer.CustomProperties["kills"] = 0;
        }
        JSAM.AudioManager.PlaySound(Sounds.joinroom);
    }
}
