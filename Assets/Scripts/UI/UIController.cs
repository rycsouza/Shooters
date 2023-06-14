using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    public LeadeboardPlayer LeadeboardPlayerDisplay;
    
    public List<Sprite> CrosshairList;

    [Header("GameObject")]
    public GameObject Crosshair;
    public GameObject ConfigPanel;
    public GameObject DeathScreen;
    public GameObject Leaderboard;
    public GameObject EndGameScreen;

    [Header("Slider")]
    public Slider WeaponSlider;
    public Slider HealthSlider;
    public Slider MouseSensitivity;
    public Slider AudioVolume;

    [Header("Text")]
    public TMP_Text HealthValue;
    public TMP_Text DeathText;
    public TMP_Text Kills, Deaths;
    public TMP_Text Timer;
    public TMP_Text MouseSensitivyValue;
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
            Cursor.visible = false;
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
