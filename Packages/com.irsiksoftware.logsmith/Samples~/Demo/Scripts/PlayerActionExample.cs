using UnityEngine;
using IrsikSoftware.LogSmith;

namespace IrsikSoftware.LogSmith.Samples
{
    /// <summary>
    /// Simulates player actions with logging.
    /// Use keyboard shortcuts to trigger different logged actions.
    /// </summary>
    public class PlayerActionExample : MonoBehaviour
    {
        [Header("Player Identity")]
        [Tooltip("Player name shown in log messages")]
        [SerializeField] private string playerName = "Player1";

        [Tooltip("Player color for visual identification")]
        [SerializeField] private Color playerColor = Color.green;

        [Header("Health System")]
        [Tooltip("Starting health points")]
        [SerializeField] [Range(1, 200)] private int startingHealth = 100;

        [Tooltip("Maximum health capacity")]
        [SerializeField] [Range(1, 200)] private int maxHealth = 100;

        [Tooltip("Health threshold for critical warning")]
        [SerializeField] [Range(1, 50)] private int criticalHealthThreshold = 30;

        [Tooltip("Log health changes")]
        [SerializeField] private bool logHealthChanges = true;

        [Header("Combat System")]
        [Tooltip("Starting ammo count")]
        [SerializeField] [Range(0, 100)] private int startingAmmo = 30;

        [Tooltip("Maximum ammo capacity")]
        [SerializeField] [Range(1, 100)] private int maxAmmo = 30;

        [Tooltip("Low ammo warning threshold")]
        [SerializeField] [Range(1, 20)] private int lowAmmoThreshold = 5;

        [Tooltip("Damage per shot")]
        [SerializeField] [Range(1, 50)] private int damagePerShot = 10;

        [Tooltip("Log combat actions")]
        [SerializeField] private bool logCombatActions = true;

        [Header("Score System")]
        [Tooltip("Starting score")]
        [SerializeField] private int startingScore = 0;

        [Tooltip("Log score changes")]
        [SerializeField] private bool logScoreChanges = true;

        [Header("Keyboard Controls")]
        [Tooltip("Enable keyboard shortcuts (1-5)")]
        [SerializeField] private bool enableKeyboardControls = true;

        [Tooltip("Show control hints on start")]
        [SerializeField] private bool showControlHints = true;

        [Header("Auto Actions")]
        [Tooltip("Automatically take damage periodically")]
        [SerializeField] private bool autoDamage = false;

        [Tooltip("Auto-damage interval (seconds)")]
        [SerializeField] [Range(1f, 10f)] private float autoDamageInterval = 3f;

        [Header("Current Stats (Read-Only)")]
        [SerializeField] [HideInInspector] private int _currentHealth;
        [SerializeField] [HideInInspector] private int _currentAmmo;
        [SerializeField] [HideInInspector] private int _score;
        [SerializeField] [HideInInspector] private bool _isDead = false;

        private ILog _playerLogger;
        private float _lastAutoDamageTime;
        private int _totalDamageTaken = 0;
        private int _totalShotsFired = 0;
        private int _totalItemsCollected = 0;

        void Start()
        {
            _playerLogger = LogSmith.CreateLogger("Player");
            _currentHealth = startingHealth;
            _currentAmmo = startingAmmo;
            _score = startingScore;

            _playerLogger.Info($"Player '{playerName}' spawned");
            _playerLogger.Debug($"Initial stats - Health: {_currentHealth}/{maxHealth}, Ammo: {_currentAmmo}/{maxAmmo}, Score: {_score}");

            if (showControlHints && enableKeyboardControls)
            {
                _playerLogger.Info("=== KEYBOARD CONTROLS ===");
                _playerLogger.Info("Press 1: Take Damage");
                _playerLogger.Info("Press 2: Heal");
                _playerLogger.Info("Press 3: Fire Weapon");
                _playerLogger.Info("Press 4: Collect Item");
                _playerLogger.Info("Press 5: Add Score");
            }
        }

        void Update()
        {
            if (!enableKeyboardControls) return;

            // Keyboard controls for demo
            if (Input.GetKeyDown(KeyCode.Alpha1))
                TakeDamage(damagePerShot);

            if (Input.GetKeyDown(KeyCode.Alpha2))
                Heal(20);

            if (Input.GetKeyDown(KeyCode.Alpha3))
                FireWeapon();

            if (Input.GetKeyDown(KeyCode.Alpha4))
                CollectItem("Health Pack");

            if (Input.GetKeyDown(KeyCode.Alpha5))
                AddScore(100);

            // Auto-damage
            if (autoDamage && !_isDead && Time.time - _lastAutoDamageTime >= autoDamageInterval)
            {
                TakeDamage(Random.Range(5, 15));
                _lastAutoDamageTime = Time.time;
            }
        }

