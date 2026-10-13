using UnityEngine;
using UnityEngine.UI;

namespace HikingMinigame
{
    public class StatsHUD : MonoBehaviour
    {
        public Image staminaFill;
        public Image natureFill;

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
            var gm = HikingGameManager.Instance;
            staminaFill.fillAmount = (float)gm.Stamina / gm.maxStamina;
            natureFill.fillAmount = (float)gm.NatureScore / gm.maxNatureScore;
        }
    }
}