using System.Linq;
using HikingMinigame.Items;
using TMPro;
using UnityEngine;

namespace HikingMinigame
{
    public class InventoryHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text _itemsText;

        private Inventory _inventory;

        private void Start()
        {
            _inventory = HikingGameManager.Instance.Inventory;
            _inventory.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_inventory != null)
                _inventory.Changed -= Refresh;
        }

        private void Refresh()
        {
            _itemsText.text = string.Join("\n", _inventory.Items.Select(item => item.DisplayName));
        }
    }
}
