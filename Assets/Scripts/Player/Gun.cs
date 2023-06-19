using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject MuzzleFlash;
    public float TimeBetweenShot = .1f, HeatPerShot = 1f;
    public bool IsAutomatic, HasCrosshair;
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
        ShotDamage = _gunObject.ShotDamage;
        AdsZoom = _gunObject.AdsZoom;
    }

    public void CallMuzzleFlash()
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
