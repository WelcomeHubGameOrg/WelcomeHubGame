using System;
using HikingMinigame.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace HikingMinigame
{
    public class HikingGameManager : MinigameController
    {
        [SerializeField, FormerlySerializedAs("maxStamina")] private int _maxStamina = 100;
        [SerializeField, FormerlySerializedAs("maxNatureScore")] private int _maxNatureScore = 100;
        [SerializeField] private int _startingNatureScore = 50;
        [SerializeField] private int _inventoryCapacity = 3;
        [SerializeField] private StaminaDrainCurve _staminaDrain = new();
        [SerializeField] private HikeScoring _scoring = new();

        public static HikingGameManager Instance { get; private set; }

        public event Action StatsChanged;
        public event Action GameOver;

        public float Stamina { get; private set; }
        public int MaxStamina => _maxStamina;
        public int NatureScore { get; private set; }
        public int MaxNatureScore => _maxNatureScore;
        public float ElapsedSeconds { get; private set; }
        public bool IsGameOver { get; private set; }
        public Inventory Inventory { get; private set; }
        public HikeScore Score => _scoring.Calculate(ElapsedSeconds, NatureScore, _maxNatureScore);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Stamina = _maxStamina;
            NatureScore = _startingNatureScore;
            Inventory = new Inventory(_inventoryCapacity);
        }

        private void Update()
        {
            if (IsGameOver)
                return;

            ElapsedSeconds += Time.deltaTime;
            Drain(_staminaDrain.RateAt(ElapsedSeconds) * Time.deltaTime);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public StaminaChange ChangeStamina(int amount, HazardType hazard)
        {
            if (IsGameOver)
                return default;

            if (amount < 0)
            {
                var protection = Inventory.FindProtectionAgainst(hazard);
                if (protection != null)
                {
                    Inventory.Use(protection);
                    return new StaminaChange(0, protection, null);
                }
            }

            var previous = Stamina;
            Stamina = Mathf.Clamp(Stamina + amount, 0f, _maxStamina);
            var delta = Mathf.RoundToInt(Stamina - previous);
            var revivedBy = ResolveExhaustion();

            StatsChanged?.Invoke();
            return new StaminaChange(delta, null, revivedBy);
        }

        public int ChangeNatureScore(int amount)
        {
            var previous = NatureScore;
            NatureScore = Mathf.Clamp(NatureScore + amount, 0, _maxNatureScore);

            StatsChanged?.Invoke();
            return NatureScore - previous;
        }

        protected override void OnFinished(bool won)
        {
            Time.timeScale = 1f;
            base.OnFinished(won);
        }

        private void Drain(float amount)
        {
            Stamina = Mathf.Max(Stamina - amount, 0f);
            ResolveExhaustion();
            StatsChanged?.Invoke();
        }

        private ItemDefinition ResolveExhaustion() => Stamina > 0f ? null : ReviveOrEndRun();

        private ItemDefinition ReviveOrEndRun()
        {
            var revive = Inventory.FindRevive();
            if (revive == null)
            {
                EndRun();
                return null;
            }

            Inventory.Use(revive);
            Stamina = Mathf.Min(revive.ReviveStamina, _maxStamina);
            return revive;
        }

        private void EndRun()
        {
            IsGameOver = true;
            GameOver?.Invoke();
        }
    }
}
