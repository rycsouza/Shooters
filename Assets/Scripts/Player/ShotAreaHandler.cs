using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotAreaHandler : MonoBehaviour
{
    public List<PhotonView> PlayersDetected
    {
        get
        {
            // Limpa a lista antes de retornar porque quando o inimigo morre, não executa o OnTriggerExit
            _playersDetected = _playersDetected.FindAll((player) => player != null);
            return _playersDetected;
        }
    }

    [Header("Debug")]
    [SerializeField] private List<PhotonView> _playersDetected;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PhotonView player = other.gameObject.GetPhotonView();
            if (!player.IsMine)
            {
                _playersDetected.Add(player);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            _playersDetected.Remove(other.gameObject.GetPhotonView());
    }
}
