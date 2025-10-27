using System;
using UnityEngine;
using DailyQuestMod.Framework;
using DailyQuestMod.Helpers;
using Duckov.Economy;

namespace DailyQuestMod.Examples.Quests
{
    /// <summary>
    /// Simple test quest for event-driven mode verification
    /// Completes when spacebar is pressed 3 times
    /// </summary>
    public class TestEventQuest : IDailyQuest
    {
        private int pressCount = 0;
        private const int REQUIRED_PRESSES = 3;

        public string QuestId => "test_event_quest";
        public string Title => "Test Event Quest";
        public string Description => $"Press spacebar ({pressCount}/{REQUIRED_PRESSES})";

        public QuestCheckMode CheckMode => QuestCheckMode.EventDriven;
        public float ScanInterval => 0f; // Not used for event-driven

        private MonoBehaviour updateHost;

        public bool IsCompleted()
        {
            return pressCount >= REQUIRED_PRESSES;
        }

        public void OnActivated()
        {
            pressCount = 0;
            Debug.Log($"[TestEventQuest] ===== ACTIVATED - press spacebar {REQUIRED_PRESSES} times =====");
            Debug.Log($"[TestEventQuest] CheckMode={CheckMode}, QuestId={QuestId}");

            // Find the DailyQuestManager GameObject and attach InputListener to it
            var managerGO = GameObject.Find("DailyQuestManager");
            if (managerGO == null)
            {
                Debug.LogError("[TestEventQuest] Could not find DailyQuestManager GameObject!");
                return;
            }

            Debug.Log($"[TestEventQuest] Found DailyQuestManager GameObject, attaching InputListener");

            updateHost = managerGO.AddComponent<InputListener>();
            Debug.Log($"[TestEventQuest] InputListener component added, enabled: {updateHost.enabled}, isActiveAndEnabled: {updateHost.isActiveAndEnabled}");

            ((InputListener)updateHost).OnSpacePressed += OnSpacePressed;
            Debug.Log($"[TestEventQuest] Event handler subscribed");
            Debug.Log($"[TestEventQuest] Time.time at activation: {Time.time:F1}");
        }

        private void OnSpacePressed()
        {
            pressCount++;
            Debug.Log($"[TestEventQuest] ===== SPACE PRESSED! ===== Count: {pressCount}/{REQUIRED_PRESSES}");
            Debug.Log($"[TestEventQuest] IsCompleted()={IsCompleted()}");

            if (IsCompleted())
            {
                Debug.Log($"[TestEventQuest] Quest completed, calling DailyQuestManager.CompleteQuest({QuestId})");
                DailyQuestManager.CompleteQuest(QuestId);
            }
        }

        public void OnCompleted()
        {
            Debug.Log($"[TestEventQuest] Completed! Total presses: {pressCount}");

            // Give test reward
            try
            {
                bool success = EconomyManager.Add(100);
                if (success)
                {
                    Debug.Log("[TestEventQuest] Gave $100 test reward to player");
                }
                else
                {
                    Debug.LogError("[TestEventQuest] Failed to give money reward");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[TestEventQuest] Error giving reward: {e}");
            }

            // Cleanup - destroy the component, not the manager GameObject
            if (updateHost != null)
            {
                GameObject.Destroy(updateHost);
                updateHost = null;
            }
        }

        public void OnExpired()
        {
            Debug.Log($"[TestEventQuest] Expired without completion (presses: {pressCount}/{REQUIRED_PRESSES})");

            // Cleanup - destroy the component, not the manager GameObject
            if (updateHost != null)
            {
                GameObject.Destroy(updateHost);
                updateHost = null;
            }
        }

        public object GetSaveData()
        {
            return pressCount;
        }

        public void LoadSaveData(object data)
        {
            if (data is int count)
            {
                pressCount = count;
                Debug.Log($"[TestEventQuest] ===== RESTORED PRESS COUNT: {pressCount}/{REQUIRED_PRESSES} =====");
                Debug.Log($"[TestEventQuest] InputListener exists: {updateHost != null}, GameObject active: {(updateHost != null ? updateHost.gameObject.activeSelf.ToString() : "N/A")}");
            }
        }

        // Helper MonoBehaviour for input detection
        private class InputListener : MonoBehaviour
        {
            public event Action OnSpacePressed;
            private int updateCount = 0;
            private float nextLog = 0f;

            private void Awake()
            {
                Debug.Log($"[TestEventQuest.InputListener] Awake called! GameObject: {gameObject.name}, enabled: {enabled}, Time.time: {Time.time:F1}");
            }

            private void Start()
            {
                Debug.Log($"[TestEventQuest.InputListener] Start called! GameObject: {gameObject.name}, enabled: {enabled}, activeInHierarchy: {gameObject.activeInHierarchy}, Time.time: {Time.time:F1}");
                nextLog = Time.time + 5f;
            }

            private void Update()
            {
                updateCount++;

                // Log every 5 seconds to verify Update is running
                if (Time.time >= nextLog)
                {
                    nextLog = Time.time + 5f;
                    Debug.Log($"[TestEventQuest.InputListener] HEARTBEAT: Update running, calls: {updateCount}, Time.time: {Time.time:F1}");
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log($"[TestEventQuest.InputListener] ===== SPACEBAR DETECTED ===== in Update()! updateCount: {updateCount}");
                    OnSpacePressed?.Invoke();
                }
            }

            private void OnEnable()
            {
                Debug.Log($"[TestEventQuest.InputListener] OnEnable called! Time.time: {Time.time:F1}");
            }

            private void OnDisable()
            {
                Debug.Log($"[TestEventQuest.InputListener] OnDisable called!");
            }

            private void OnDestroy()
            {
                Debug.Log($"[TestEventQuest.InputListener] GameObject being destroyed, updateCount: {updateCount}");
            }
        }
    }
}
