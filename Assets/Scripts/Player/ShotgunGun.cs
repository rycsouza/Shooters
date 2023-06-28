using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class ShotgunGun : Gun
{
    [Header("Shotgun Setup")]
    [SerializeField] ShotAreaHandler _shotArea;

    public override float Shoot(Camera _cam, GameObject _playerHitImpact, GameObject _bulletPrefab)
    {
        CallMuzzleFlash();

        foreach (PhotonView player in _shotArea.PlayersDetected)
        {
            PhotonNetwork.Instantiate(_playerHitImpact.name, player.transform.position, Quaternion.identity);
            player.RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName, ShotDamage, PhotonNetwork.LocalPlayer.ActorNumber);
        }

        ShotAudio.Stop();
        ShotAudio.Play();

        return TimeBetweenShot;
    }
}
