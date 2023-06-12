using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    public GameObject Crosshair;
    public Slider WeaponSlider;

    public Slider HealthSlider;
    public TMP_Text HealthValue;

    public GameObject DeathScreen;
    public TMP_Text DeathText;

    public List<Sprite> CrosshairList;

    public TMP_Text Kills, Deaths;

    public GameObject Leaderboard;
    public LeadeboardPlayer LeadeboardPlayerDisplay;

    public GameObject EndGameScreen;

    public TMP_Text Timer;

    public GameObject ConfigPanel;

    public Slider MouseSensitivity;
    public TMP_Text MouseSensitivyValue;
    public Slider AudioVolume;
    public TMP_Text AudioVolumeValue;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowOrHidePanel();
        }

        if (ConfigPanel.activeInHierarchy && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ShowOrHidePanel()
    {
        if (!ConfigPanel.activeInHierarchy)
        {
            ConfigPanel.SetActive(true);
        }
        else
        {
            ConfigPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void ReturnToMainMenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
