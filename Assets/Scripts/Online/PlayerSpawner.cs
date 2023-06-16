using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance;

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _deathEffect;
    [SerializeField] private PowerUp[] _allDrops;

    private GameObject _player;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnected) SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint();

        _player = PhotonNetwork.Instantiate(_playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }

    public void Die(string damager)
    {
        UIController.Instance.DeathText.text = "Você foi morto por: " + damager;

        MatchManager.Instance.UpdateStatsSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);

        if (_player != null) StartCoroutine(DieCoroutine());
    }

    public IEnumerator DieCoroutine()
    {
        PhotonNetwork.Instantiate(_deathEffect.name, _player.transform.position, Quaternion.identity);
        PhotonNetwork.Instantiate(_allDrops[Random.Range(0, _allDrops.Length)].name, _player.transform.position, _player.transform.rotation);

        PhotonNetwork.Destroy(_player);
        _player = null;
        UIController.Instance.DeathScreen.SetActive(true);

        yield return new WaitForSeconds(5f);

        UIController.Instance.DeathScreen.SetActive(false);

        if(MatchManager.Instance.State == MatchManager.GameStates.Playing && _player == null) SpawnPlayer();
    }
}
