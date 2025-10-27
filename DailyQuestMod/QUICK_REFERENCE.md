# Daily Quest Framework - Quick Reference

**Version:** 1.0

---

## For Framework Users (Modders)

### 1. Create a Quest

Implement `IDailyQuest` interface:

```csharp
public class MyQuest : IDailyQuest
{
    public string QuestId => "my_unique_quest_id";
    public string Title => "My Quest Title";
    public string Description => "Quest description with progress";
    
    // Choose: Scanning or EventDriven
    public QuestCheckMode CheckMode => QuestCheckMode.EventDriven;
    public float ScanInterval => 0.5f; // Only for Scanning mode
    
    // Return true when quest is done
    public bool IsCompleted() => /* your condition */;
    
    // Called when quest starts
    public void OnActivated() { /* subscribe to events */ }
    
    // Called when quest completes - GIVE REWARDS HERE
    public void OnCompleted() { 
        EconomyManager.Add(500); // Money
        EXPManager.AddExp(100);  // Exp
    }
    
    // Called when quest expires
    public void OnExpired() { /* cleanup */ }
    
    // Save/load state
    public object GetSaveData() => /* your data or null */;
    public void LoadSaveData(object data) { /* restore state */ }
}
```

### 2. Create a Quest Pool

Implement `IDailyQuestPool` interface:

```csharp
public class MyQuestPool : IDailyQuestPool
{
    public string PoolId => "my_pool_id";
    
    private List<IDailyQuest> allQuests;
    
    public void Initialize()
    {
        // Load/create your quests
        allQuests = new List<IDailyQuest> { /* quests */ };
    }
    
    public List<IDailyQuest> SelectQuestsForToday(DateTime today)
    {
        // Your selection logic (random, weighted, etc.)
        return allQuests.Take(2).ToList();
    }
}
```

### 3. Register Your Pool

In your ModBehaviour:

```csharp
public override void OnAfterSetup()
{
    DailyQuestManager.RegisterQuestPool(new MyQuestPool());
}
```

---

## Quest Check Modes

### Scanning Mode
- Framework calls `IsCompleted()` every `ScanInterval` seconds
- Use for: location checks, possession checks, time checks
- Example: "Reach the cave", "Have 100 gold"

```csharp
public QuestCheckMode CheckMode => QuestCheckMode.Scanning;
public float ScanInterval => 1.0f; // Check every second

public bool IsCompleted()
{
    // Check condition (called automatically)
    return /* condition */;
}
```

### Event-Driven Mode
- You call `DailyQuestManager.CompleteQuest(questId)` when done
- Use for: kill counts, submissions, specific events
- More efficient than scanning

```csharp
public QuestCheckMode CheckMode => QuestCheckMode.EventDriven;

public void OnActivated()
{
    Health.OnDead += OnEnemyDead;
}

private void OnEnemyDead(Health health, DamageInfo info)
{
    if (/* condition met */)
    {
        DailyQuestManager.CompleteQuest(QuestId);
    }
}
```

---

## Framework API

### Configuration

```csharp
// Max quests per day (-1 = unlimited)
DailyQuestManager.MaxDailyQuests = 10;

// How often to check for day crossing (seconds)
DailyQuestManager.DayCrossingCheckInterval = 60f;
```

### Registration

```csharp
// Register pool
DailyQuestManager.RegisterQuestPool(pool);

// Unregister pool
DailyQuestManager.UnregisterQuestPool("pool_id");
```

### Query Quests

```csharp
// Get active quests
List<IDailyQuest> active = DailyQuestManager.GetActiveQuests();

// Get completed quests
List<IDailyQuest> completed = DailyQuestManager.GetCompletedQuests();

// Get specific quest
IDailyQuest quest = DailyQuestManager.GetQuest("quest_id");

// Check status
bool isActive = DailyQuestManager.IsQuestActive("quest_id");
bool isDone = DailyQuestManager.IsQuestCompleted("quest_id");
```

### Complete Quest (Event-Driven)

