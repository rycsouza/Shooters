using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviourPunCallbacks
{
    public GameObject MuzzleFlash;
    public float TimeBetweenShot = .1f, HeatPerShot = 1f;
    public bool IsAutomatic, HasCrosshair;
    public Sprite Crosshair;
    public int ShotDamage;
    public float AdsZoom;
    public AudioSource ShotAudio;
    public float HeatCounter;
    public float LastHeatCounter;

    [SerializeField] private GunObject _gunObject;

    private void Start()
    {
        TimeBetweenShot = _gunObject.TimeBetweenShot;
        HeatPerShot = _gunObject.HeatPerShot;
        IsAutomatic = _gunObject.IsAutomatic;
        HasCrosshair = _gunObject.HasCrosshair;
        Crosshair = _gunObject.Crosshair;
        ShotDamage = _gunObject.ShotDamage;
        AdsZoom = _gunObject.AdsZoom;
    }

    public virtual float Shoot(Camera _cam, GameObject _playerHitImpact, GameObject _bulletPrefab)
    {
        CallMuzzleFlash();

        Ray ray = _cam.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        ray.origin = _cam.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                PhotonNetwork.Instantiate(_playerHitImpact.name, hit.point, Quaternion.identity);

                hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName, ShotDamage, PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {
                GameObject bulletImpact = Instantiate(_bulletPrefab, hit.point + (hit.normal * .002f), Quaternion.LookRotation(Vector3.forward, Vector3.up));

                Destroy(bulletImpact, 5f);
            }
        }

        ShotAudio.Stop();
        ShotAudio.Play();

        return TimeBetweenShot;
    }

    protected void CallMuzzleFlash()
    {
        StartCoroutine(CouMuzzleFlash());
    }

    private IEnumerator CouMuzzleFlash()
    {
        MuzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.01f);
        MuzzleFlash.SetActive(false);
    }
}
