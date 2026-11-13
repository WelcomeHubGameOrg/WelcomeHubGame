using System;
using UnityEngine;

public class Scanner : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out Barcode barcode))
        {
            barcode.Owner.StartTimer();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out Barcode barcode))
        {
            barcode.Owner.StopTimer();
        }
    }
}
