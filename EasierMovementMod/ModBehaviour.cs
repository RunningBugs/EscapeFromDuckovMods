using UnityEngine;

namespace EasierMovementMod
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private bool movementModified = false;
        private float originalWalkAcc = -1f;
        private float originalRunAcc = -1f;

        // High acceleration values for instant movement
        private const float INSTANT_WALK_ACC = 999f;
        private const float INSTANT_RUN_ACC = 999f;

        void Awake()
        {
        }

        void Update()
        {
            if (!movementModified)
            {
                TryModifyMovement();
            }
        }

        private void TryModifyMovement()
        {
            // Wait for LevelManager to be initialized
            if (LevelManager.Instance == null || !LevelManager.LevelInited)
                return;

            // Get the main character
            var mainCharacter = LevelManager.Instance.MainCharacter;
            if (mainCharacter == null)
                return;

            // Get the movement component
            var movement = mainCharacter.movementControl;
            if (movement == null)
                return;

            // Store original values if not already stored
            if (originalWalkAcc < 0)
            {
                originalWalkAcc = movement.walkAcc;
                originalRunAcc = movement.runAcc;
            }

            // Modify the character's acceleration stats through the character item
            var characterItem = mainCharacter.CharacterItem;
            if (characterItem != null)
            {
                // Get the stat objects
                var walkAccStat = characterItem.GetStat("WalkAcc".GetHashCode());
                var runAccStat = characterItem.GetStat("RunAcc".GetHashCode());

                if (walkAccStat != null && runAccStat != null)
                {
                    // Set high acceleration values to remove inertia
                    walkAccStat.BaseValue = INSTANT_WALK_ACC;
                    runAccStat.BaseValue = INSTANT_RUN_ACC;

                    movementModified = true;
                }
            }
        }

        void OnDestroy()
        {
            // Restore original values if possible
            if (movementModified && LevelManager.Instance != null)
            {
                var mainCharacter = LevelManager.Instance.MainCharacter;
                if (mainCharacter != null && mainCharacter.CharacterItem != null)
                {
                    var walkAccStat = mainCharacter.CharacterItem.GetStat("WalkAcc".GetHashCode());
                    var runAccStat = mainCharacter.CharacterItem.GetStat("RunAcc".GetHashCode());

                    if (walkAccStat != null && runAccStat != null && originalWalkAcc >= 0)
                    {
                        walkAccStat.BaseValue = originalWalkAcc;
                        runAccStat.BaseValue = originalRunAcc;
                    }
                }
            }
        }
    }
}
