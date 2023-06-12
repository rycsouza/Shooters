using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Realtime;

public class RoomButton : MonoBehaviour
{
    [SerializeField] private TMP_Text _buttonText;

    private RoomInfo _info;

    public void SetButtonDetails(RoomInfo InputInfo)
    {
        _info = InputInfo;

        _buttonText.text = _info.Name;
    }

    public void OpenRoom()
    {
        Launcher.Instance.JoinRoom(_info);
    }
}
