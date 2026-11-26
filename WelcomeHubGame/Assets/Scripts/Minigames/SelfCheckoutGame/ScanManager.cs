using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class ScanManager : MonoBehaviour
{
    [SerializeField] List<Transform> ScanPoints = new List<Transform>();
    [SerializeField] private GameObject productContainer;
    [SerializeField] private GameObject scannerContent;
    [SerializeField] private SelfCheckoutGame game;
    private GameObject _currentScanPoint;
    
    private readonly float[] _rotations = { 0f, 90f, 180f, 270f };

    private void Start()
    {
        _currentScanPoint = ScanPoints[0].gameObject;
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
        _currentScanPoint.SetActive(false);
        _currentScanPoint.transform.rotation = Quaternion.identity;
        _currentScanPoint = ScanPoints[(Random.Range(0, ScanPoints.Count))].gameObject;
        _currentScanPoint.transform.Rotate(0f, 0f, _rotations[Random.Range(0, _rotations.Length)]);
        _currentScanPoint.SetActive(true);

        GameObject scannedProductInfo = Instantiate(productContainer, scannerContent.transform);
        scannedProductInfo.GetComponent<ProductContainer>().SetValues(price, productName);
        
    }
}
