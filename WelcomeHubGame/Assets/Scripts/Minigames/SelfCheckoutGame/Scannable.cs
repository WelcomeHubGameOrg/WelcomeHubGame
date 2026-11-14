using UnityEngine;
using Random = UnityEngine.Random;

public class Scannable : MonoBehaviour
{
    private bool _hasScanned = false;
    public bool _inContact = false;
    private float _timeRequired;
    private float _timer;

    // assume that price is in cents
    [SerializeField] private int price;
    [SerializeField] private string productName;

    [SerializeField] SelfCheckoutGame game;

    private void Awake()
    {
        game = FindFirstObjectByType<SelfCheckoutGame>();
        _timeRequired = Random.Range(0.5f, 3.0f);
    }

    void Update()
    {
        if (_inContact && !_hasScanned)
        {
            _timer += Time.deltaTime;

            if (_timer >= _timeRequired)
            {
                _hasScanned = true;
                _inContact = false;
                Debug.Log("Scanned object successfully");
                if (game) game.NotifyScannedItem(this.price, this.productName);
            }
        }
    }

    public void StartTimer()
    {
        if (_hasScanned) return;
        
        _timer = 0.0f;
        _inContact = true;
    }

    public void StopTimer()
    {
        if (_hasScanned) return;
        
        _timer = 0.0f;
        _inContact = false;
    }
    
}
