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

        if (_shotArea.PlayersDetected.Count == 0)
        {
            Ray ray = _cam.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
            ray.origin = _cam.transform.position;

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (!hit.collider.gameObject.CompareTag("Player"))
                {
                    GameObject bulletImpact = Instantiate(_bulletPrefab, hit.point + (hit.normal * .002f), Quaternion.LookRotation(Vector3.forward, Vector3.up));
                    Destroy(bulletImpact, 5f);
                }
            }
        }

        ShotAudio.Stop();
        ShotAudio.Play();

        return TimeBetweenShot;
    }
}
