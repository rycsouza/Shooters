using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOverTime : MonoBehaviour
{
    public float Lifetime = 1.5f;

    void Start()
    {
        Destroy(gameObject, Lifetime * Time.deltaTime);
    }
}
