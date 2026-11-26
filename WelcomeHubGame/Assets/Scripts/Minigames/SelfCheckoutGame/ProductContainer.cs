using TMPro;
using UnityEngine;

public class ProductContainer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI productText;
    [SerializeField] private TextMeshProUGUI priceText;

    public void SetValues(int productPrice, string productName)
    {
        decimal price = productPrice / 100m;

        productText.SetText(productName);
        priceText.SetText(price + " €");
    }
}
