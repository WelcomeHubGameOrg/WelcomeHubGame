using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ScanManager : MonoBehaviour
{
    [SerializeField] List<Transform> ScanPoints = new List<Transform>();
    [SerializeField] private GameObject currentScanPoint;
    [SerializeField] private SelfCheckoutGame game;

    private readonly float[] _rotations = { 0f, 90f, 180f, 270f };

    private void Start()
    {
        currentScanPoint = ScanPoints[0].gameObject;
    }

    private void OnEnable()
    {
        game.ScannedItem += OnProductScanned;
    }

    private void OnDisable()
    {
        game.ScannedItem -= OnProductScanned;
    }

    private void OnProductScanned(int price, string productName)
    {
        currentScanPoint.SetActive(false);
        currentScanPoint.transform.rotation = Quaternion.identity;
        currentScanPoint = ScanPoints[(Random.Range(0, ScanPoints.Count))].gameObject;
        currentScanPoint.transform.Rotate(0f, 0f, _rotations[Random.Range(0, _rotations.Length)]);
        currentScanPoint.SetActive(true);

        decimal priceEuros = price / 100m;
        Debug.Log($"Product scanned: {productName}, price: {priceEuros}€");
    }
}
