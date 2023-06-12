using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static MatchManager Instance;

    #region Variables
    public enum EventCodes : byte
    {
        NewPlayer,
        ListPlayers,
        ChangeStat,
        NextMatch,
        TimerSync
    }

    public enum GameStates
    {
        Waiting,
        Playing,
        Ending
    }

    public GameStates State = GameStates.Waiting;

    public Transform MapCamPoint;
    public float waitAfterEnding = 5f;
    public float MatchLenght = 300f;
    public int KillToWin = 3;
    public bool Perpetual;

    [SerializeField] private List<PlayerInfo> _allPlayers = new List<PlayerInfo>();
    private List<LeadeboardPlayer> lboardPlayers = new List<LeadeboardPlayer>();

    private int index;
    private float _currentMatchTime;
    private float _sendTimer;
    #endregion

    #region Native Functions
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("Menu_SCN");
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);

            State = GameStates.Playing;
        }

        SetupTimer();

        if (!PhotonNetwork.IsMasterClient)
        {
            UIController.Instance.Timer.gameObject.SetActive(false);
        }

        UIController.Instance.AudioVolume.value = AudioListener.volume;
        UIController.Instance.AudioVolumeValue.text = UIController.Instance.AudioVolume.value.ToString("0.0");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && State != GameStates.Ending)
        {
            if (UIController.Instance.Leaderboard.activeInHierarchy)
            {
                UIController.Instance.Leaderboard.SetActive(false);
            }
            else
            {
                ShowLeaderboard();
            }
        }
        if (PhotonNetwork.IsMasterClient)
        {
            if (_currentMatchTime > 0f && State == GameStates.Playing)
            {
                _currentMatchTime -= Time.deltaTime;

                UpdateTimerDisplay();

                _sendTimer -= Time.deltaTime;

                if (_sendTimer <= 0f)
                {
                    _sendTimer += 1f;

                    TimerSend();
                }
            }
            else if (_currentMatchTime <= 0f && State == GameStates.Playing)
            {
                _currentMatchTime = 0f;

                State = GameStates.Ending;

                ListPlayersSend();

                StateCheck();
            }
        }

        AudioListener.volume = UIController.Instance.AudioVolume.value;
        UIController.Instance.AudioVolumeValue.text = AudioListener.volume.ToString("0.0");

        if (UIController.Instance.AudioVolume.value == 0f)
        {
            AudioListener.pause = true;
        }
        else
        {
            AudioListener.pause = false;
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code < 200)
        {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            switch (theEvent)
            {
                case EventCodes.NewPlayer:
                    NewPlayerRecive(data);
                    break;
                case EventCodes.ListPlayers:
                    ListPlayersRecive(data);
                    break;
                case EventCodes.ChangeStat:
                    UpdateStatsRecive(data);
                    break;
                case EventCodes.NextMatch:
                    NextMatchReceive();
                    break;
                case EventCodes.TimerSync:
                    TimerReceive(data);
                    break;
            }
        }
    }
    #endregion

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void NewPlayerSend(string userName)
    {
        object[] package = new object[4];
        package[0] = userName;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
            );
    }

    public void NewPlayerRecive(object[] dataRecived)
    {
        PlayerInfo player = new PlayerInfo((string)dataRecived[0], (int)dataRecived[1], (int)dataRecived[2], (int)dataRecived[3]);

        _allPlayers.Add(player);

        ListPlayersSend();
    }

    public void ListPlayersSend()
    {
        object[] package = new object[_allPlayers.Count + 1];

        package[0] = State;

        for (int i = 0; i < _allPlayers.Count; i++)
        {
            object[] piece = new object[4];

            piece[0] = _allPlayers[i].name;
            piece[1] = _allPlayers[i].actor;
            piece[2] = _allPlayers[i].kills;
            piece[3] = _allPlayers[i].deaths;

            package[i + 1] = piece;
        }

        PhotonNetwork.RaiseEvent(
           (byte)EventCodes.ListPlayers,
           package,
           new RaiseEventOptions { Receivers = ReceiverGroup.All },
           new SendOptions { Reliability = true }
           );
    }

    public void ListPlayersRecive(object[] dataRecived)
    {
        _allPlayers.Clear();

        State = (GameStates)dataRecived[0];

        for (int i = 1; i < dataRecived.Length; i++)
        {
            object[] piece = (object[])dataRecived[i];

            PlayerInfo player = new PlayerInfo(
                (string)piece[0],
                (int)piece[1],
                (int)piece[2],
                (int)piece[3]
                );
            _allPlayers.Add(player);

            if (PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                index = i - 1;
            }
        }

        StateCheck();
    }

    public void UpdateStatsSend(int actorSending, int statToUpdate, int amountToChange)
    {
        object[] package = new object[] { actorSending, statToUpdate, amountToChange };

        PhotonNetwork.RaiseEvent(
           (byte)EventCodes.ChangeStat,
           package,
           new RaiseEventOptions { Receivers = ReceiverGroup.All },
           new SendOptions { Reliability = true }
           );
    }

    public void UpdateStatsRecive(object[] dataRecived)
    {
        int actor = (int)dataRecived[0];
        int statType = (int)dataRecived[1];
        int amount = (int)dataRecived[2];

        for (int i = 0; i < _allPlayers.Count; i++)
        {
            if (_allPlayers[i].actor == actor)
            {
                switch (statType)
                {
                    case 0: //kills
                        _allPlayers[i].kills += amount;
                        break;
                    case 1: //Death
                        _allPlayers[i].deaths += amount;
                        break;
                }

                if (i == index)
                {
                    UpdateStatsDisplay();
                }

                if (UIController.Instance.Leaderboard.activeInHierarchy)
                {
                    ShowLeaderboard();
                }

                break;
            }
        }

        ScoreCheck();
    }

    public void UpdateStatsDisplay()
    {
        if (_allPlayers.Count > index)
        {
            UIController.Instance.Kills.text = "Kills: " + _allPlayers[index].kills;
            UIController.Instance.Deaths.text = "Deaths: " + _allPlayers[index].deaths;
        }
        else
        {
            UIController.Instance.Kills.text = "Kills: " + 0;
            UIController.Instance.Deaths.text = "Deaths: " + 0;
        }
    }

    void ShowLeaderboard()
    {
        foreach (LeadeboardPlayer lp in lboardPlayers)
        {
            Destroy(lp.gameObject);
        }
        lboardPlayers.Clear();

        UIController.Instance.LeadeboardPlayerDisplay.gameObject.SetActive(false);

        List<PlayerInfo> sorted = SortPlayers(_allPlayers);

        foreach (PlayerInfo player in sorted)
        {
            LeadeboardPlayer newPlayerDisplay = Instantiate(UIController.Instance.LeadeboardPlayerDisplay, UIController.Instance.LeadeboardPlayerDisplay.transform.parent);

            newPlayerDisplay.SetDetails(player.name, player.kills, player.deaths);

            newPlayerDisplay.gameObject.SetActive(true);

            lboardPlayers.Add(newPlayerDisplay);
        }
        UIController.Instance.Leaderboard.SetActive(true);
    }

    private List<PlayerInfo> SortPlayers(List<PlayerInfo> players)
    {
        List<PlayerInfo> sorted = new List<PlayerInfo>();

        while (sorted.Count < players.Count)
        {
            int heighest = -1;
            PlayerInfo selectedPlayer = players[0];

            foreach (PlayerInfo player in players)
            {
                if (!sorted.Contains(player))
                {
                    if (player.kills > heighest)
                    {
                        selectedPlayer = player;
                        heighest = player.kills;
                    }
                }
            }

            sorted.Add(selectedPlayer);
        }

        return sorted;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        foreach (PlayerInfo player in _allPlayers)
        {
            if (player.name == otherPlayer.NickName)
            {
                _allPlayers.Remove(player);
                PhotonNetwork.DestroyPlayerObjects(otherPlayer);
            }
        }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene("Menu_SCN");
    }

    void ScoreCheck()
    {
        bool winnerFound = false;

        foreach (PlayerInfo player in _allPlayers)
        {
            if (player.kills >= KillToWin && KillToWin > 0)
            {
                winnerFound = true;
                break;
            }
        }

        if (winnerFound)
        {
            if (PhotonNetwork.IsMasterClient && State != GameStates.Ending)
            {
                State = GameStates.Ending;
                ListPlayersSend();
            }
        }
    }

    void StateCheck()
    {
        if (State == GameStates.Ending)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        State = GameStates.Ending;

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }

        UIController.Instance.EndGameScreen.SetActive(true);
        ShowLeaderboard();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Camera.main.transform.SetPositionAndRotation(MapCamPoint.position, MapCamPoint.rotation);

        StartCoroutine(EndCoroutine());
    }

    private IEnumerator EndCoroutine()
    {
        yield return new WaitForSeconds(waitAfterEnding);

        if (!Perpetual)
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (!Launcher.Instance.ChangeMapBetweenRounds)
                {
                    NextMatchSend();
                }
                else
                {
                    int newLevel = Random.Range(0, Launcher.Instance.AllMaps.Length);

                    if (Launcher.Instance.AllMaps[newLevel] == SceneManager.GetActiveScene().name)
                    {
                        NextMatchSend();
                    }
                    else
                    {
                        PhotonNetwork.LoadLevel(Launcher.Instance.AllMaps[newLevel]);
                    }
                }
            }
        }
    }

    public void NextMatchSend()
    {
        PhotonNetwork.RaiseEvent(
          (byte)EventCodes.NextMatch,
          null,
          new RaiseEventOptions { Receivers = ReceiverGroup.All },
          new SendOptions { Reliability = true }
          );
    }

    public void NextMatchReceive()
    {
        State = GameStates.Playing;

        UIController.Instance.EndGameScreen.SetActive(false);
        UIController.Instance.Leaderboard.SetActive(false);

        foreach (PlayerInfo player in _allPlayers)
        {
            player.kills = 0;
            player.deaths = 0;
        }

        SetupTimer();

        PlayerSpawner.Instance.SpawnPlayer();

    }

    public void SetupTimer()
    {
        if (MatchLenght > 0)
        {
            _currentMatchTime = MatchLenght;
            UpdateTimerDisplay();
        }
    }

    public void UpdateTimerDisplay()
    {
        var timeToDisplay = System.TimeSpan.FromSeconds(_currentMatchTime);

        UIController.Instance.Timer.text = timeToDisplay.Minutes.ToString("00") + ":" + timeToDisplay.Seconds.ToString("00");
    }

    public void TimerSend()
    {
        object[] package = new object[] { (int)_currentMatchTime, State };

        PhotonNetwork.RaiseEvent(
          (byte)EventCodes.TimerSync,
          package,
          new RaiseEventOptions { Receivers = ReceiverGroup.All },
          new SendOptions { Reliability = true }
          );
    }

    public void TimerReceive(object[] dataReceived)
    {
        _currentMatchTime = (int)dataReceived[0];
        State = (GameStates)dataReceived[1];

        UpdateTimerDisplay();

        UIController.Instance.Timer.gameObject.SetActive(true);
    }
}
[System.Serializable]
public class PlayerInfo
{
    public string name;
    public int actor, kills, deaths;

    public PlayerInfo(string _name, int _actor, int _kills, int _deaths)
    {
        name = _name;
        actor = _actor;
        kills = _kills;
        deaths = _deaths;
    }
}