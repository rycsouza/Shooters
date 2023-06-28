using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "Gun", menuName = "Gun/GunObject")]
public class GunObject : ScriptableObject
{
    public float TimeBetweenShot, HeatPerShot;
    public bool IsAutomatic, HasCrosshair;
    public Sprite Crosshair;
    public int ShotDamage;
    public float AdsZoom;
}