        [ContextMenu("Take Damage")]
        public void TakeDamage(int amount)
        {
            _currentHealth -= amount;

            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                _playerLogger.Error($"{playerName} has died!");
            }
            else if (_currentHealth < 30)
            {
                _playerLogger.Warn($"{playerName} health critical: {_currentHealth}/{startingHealth}");
            }
            else
            {
                _playerLogger.Info($"{playerName} took {amount} damage (health: {_currentHealth}/{startingHealth})");
            }
        }

        [ContextMenu("Heal")]
        public void Heal(int amount)
        {
            _currentHealth = Mathf.Min(_currentHealth + amount, startingHealth);
            _playerLogger.Info($"{playerName} healed for {amount} (health: {_currentHealth}/{startingHealth})");
        }

        [ContextMenu("Fire Weapon")]
        public void FireWeapon()
        {
            if (_currentAmmo <= 0)
            {
                _playerLogger.Warn($"{playerName} out of ammo!");
                return;
            }

            _currentAmmo--;
            _playerLogger.Debug($"{playerName} fired weapon (ammo: {_currentAmmo}/{startingAmmo})");

            if (_currentAmmo <= 5)
                _playerLogger.Warn($"{playerName} low on ammo: {_currentAmmo} remaining");
        }

        [ContextMenu("Collect Item")]
        public void CollectItem(string itemName)
        {
            _playerLogger.Info($"{playerName} collected: {itemName}");
        }

        [ContextMenu("Add Score")]
        public void AddScore(int points)
        {
            _score += points;
            _playerLogger.Info($"{playerName} scored {points} points (total: {_score})");
        }

        [ContextMenu("Show Player Stats")]
        public void ShowPlayerStats()
        {
            _playerLogger.Info("=== PLAYER STATISTICS ===");
            _playerLogger.Info($"Name: {playerName}");
            _playerLogger.Info($"Health: {_currentHealth}/{maxHealth}");
            _playerLogger.Info($"Ammo: {_currentAmmo}/{maxAmmo}");
            _playerLogger.Info($"Score: {_score}");
            _playerLogger.Info($"Status: {(_isDead ? "DEAD" : "ALIVE")}");
            _playerLogger.Info($"Total Damage Taken: {_totalDamageTaken}");
            _playerLogger.Info($"Total Shots Fired: {_totalShotsFired}");
            _playerLogger.Info($"Total Items Collected: {_totalItemsCollected}");
        }

        [ContextMenu("Reset Player")]
        public void ResetPlayer()
        {
            _currentHealth = startingHealth;
            _currentAmmo = startingAmmo;
            _score = startingScore;
            _isDead = false;
            _totalDamageTaken = 0;
            _totalShotsFired = 0;
            _totalItemsCollected = 0;

            _playerLogger.Info($"{playerName} has been reset to starting values");
        }

        [ContextMenu("Full Heal & Reload")]
        public void FullHealAndReload()
        {
            _currentHealth = maxHealth;
            _currentAmmo = maxAmmo;
            _isDead = false;

            _playerLogger.Info($"{playerName} fully healed and reloaded!");
        }

        [ContextMenu("Simulate Death")]
        public void SimulateDeath()
        {
            _currentHealth = 0;
            _isDead = true;
            _playerLogger.Error($"{playerName} has died!");
            _playerLogger.Critical($"GAME OVER for {playerName}");
        }

        void OnEnable()
        {
            if (_playerLogger != null)
                _playerLogger.Debug($"{playerName} enabled");
        }

        void OnDisable()
        {
            if (_playerLogger != null)
                _playerLogger.Debug($"{playerName} disabled");
        }

        void OnDrawGizmosSelected()
        {
            // Draw health bar visualization
            Gizmos.color = _isDead ? Color.red : playerColor;
            Gizmos.DrawWireSphere(transform.position, 1f);

            // Draw health as a line
            if (_currentHealth > 0)
            {
                float healthPercent = (float)_currentHealth / maxHealth;
                Gizmos.color = healthPercent > 0.5f ? Color.green : healthPercent > 0.25f ? Color.yellow : Color.red;
                Gizmos.DrawLine(transform.position + Vector3.up * 2f, transform.position + Vector3.up * 2f + Vector3.right * healthPercent * 2f);
            }
        }
    }
}
