using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;

public class Launcher : MonoBehaviourPunCallbacks
{
    #region Variables
    public static Launcher Instance;

    [SerializeField] private GameObject _loadingScreen, _menuScreen, _createRoomScreen, _roomScreen, _errorScreen, _roomBrowserScreen, _nicknameScreen;
    [SerializeField] private Button _startButton, _leaveRoom, _testButton;
    [SerializeField] private RoomButton _roomButton;
    [SerializeField] private TMP_Text _loadText, _roomNameText, _errorText, _playerNameLabel;
    [SerializeField] private TMP_InputField _roomNameInput, _nicknameInput;
    [SerializeField] private string _versao;
    private List<RoomButton> _allRoomButtons = new List<RoomButton>();
    private List<TMP_Text> _allPlayersNames = new List<TMP_Text>();
    public static bool hasSetNickname;

    public string[] AllMaps;
    public bool ChangeMapBetweenRounds = true;
    #endregion

    #region Native Functions
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        PhotonNetwork.GameVersion = _versao;

        CloseMenus();
        _loadingScreen.SetActive(true);
        _loadText.text = "Conectando ao Servidor...";

        if (!PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.GameVersion != _versao) _loadText.text = "Versão Desatualizada! Atualize para a versão mais recente (" + _versao + ")";
            else PhotonNetwork.ConnectUsingSettings();
        }

#if UNITY_EDITOR
        _testButton.gameObject.SetActive(true);
#endif

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    #endregion

    #region Override Functions
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();

        PhotonNetwork.AutomaticallySyncScene = true;

        _loadText.text = "Conectado!";
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        _menuScreen.SetActive(true);

        if (!hasSetNickname)
        {
            CloseMenus();
            _nicknameScreen.SetActive(true);

            if (PlayerPrefs.HasKey("playerName"))
            {
                _nicknameInput.text = PlayerPrefs.GetString("playerName");
            }
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
        }
    }

    public override void OnJoinedRoom()
    {
        CloseMenus();
        _roomScreen.SetActive(true);

        _roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        ListAllPlayers();

        if (PhotonNetwork.IsMasterClient)
        {
            _startButton.gameObject.SetActive(true);
        }
        else
        {
            _startButton.gameObject.SetActive(false);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        _errorText.text = "Falha ao Criar Sala: " + message;
        CloseMenus();
        _errorScreen.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        CloseMenus();
        _menuScreen.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomButton room in _allRoomButtons)
        {
            Destroy(room.gameObject);
        }
        _allRoomButtons.Clear();

        _roomButton.gameObject.SetActive(false);

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButton newButton = Instantiate(_roomButton, _roomButton.transform.parent);
                newButton.SetButtonDetails(roomList[i]);
                newButton.gameObject.SetActive(true);

                _allRoomButtons.Add(newButton);
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TMP_Text newPlayerLabel = Instantiate(_playerNameLabel, _playerNameLabel.transform.parent);
        newPlayerLabel.text = newPlayer.NickName;
        newPlayerLabel.gameObject.SetActive(true);

        _allPlayersNames.Add(newPlayerLabel);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListAllPlayers();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _startButton.gameObject.SetActive(true);
        }
        else
        {
            _startButton.gameObject.SetActive(false);
        }
    }

    #endregion

    public void OpenRoomCreate()
    {
        CloseMenus();
        _createRoomScreen.SetActive(true);
    }

    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(_roomNameInput.text))
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 20;

            PhotonNetwork.CreateRoom(_roomNameInput.text, options);

            CloseMenus();
            _loadText.text = "Criando Sala...";
            _loadingScreen.SetActive(true);
        }
    }

    public void CloseErrorScreen()
    {
        CloseMenus();
        _menuScreen.SetActive(true);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
        _loadText.text = "Saindo da Sala";
        _loadingScreen.SetActive(true);
    }

    public void OpenRoomBrowser()
    {
        CloseMenus();
        _roomBrowserScreen.SetActive(true);
    }

    public void CloseRoomBrowser()
    {
        CloseMenus();
        _menuScreen.SetActive(true);
    }

    public void JoinRoom(RoomInfo inputInfo)
    {
        PhotonNetwork.JoinRoom(inputInfo.Name);

        CloseMenus();
        _loadText.text = "Entrando na Sala...";
        _loadingScreen.SetActive(true);
    }

    public void LeaveGame()
    {
        Application.Quit();
    }

    private void ListAllPlayers()
    {
        foreach (TMP_Text player in _allPlayersNames)
        {
            Destroy(player.gameObject);
        }
        _allPlayersNames.Clear();

        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < players.Length; i++)
        {
            TMP_Text newPlayerLabel = Instantiate(_playerNameLabel, _playerNameLabel.transform.parent);
            newPlayerLabel.text = players[i].NickName;
            newPlayerLabel.gameObject.SetActive(true);

            _allPlayersNames.Add(newPlayerLabel);
        }
    }

    public void SetNickname()
    {
        if (!string.IsNullOrEmpty(_nicknameInput.text))
        {
            PhotonNetwork.NickName = _nicknameInput.text;

            CloseMenus();
            _menuScreen.SetActive(true);

            PlayerPrefs.SetString("playerName", _nicknameInput.text);

            hasSetNickname = true;
        }
    }

    public void StartGame()
    {
        _startButton.enabled = false;
        _leaveRoom.enabled = false;
        PhotonNetwork.LoadLevel(AllMaps[Random.Range(0, AllMaps.Length)]);
    }

    void CloseMenus()
    {
        _loadingScreen.SetActive(false);
        _menuScreen.SetActive(false);
        _createRoomScreen.SetActive(false);
        _roomScreen.SetActive(false);
        _errorScreen.SetActive(false);
        _roomBrowserScreen.SetActive(false);
        _nicknameScreen.SetActive(false);
    }

    public void QuickJoin()
    {
        PhotonNetwork.CreateRoom("TestRoom");
        CloseMenus();
        _loadText.text = "Criando Sala...";
        _loadingScreen.SetActive(true);
    }

}