```csharp
// Call this when your quest is done
DailyQuestManager.CompleteQuest("quest_id");
```

### Time Info

```csharp
// Last refresh time
DateTime lastRefresh = DailyQuestManager.LastRefreshTime;

// Time until next refresh
TimeSpan timeLeft = DailyQuestManager.TimeUntilNextRefresh;

// Force refresh (testing only)
DailyQuestManager.ForceRefresh();
```

### Events

```csharp
// Quest refresh (new day)
DailyQuestManager.OnQuestsRefreshed += () => { /* ... */ };

// Quest completed
DailyQuestManager.OnQuestCompleted += (quest) => { /* ... */ };

// Quest expired
DailyQuestManager.OnQuestExpired += (quest) => { /* ... */ };
```

---

## Helper Base Classes

### DailyQuestBase

Implements boilerplate for you:

```csharp
public class MyQuest : DailyQuestBase
{
    public override string QuestId => "my_quest";
    public override string Title => "My Title";
    public override QuestCheckMode CheckMode => QuestCheckMode.Scanning;
    
    public override bool IsCompleted() => /* condition */;
    
    public override void OnCompleted()
    {
        GiveMoneyReward(500);
        GiveExpReward(100);
        GiveItemReward(itemId, amount);
    }
}
```

### SimpleScanningQuest

For simple scanning quests:

```csharp
public class MyLocationQuest : SimpleScanningQuest
{
    public override string QuestId => "reach_cave";
    public override string Title => "Explorer";
    
    public override bool IsCompleted()
    {
        // Your location check
    }
    
    public override void OnCompleted()
    {
        GiveMoneyReward(300);
    }
}
```

### SimpleEventQuest

For simple event-driven quests:

```csharp
public class MyKillQuest : SimpleEventQuest
{
    private int kills = 0;
    
    public override string QuestId => "kill_enemies";
    public override string Title => "Hunter";
    public override string Description => $"Kill enemies ({kills}/5)";
    
    public override bool IsCompleted() => kills >= 5;
    
    public override void OnActivated()
    {
        kills = 0;
        Health.OnDead += OnEnemyDead;
    }
    
    private void OnEnemyDead(Health h, DamageInfo info)
    {
        if (/* is enemy */)
        {
            kills++;
            NotifyCompleted(); // Checks and completes if done
        }
    }
}
```

---

## Important Notes

### Quest IDs
- **Must be unique** across all pools
- Use namespacing: `"MyMod.MyQuest"` or `"mymod_quest_01"`

### Rewards
- Framework **does NOT** automatically give rewards
- You **must** handle rewards in `OnCompleted()`
- Use `EconomyManager.Add(money)` for money
- Use `EXPManager.AddExp(exp)` for experience
- Generate items and add to player storage

### Lifecycle
- `OnActivated()` - quest becomes active (subscribe to events)
- `OnCompleted()` - quest finished (give rewards, cleanup)
- `OnExpired()` - day ended without completion (cleanup)

### Save/Load
- Return state in `GetSaveData()` (primitives, strings, serializable)
- Restore in `LoadSaveData(data)`
- Return `null` if no state to save

### Error Handling
- Framework catches and logs all errors
- Your quest won't crash the game
- Check logs for issues

### Performance
- Scanning interval: 0.5-2.0 seconds recommended
- Too low = performance impact
- Use event-driven when possible

---

## Example: Complete Boss Kill Quest

```csharp
public class KillBossQuest : SimpleEventQuest
{
    private int bossKills = 0;
    private const int Required = 1;
    
    public override string QuestId => "daily_kill_boss";
    public override string Title => "Boss Hunter";
    public override string Description => $"Defeat any boss ({bossKills}/{Required})";
    
    public override bool IsCompleted() => bossKills >= Required;
    
    public override void OnActivated()
    {
        bossKills = 0;
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
        // Show notification
    }
    
    private void OnEnemyDead(Health health, DamageInfo info)
    {
        // Check if player killed a boss
        if (!info.fromCharacter?.IsMainCharacter()) return;
        
        var character = health.TryGetCharacter();
        if (character?.characterPreset?.characterIconType == CharacterIconTypes.boss)
        {
            bossKills++;
            NotifyCompleted();
        }
    }
    
    public override object GetSaveData() => bossKills;
    public override void LoadSaveData(object data) => bossKills = (int)data;
}
```

