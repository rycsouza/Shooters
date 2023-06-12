using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject MuzzleFlash;
    public float TimeBetweenShot = .1f, HeatPerShot = 1f;
    public bool IsAutomatic;
    public int ShotDamage;
    public float AdsZoom;
    public AudioSource ShotAudio;
    public float HeatCounter;
    public float LastHeatCounter;
}
