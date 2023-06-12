using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeadeboardPlayer : MonoBehaviour
{
    [SerializeField] private TMP_Text _playerName, _kills, _deaths;

    public void SetDetails(string name, int kills, int deaths)
    {
        _playerName.text = name;
        _kills.text = kills.ToString();
        _deaths.text = deaths.ToString();
    }
}