---

## Refresh Cycle

**When:** Every day at midnight UTC

**What happens:**
1. All incomplete quests → `OnExpired()` called
2. All quest pools → `SelectQuestsForToday()` called
3. Selected quests → `OnActivated()` called
4. State saved

**One quest per day:** Each quest can only be completed once per day

---

## UI Integration

### Add Daily Quest Giver to NPC

```csharp
// Add DailyQuestGiver component to NPC GameObject
// If NPC has other interactions, use MultiInteraction:

GameObject npc = /* your NPC */;
var multiInteraction = npc.AddComponent<MultiInteraction>();
var questGiver = npc.AddComponent<QuestGiver>(); // Normal quests
var dailyQuestGiver = npc.AddComponent<DailyQuestGiver>(); // Daily quests

// MultiInteraction will show both options
```

Player will see two interaction options:
- "Quests" (normal)
- "Daily Quests" (our system)

---

## Testing

```csharp
// Force refresh to test
DailyQuestManager.ForceRefresh();

// Check active quests
var quests = DailyQuestManager.GetActiveQuests();
foreach (var q in quests)
{
    Debug.Log($"Active: {q.Title}");
}

// Manually complete (testing)
DailyQuestManager.CompleteQuest("my_quest_id");
```

---

## Common Patterns

### Random Selection
```csharp
public List<IDailyQuest> SelectQuestsForToday(DateTime today)
{
    int seed = today.Year * 10000 + today.DayOfYear;
    var rng = new Random(seed);
    return allQuests.OrderBy(x => rng.Next()).Take(2).ToList();
}
```

### Weighted Selection
```csharp
// Each quest has a weight property
return WeightedRandom(allQuests, q => q.Weight, count: 3);
```

### Sequential (Rotate Daily)
```csharp
public List<IDailyQuest> SelectQuestsForToday(DateTime today)
{
    int dayIndex = today.DayOfYear % allQuests.Count;
    return new List<IDailyQuest> { allQuests[dayIndex] };
}
```

---

## Troubleshooting

**Quest not appearing?**
- Check pool is registered
- Check `SelectQuestsForToday()` returns the quest
- Check `MaxDailyQuests` limit
- Check for duplicate quest IDs

**Quest not completing?**
- For scanning: verify `IsCompleted()` returns true
- For event-driven: verify you call `CompleteQuest()`
- Check logs for errors

**Progress lost on reload?**
- Implement `GetSaveData()` and `LoadSaveData()`
- Return non-null data from `GetSaveData()`

**Performance issues?**
- Use event-driven instead of scanning when possible
- Increase scan interval (1-2 seconds)
- Reduce number of active quests

---

## Summary Checklist

Creating a new quest:
- [ ] Implement `IDailyQuest` interface
- [ ] Choose check mode (Scanning or EventDriven)
- [ ] Implement `IsCompleted()` logic
- [ ] Give rewards in `OnCompleted()`
- [ ] Clean up in `OnExpired()`
- [ ] Subscribe to events in `OnActivated()`
- [ ] Implement save/load if needed
- [ ] Ensure unique quest ID

Creating a quest pool:
- [ ] Implement `IDailyQuestPool` interface
- [ ] Load/create quests in `Initialize()`
- [ ] Implement selection logic in `SelectQuestsForToday()`
- [ ] Register pool in ModBehaviour
- [ ] Ensure unique pool ID

Testing:
- [ ] Register pool and verify it's called
- [ ] Test quest activation
- [ ] Test quest completion
- [ ] Test quest expiration
- [ ] Test save/load
- [ ] Test with multiple quests

---

For detailed information, see **FRAMEWORK_DESIGN.md**