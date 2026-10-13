using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HikingMinigame
{
    public class HikingGameManager : MonoBehaviour
    {
        public static HikingGameManager Instance { get; private set; }

        
        
        public int Stamina { get; private set; } = 100;
        public int maxStamina = 100;

        public int NatureScore { get; private set; } = 50;
        public int maxNatureScore = 100;

        public Action StatsChanged;
        
        private bool isGameOver = false;
        
        
        public int debugStep = 10;

        private void Update()
        {
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                ModifyStamina(-debugStep);
                ModifyNatureScore(-debugStep);
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                ModifyStamina(debugStep);
                ModifyNatureScore(debugStep);
            }
        }
        
        
        
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
            Stamina = Mathf.Clamp(Stamina + amount, 0, maxStamina);
            StatsChanged?.Invoke();
            
            if (Stamina <= 0)
            {
                CheckStaminaGameOver();
            }
        }

        public void ModifyNatureScore(int amount)
        {
            NatureScore = Mathf.Clamp(NatureScore + amount, 0, maxNatureScore);
            StatsChanged?.Invoke();
        }

        public void ResetStats()
        {
            Stamina = 50;
            NatureScore = 50;
        }
        
        private void CheckStaminaGameOver()
        {
            // placeholder -- item-based prevention, restore-on-empty rules,
            // and actual game over logic go here once designed
            if(Stamina <= 0) isGameOver = true;
            
            Debug.Log("Stamina hit zero -- game over check goes here.");
        }
    }
}