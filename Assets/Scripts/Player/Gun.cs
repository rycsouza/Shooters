using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject MuzzleFlash;
    public float TimeBetweenShot, HeatPerShot;
    public bool IsAutomatic;
    public int ShotDamage;
    public float AdsZoom;
    public AudioSource ShotAudio;
    public float HeatCounter;
    public float LastHeatCounter;
    public Transform ShootRange;

    [SerializeField] private GunObject _gunObject;

    private void Start()
    {
        TimeBetweenShot = _gunObject.TimeBetweenShot;
        HeatPerShot = _gunObject.HeatPerShot;
        IsAutomatic = _gunObject.IsAutomatic;
        ShotDamage = _gunObject.ShotDamage;
        AdsZoom = _gunObject.AdsZoom;
    }

    private void Update()
    {
        UIController.Instance.Crosshair.SetActive(_gunObject.HasCrosshair);
    }

    public void CallMuzzleFlash()
    {
        if(!MuzzleFlash.activeInHierarchy) StartCoroutine(MuzzleFlashCourotine());
    }

    private IEnumerator MuzzleFlashCourotine()
    {
        MuzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.01f);
        MuzzleFlash.SetActive(false);
    }
}
