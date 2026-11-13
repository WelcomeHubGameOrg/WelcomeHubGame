using System;
using UnityEngine;

public class Barcode : MonoBehaviour
{
    public Scannable Owner { get; private set; }
    private void Awake()
    {
        Owner = GetComponentInParent<Scannable>();
    }
}
