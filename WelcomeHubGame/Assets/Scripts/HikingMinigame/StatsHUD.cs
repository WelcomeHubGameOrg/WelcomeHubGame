using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace HikingMinigame
{
    public class StatsHUD : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("staminaFill")] private Image _staminaFill;
        [SerializeField, FormerlySerializedAs("natureFill")] private Image _natureFill;

        private void Start()
        {
            HikingGameManager.Instance.StatsChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (HikingGameManager.Instance != null)
                HikingGameManager.Instance.StatsChanged -= Refresh;
        }

        private void Refresh()
        {
            var game = HikingGameManager.Instance;
            _staminaFill.fillAmount = (float)game.Stamina / game.MaxStamina;
            _natureFill.fillAmount = (float)game.NatureScore / game.MaxNatureScore;
        }
    }
}
