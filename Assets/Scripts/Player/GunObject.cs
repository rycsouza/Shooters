using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "Gun", menuName = "Gun/NewGun")]
public class GunObject : ScriptableObject
{
    public float TimeBetweenShot, HeatPerShot;
    public bool IsAutomatic;
    public int ShotDamage;
    public float AdsZoom;
    public bool HasCrosshair;
}
