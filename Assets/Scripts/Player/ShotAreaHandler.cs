using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotAreaHandler : MonoBehaviour
{
    public List<PhotonView> PlayersDetected => _playersDetected;

    [Header("Debug")]
    [SerializeField] private List<PhotonView> _playersDetected;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            _playersDetected.Add(other.gameObject.GetPhotonView());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            _playersDetected.Remove(other.gameObject.GetPhotonView());
    }
}
