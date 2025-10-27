# Daily Quest Framework Design Document

**Version:** 1.0  
**Date:** 2024  
**Project:** DailyQuestMod for Duckov

---

## Table of Contents

1. [Overview](#overview)
2. [Core Concepts](#core-concepts)
3. [Framework Interfaces](#framework-interfaces)
4. [Framework API](#framework-api)
5. [Internal Implementation](#internal-implementation)
6. [Helper Utilities](#helper-utilities)
7. [Integration Components](#integration-components)
8. [Usage Examples](#usage-examples)
9. [Project Structure](#project-structure)
10. [Design Decisions](#design-decisions)

---

## Overview

The Daily Quest Framework provides a flexible, extensible system for implementing daily quests in Duckov. The framework is designed to be:

- **General-purpose**: Not tied to specific quest types or implementations
- **Extensible**: Modders can easily add custom quest pools and quest types
- **Flexible**: Supports both event-driven and polling-based quest checking
- **Time-based**: Automatically refreshes quests based on real-world day (UTC)

### Key Features

- Plugin-based architecture for quest pools
- Support for scanning (polling) and event-driven quest checking
- Automatic daily refresh at midnight UTC
- Quest expiration handling
- Save/load support
- Configurable maximum daily quest limit
- Separate tracking from in-game quest system

---

## Core Concepts

### Quest Pools

A **Quest Pool** is a source of daily quests. Quest pools can:
- Generate quests from files, hardcoded data, or procedurally
- Implement custom selection logic (random, weighted, sequential, etc.)
- Be registered by any mod

### Quest Check Modes

**Scanning Mode:**
- Framework periodically polls `IsCompleted()` at specified interval
- Good for: location checks, item possession checks, time-based conditions
- Default interval: 0.5 seconds (configurable per quest)

**Event-Driven Mode:**
- No polling, modder subscribes to events and manually calls `CompleteQuest()`
- Good for: kill counts, item submission, specific game events
- More efficient for event-based conditions

### Daily Refresh Cycle

1. At midnight UTC, framework checks for day crossing
2. Expires all incomplete quests (calls `OnExpired()`)
3. Calls `SelectQuestsForToday()` on all registered pools
4. Collects all selected quests
5. Applies max quest limit if configured (random sampling)
6. Activates selected quests (calls `OnActivated()`)
7. Saves state

---

## Framework Interfaces

### IDailyQuest

Represents a single daily quest. Modders implement this interface to create custom quests.

```csharp
/// <summary>
/// Represents a single daily quest
/// </summary>
public interface IDailyQuest
{
    // ===== Identity =====
    
    /// <summary>
    /// Unique quest identifier (must be unique across all pools)
    /// </summary>
    string QuestId { get; }
    
    /// <summary>
    /// Display name of the quest
    /// </summary>
    string Title { get; }
    
    /// <summary>
    /// Quest description (can include progress info)
    /// </summary>
    string Description { get; }
    
    // ===== Checking Mode =====
    
    /// <summary>
    /// How the quest completion should be checked
    /// </summary>
    QuestCheckMode CheckMode { get; }
    
    /// <summary>
    /// Scan interval in seconds (only used if CheckMode == Scanning)
    /// Recommended: 0.5 - 2.0 seconds
    /// </summary>
    float ScanInterval { get; }
    
    /// <summary>
    /// Returns true if quest objectives are completed
    /// For Scanning mode: called periodically by framework
    /// For EventDriven mode: called when CompleteQuest() is invoked
    /// </summary>
    bool IsCompleted();
    
    // ===== Lifecycle Callbacks =====
    
    /// <summary>
    /// Called when quest is activated for the day
    /// Use this to: subscribe to events, initialize state, etc.
    /// </summary>
    void OnActivated();
    
    /// <summary>
    /// Called when quest is completed (IsCompleted() returns true)
    /// IMPORTANT: Handle rewards here (money, exp, items, etc.)
    /// Framework does NOT automatically give rewards
    /// </summary>
    void OnCompleted();
    
    /// <summary>
    /// Called when day ends and quest was not completed
    /// Use this to: clean up event subscriptions, show notifications, etc.
    /// </summary>
    void OnExpired();
    
    // ===== Persistence =====
    
    /// <summary>
    /// Return quest state data to be saved
    /// Return null if no state needs to be saved
    /// Supported types: primitives, strings, serializable objects
    /// </summary>
    object GetSaveData();
    
    /// <summary>
    /// Restore quest state from saved data
    /// Called after OnActivated() when loading from save
    /// </summary>
    void LoadSaveData(object data);
}

/// <summary>
/// Quest completion checking mode
/// </summary>
public enum QuestCheckMode
{
    /// <summary>
    /// Framework polls IsCompleted() at ScanInterval
    /// Use for: location checks, possession checks, time checks
    /// </summary>
    Scanning,
    
    /// <summary>
    /// Modder manually calls DailyQuestManager.CompleteQuest()
    /// Use for: kill counts, event triggers, submissions
    /// More efficient than scanning
    /// </summary>
    EventDriven
}
```

### IDailyQuestPool

Represents a source of daily quests. Modders implement this to provide quest pools.

```csharp
/// <summary>
/// Represents a pool of quests that can be selected from
/// </summary>
public interface IDailyQuestPool
{
    /// <summary>
    /// Unique pool identifier
    /// </summary>
    string PoolId { get; }
    
    /// <summary>
    /// Called once when pool is registered
    /// Use this to: load quest definitions, parse files, initialize data
    /// NOTE: May be called immediately on registration or delayed until first refresh
    /// </summary>
    void Initialize();
    
    /// <summary>
    /// Select which quests should be active today
    /// Called automatically when day refreshes (midnight UTC)
    /// 
    /// Implementation should:
    /// 1. Apply selection logic (random, weighted, sequential, etc.)
    /// 2. Return list of quest instances to activate
    /// 3. Can return empty list if no quests available
    /// </summary>
    /// <param name="today">Current date (UTC)</param>
    /// <returns>List of quests to activate for today</returns>
    List<IDailyQuest> SelectQuestsForToday(DateTime today);
}
```

---

## Framework API

### DailyQuestManager

Static API for interacting with the daily quest system.

```csharp
/// <summary>
/// Main daily quest system manager
/// Static API for quest management
/// </summary>
public static class DailyQuestManager
{
    // ===== Configuration =====
    
    /// <summary>
    /// Maximum number of daily quests allowed simultaneously
    /// -1 = unlimited (default)
    /// 0 = no quests
    /// >0 = max limit, excess quests are randomly sampled
    /// </summary>
    public static int MaxDailyQuests { get; set; } = -1;
    
    /// <summary>
    /// How often to check for day crossing (in seconds)
    /// Default: 60 seconds
    /// Lower values = more responsive but more CPU usage
    /// </summary>
    public static float DayCrossingCheckInterval { get; set; } = 60f;
    
    // ===== Registration =====
    
    /// <summary>
    /// Register a quest pool
    /// Pool.Initialize() may be called immediately or deferred
    /// </summary>
    /// <param name="pool">Quest pool to register</param>
    public static void RegisterQuestPool(IDailyQuestPool pool);
    
    /// <summary>
    /// Unregister a quest pool
    /// Does not affect currently active quests from this pool
    /// </summary>
    /// <param name="poolId">Pool ID to unregister</param>
    public static void UnregisterQuestPool(string poolId);
    
    // ===== Quest Management =====
    
    /// <summary>
    /// Get all currently active daily quests (not completed)
    /// </summary>
    public static List<IDailyQuest> GetActiveQuests();
    
    /// <summary>
    /// Get all completed quests for today
    /// </summary>
    public static List<IDailyQuest> GetCompletedQuests();
    
    /// <summary>
    /// Get quest by ID from active or completed lists
    /// Returns null if not found
    /// </summary>
    public static IDailyQuest GetQuest(string questId);
    
    /// <summary>
    /// Mark an event-driven quest as completed
    /// Framework will verify completion via IsCompleted() and call OnCompleted()
    /// For scanning quests, this is called automatically
    /// </summary>
    /// <param name="questId">Quest ID to complete</param>
    public static void CompleteQuest(string questId);
    
    /// <summary>
    /// Check if a quest is currently active
    /// </summary>
    public static bool IsQuestActive(string questId);
    
    /// <summary>
    /// Check if a quest is completed today
    /// </summary>
    public static bool IsQuestCompleted(string questId);
    
    // ===== Time & Refresh =====
    
    /// <summary>
    /// Last time quests were refreshed (date only, midnight UTC)
    /// </summary>
    public static DateTime LastRefreshTime { get; }
    
    /// <summary>
    /// Time remaining until next refresh (next midnight UTC)
    /// </summary>
    public static TimeSpan TimeUntilNextRefresh { get; }
    
    /// <summary>
    /// Manually trigger quest refresh
    /// WARNING: This will expire all current quests
    /// Mainly for testing/debugging
    /// </summary>
    public static void ForceRefresh();
    
    // ===== Events =====
    
    /// <summary>
    /// Fired when quests are refreshed for a new day
    /// </summary>
    public static event Action OnQuestsRefreshed;
    
    /// <summary>
    /// Fired when a quest is completed
    /// </summary>
    public static event Action<IDailyQuest> OnQuestCompleted;
    
    /// <summary>
    /// Fired when a quest expires without completion
    /// </summary>
    public static event Action<IDailyQuest> OnQuestExpired;
}
```

---

## Internal Implementation

### Core Logic Flow

#### Initialization
1. ModBehaviour creates DailyQuestManagerInternal GameObject
2. Register quest pools via `RegisterQuestPool()`
3. Load saved state (last refresh date, active quests)
4. If saved date != today, trigger refresh
5. Otherwise, restore active quests and their state

#### Update Loop
```
Every frame:
  1. Check if time to verify day crossing (every DayCrossingCheckInterval seconds)
     - If HasDayCrossed(): trigger RefreshDailyQuests()
  
  2. For each active scanning quest:
     - If time >= nextScanTime:
       - Call quest.IsCompleted()
       - If true: call CompleteQuestInternal()
       - Update nextScanTime
```

#### Refresh Cycle
```
RefreshDailyQuests():
  1. For each active quest not completed:
     - Try: quest.OnExpired()
     - Catch & log errors
     - Fire OnQuestExpired event
  
  2. Clear active and completed quest lists
  
  3. For each registered pool:
     - Try: quests = pool.SelectQuestsForToday(DateTime.UtcNow)
     - Catch & log errors
     - Add quests to collection
  
  4. If MaxDailyQuests > 0 and total quests > max:
     - Random sample to MaxDailyQuests
  
  5. For each selected quest:
     - Try: quest.OnActivated()
     - Catch & log errors
     - Add to activeQuests with scan timing info
  
  6. Update lastRefreshDate = DateTime.UtcNow.Date
  7. Fire OnQuestsRefreshed event
  8. SaveState()
```

#### Quest Completion
```
CompleteQuest(questId):
  1. Find quest in activeQuests
  2. If not found or already completed: return
  3. CompleteQuestInternal(questData)

CompleteQuestInternal(questData):
  1. Mark questData.completed = true
  2. Move to completedQuests list
  3. Try: quest.OnCompleted()  // Quest handles rewards
  4. Catch & log errors
  5. Fire OnQuestCompleted event
  6. SaveState()
```

### Data Structures

```csharp
private class QuestData
{
    public IDailyQuest quest;
    public float nextScanTime;      // For scanning quests
    public bool completed;
}

private class SaveData
{
    public string lastRefreshDate;  // ISO 8601 format
    public List<QuestSaveEntry> activeQuests;
    public List<string> completedQuestIds;
}

private class QuestSaveEntry
{
    public string questId;
    public string poolId;           // For quest reconstruction
    public object questData;        // From quest.GetSaveData()
    public bool completed;
}
```

### Error Handling

All modder code is wrapped in try-catch:
- Quest.OnActivated()
- Quest.OnCompleted()
- Quest.OnExpired()
- Quest.IsCompleted()
- Pool.Initialize()
- Pool.SelectQuestsForToday()

Errors are logged with Debug.LogError() and execution continues.

### Quest ID Uniqueness

Framework enforces unique quest IDs:
- When activating quests, check for duplicate IDs
- If duplicate found, log error and skip that quest
- First-come-first-served if multiple pools provide same ID

---

## Helper Utilities

### DailyQuestBase

Abstract base class with boilerplate implementations.

```csharp
public abstract class DailyQuestBase : IDailyQuest
{
    public abstract string QuestId { get; }
    public abstract string Title { get; }
    public virtual string Description => Title;
    
    public abstract QuestCheckMode CheckMode { get; }
    public virtual float ScanInterval => 0.5f;
    
    public abstract bool IsCompleted();
    
    public virtual void OnActivated() { }
    public virtual void OnCompleted() { }
    public virtual void OnExpired() { }
    
    public virtual object GetSaveData() => null;
    public virtual void LoadSaveData(object data) { }
    
    // Helper: Give money reward
    protected void GiveMoneyReward(int amount)
    {
        EconomyManager.Add(amount);
        // Show notification
    }
    
    // Helper: Give exp reward
    protected void GiveExpReward(int amount)
    {
        EXPManager.AddExp(amount);
        // Show notification
    }
    
    // Helper: Give item reward
    protected void GiveItemReward(int itemId, int amount)
    {
        // Generate and give items to player storage
    }
}
```

### SimpleScanningQuest

Base class for simple scanning quests.

```csharp
public abstract class SimpleScanningQuest : DailyQuestBase
{
    public override QuestCheckMode CheckMode => QuestCheckMode.Scanning;
    public override float ScanInterval => 1.0f; // Override if needed
    
    // Subclass only needs to implement IsCompleted() and rewards
}
```

### SimpleEventQuest

Base class for simple event-driven quests.

```csharp
public abstract class SimpleEventQuest : DailyQuestBase
{
    public override QuestCheckMode CheckMode => QuestCheckMode.EventDriven;
    public override float ScanInterval => 0f; // Not used
    
    protected void NotifyCompleted()
    {
        if (IsCompleted())
        {
            DailyQuestManager.CompleteQuest(QuestId);
        }
    }
}
```

---

## Integration Components

### DailyQuestGiver

NPC component for showing daily quests UI.

```csharp
/// <summary>
/// InteractableBase component for NPCs to show daily quests
/// Use with MultiInteraction for multiple interaction options
/// </summary>
public class DailyQuestGiver : InteractableBase
{
    public override string InteractName => "Daily Quests"; // Localized
    
    protected override void OnInteractStart(CharacterMainControl character)
    {
        DailyQuestGiverView.Instance?.Setup(this);
        DailyQuestGiverView.Instance?.Open();
    }
    
    protected override void OnInteractStop()
    {
        DailyQuestGiverView.Instance?.Close();
    }
}
```

### DailyQuestGiverView

UI view for displaying daily quests (based on QuestGiverView).

**Features:**
- Tabs: Available (active, not completed) / Completed
- Quest list with title
- Quest details panel with description
- No progress bars (text-based progress in description)
- Complete button (for event-driven quests that need manual submission)

**Not integrated with game's QuestManager** - completely separate UI instance.

---

## Usage Examples

### Example 1: Event-Driven Boss Kill Quest

```csharp
public class KillBossQuest : SimpleEventQuest
{
    private int killCount = 0;
    private int requiredCount = 1;
    
    public override string QuestId => "daily_kill_boss";
    public override string Title => "Boss Hunter";
    public override string Description => $"Defeat any boss creature ({killCount}/{requiredCount})";
    
    public override bool IsCompleted() => killCount >= requiredCount;
    
    public override void OnActivated()
    {
        killCount = 0;
        Health.OnDead += OnEnemyDead;
    }
    
    public override void OnExpired()
    {
        Health.OnDead -= OnEnemyDead;
    }
    
    public override void OnCompleted()
    {
        Health.OnDead -= OnEnemyDead;
        GiveMoneyReward(500);
        GiveExpReward(100);
        // Show completion notification
    }
    
    private void OnEnemyDead(Health health, DamageInfo info)
    {
        // Check if it's a boss killed by player
        if (IsBoss(health) && info.fromCharacter?.IsMainCharacter() == true)
        {
            killCount++;
            NotifyCompleted(); // Check and complete if done
        }
    }
    
    private bool IsBoss(Health health)
    {
        var character = health.TryGetCharacter();
        if (character == null) return false;
        
        var preset = character.characterPreset;
        return preset != null && preset.characterIconType == CharacterIconTypes.boss;
    }
    
    public override object GetSaveData() => killCount;
    public override void LoadSaveData(object data) => killCount = (int)data;
}
```

### Example 2: Scanning Location Quest

```csharp
public class ReachLocationQuest : SimpleScanningQuest
{
    private Vector3 targetLocation;
    private float radius;
    private string locationName;
    
    public override string QuestId => $"daily_reach_{locationName}";
    public override string Title => $"Explorer: {locationName}";
    public override string Description => $"Find and reach {locationName}";
    public override float ScanInterval => 1.0f; // Check every second
    
    public ReachLocationQuest(Vector3 location, float radius, string name)
    {
        this.targetLocation = location;
        this.radius = radius;
        this.locationName = name;
    }
    
    public override bool IsCompleted()
    {
        var player = CharacterMainControl.Main;
        if (player == null) return false;
        
        float distance = Vector3.Distance(player.transform.position, targetLocation);
        return distance <= radius;
    }
    
    public override void OnCompleted()
    {
        GiveMoneyReward(300);
        // Show completion notification
    }
}
```

### Example 3: Hardcoded Quest Pool

```csharp
public class ExampleQuestPool : IDailyQuestPool
{
    public string PoolId => "ExampleQuests";
    
    private List<IDailyQuest> allQuests;
    
    public void Initialize()
    {
        allQuests = new List<IDailyQuest>
        {
            new KillBossQuest(),
            new ReachLocationQuest(new Vector3(100, 0, 200), 10f, "Hidden Cave"),
            // Add more quests...
        };
    }
    
    public List<IDailyQuest> SelectQuestsForToday(DateTime today)
    {
        // Random selection: 2 quests per day
        int seed = today.Year * 10000 + today.DayOfYear;
        Random rng = new Random(seed);
        
        return allQuests
            .OrderBy(q => rng.Next())
            .Take(2)
            .ToList();
    }
}

// Registration in ModBehaviour
public override void OnAfterSetup()
{
    DailyQuestManager.RegisterQuestPool(new ExampleQuestPool());
}
```

### Example 4: File-Based Quest Pool (Future Implementation)

```csharp
public class FileBasedQuestPool : IDailyQuestPool
{
    private string questFolderPath;
    private List<IDailyQuest> parsedQuests;
    
    public string PoolId => "FileBasedQuests";
    
    public FileBasedQuestPool(string folderPath)
    {
        this.questFolderPath = folderPath;
    }
    
    public void Initialize()
    {
        // Parse all .txt files in folder
        // Create quest instances based on definitions
        parsedQuests = QuestParser.ParseQuestFolder(questFolderPath);
    }
    
    public List<IDailyQuest> SelectQuestsForToday(DateTime today)
    {
        // Read meta.ini or use default selection logic
        // For example: random 3 quests per day
        return RandomSelect(parsedQuests, 3, today);
    }
}
```

---

## Project Structure

```
DailyQuestMod/
├── info.ini                          # Mod metadata
├── DailyQuestMod.csproj              # Project file
├── FRAMEWORK_DESIGN.md               # This document
│
├── ModBehaviour.cs                   # Mod entry point
│
├── Framework/                        # Core framework (general-purpose)
│   ├── IDailyQuest.cs                # Quest interface
│   ├── IDailyQuestPool.cs            # Pool interface
│   ├── QuestCheckMode.cs             # Enum
│   ├── DailyQuestManager.cs          # Static API
│   └── DailyQuestManagerInternal.cs  # Internal implementation
│
├── Helpers/                          # Helper utilities
│   ├── DailyQuestBase.cs             # Base quest class
│   ├── SimpleScanningQuest.cs        # Scanning quest base
│   └── SimpleEventQuest.cs           # Event quest base
│
├── Integration/                      # Game integration
│   ├── DailyQuestGiver.cs            # NPC component
│   └── UI/
│       └── DailyQuestGiverView.cs    # UI view
│
└── Examples/                         # Example implementations (not framework)
    ├── Pools/
    │   ├── FileBasedQuestPool.cs     # File parsing pool
    │   └── ExampleQuestPool.cs       # Hardcoded pool
    ├── Quests/
    │   ├── KillBossQuest.cs          # Boss killing
    │   ├── KillMobsQuest.cs          # Mob killing
    │   └── SubmitItemsQuest.cs       # Item submission
    └── QuestDefinitions/             # Quest files
        ├── BossKilling/
        │   └── *.txt
        ├── ItemSubmission/
        │   └── *.txt
        └── MobKilling/
            └── *.txt
```

---

## Design Decisions

### Why Separate from Game's Quest System?

- **Independence**: No conflicts with game quest IDs or state
- **Flexibility**: Can implement custom UI and behavior
- **Simplicity**: Don't need to work around game's quest assumptions
- **Clean**: Daily quests have different lifecycle than story quests

### Why Interface-Based Instead of Task-Based?

- **Flexibility**: Modders can implement quests any way they want
- **Simplicity**: No need to understand game's Task system
- **Extensibility**: Easy to add any kind of quest logic
- **Decoupling**: Framework doesn't depend on game's quest implementation

### Why Two Check Modes?

- **Efficiency**: Event-driven is more efficient for event-based conditions
- **Flexibility**: Scanning is easier for state-based conditions (location, possession)
- **Choice**: Modder can choose the most appropriate approach

### Why UTC Midnight for Refresh?

- **Consistency**: Same refresh time for all players worldwide
- **Simplicity**: No timezone complexity
- **Reliability**: DateTime.UtcNow is stable and reliable
- **Following Game Pattern**: BlackMarket uses DateTime.UtcNow

### Why Unlimited Quests by Default?

- **Flexibility**: Let modders decide quest count in their pools
- **No Artificial Limits**: Framework shouldn't impose restrictions
- **Configurable**: Can set MaxDailyQuests if needed
- **Fairness**: All pools get a chance to contribute

### Why Quest Handles Rewards?

- **Flexibility**: Modders can implement custom reward logic
- **Extensibility**: Not limited to money/exp/items
- **Control**: Quest knows best what rewards to give
- **Simplicity**: Framework doesn't need reward system

### Why Save Quest State?

- **Persistence**: Progress survives logout/reload
- **Player Friendly**: Don't lose progress if game crashes
- **Expected Behavior**: Players expect progress to persist
- **Completeness**: Professional quest system behavior

---

## Future Considerations

### Potential Enhancements

1. **Quest Dependencies**: Quest B unlocks after completing Quest A
2. **Quest Chains**: Multi-day quest sequences
3. **Difficulty Tiers**: Easy/Medium/Hard quest categorization
4. **Streak Bonuses**: Rewards for completing X days in a row
5. **Quest History**: View past completed quests
6. **Notifications**: Toast messages for completion/expiration
7. **Progress Bars**: Visual progress in UI (requires extending interface)
8. **Quest Preview**: See tomorrow's quests based on seed
9. **Manual Quest Selection**: Player chooses from available pool
10. **Localization**: Multi-language support for quest text

### Backward Compatibility

When adding features, maintain interface compatibility:
- Add optional methods with default implementations
- Use extension methods for new functionality
- Version the save data format
- Provide migration path for old quests

---

## Summary

This framework provides a flexible foundation for daily quests in Duckov. The key design principles are:

1. **Extensibility**: Easy for modders to add custom quests and pools
2. **Flexibility**: Support multiple checking modes and implementations
3. **Simplicity**: Clean interfaces, minimal boilerplate
4. **Robustness**: Error handling, state persistence, proper lifecycle management
5. **Independence**: Separate from game's quest system

The framework handles:
- Time tracking and daily refresh
- Quest lifecycle management
- Condition checking (scanning and event-driven)
- State persistence
- Quest pool registry

Modders implement:
- Quest logic (conditions, progress)
- Quest pools (selection, generation)
- Reward handling
- Quest content (text, parameters)

This separation of concerns creates a powerful, maintainable system that can support a wide variety of daily quest implementations.