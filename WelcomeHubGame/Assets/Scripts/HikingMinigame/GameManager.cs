using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int stamina = 100;
    public int maxStamina = 100;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ModifyStamina(int amount)
    {
        stamina = Mathf.Clamp(stamina + amount, 0, maxStamina);

        if (stamina <= 0)
        {
            CheckStaminaGameOver();
        }
    }

    private void CheckStaminaGameOver()
    {
        // placeholder -- item-based prevention, restore-on-empty rules,
        // and actual game over logic go here once designed
        Debug.Log("Stamina hit zero -- game over check goes here.");
    }
}