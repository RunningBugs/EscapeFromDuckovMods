# Daily Quest Framework

A flexible, extensible framework for implementing daily quests in Duckov.

## Overview

The Daily Quest Framework provides a plugin-based system for creating daily quests that refresh every real-world day (midnight UTC). The framework is designed to be general-purpose and easily extensible by other modders.

### Key Features

- **Flexible Quest System**: Support for both event-driven and scanning (polling) quest checking
- **Extensible Architecture**: Modders can easily add custom quest types and quest pools
- **Automatic Daily Refresh**: Quests refresh at midnight UTC each day
- **Quest Expiration**: Incomplete quests expire at end of day
- **Save/Load Support**: Quest progress persists across game sessions
- **Separate from Game Quests**: Independent system that doesn't conflict with in-game quests
- **UI Integration**: NPC interaction for viewing and completing daily quests

## Documentation

- **[FRAMEWORK_DESIGN.md](FRAMEWORK_DESIGN.md)** - Complete framework design and architecture
- **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** - Quick reference for common tasks
- **[IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md)** - Development roadmap

## Quick Start

### For Users

1. Install the mod in your `Mods/` folder
2. Build or access the Commemorative Trophy in your base
3. Interact with the trophy - you'll see "Daily Quests" option
4. View available daily quests
5. Complete quests to earn rewards
6. Quests refresh daily at midnight UTC
7. **Testing keybind:** Press 'K' to open Daily Quest UI directly

### For Modders - Creating a Simple Quest

```csharp
using DailyQuestMod.Framework;
using DailyQuestMod.Helpers;

public class MyQuest : SimpleEventQuest
{
    private int progress = 0;
    
    public override string QuestId => "mymod_my_quest";
    public override string Title => "My First Quest";
    public override string Description => $"Do something ({progress}/5)";
    
    public override bool IsCompleted() => progress >= 5;
    
    public override void OnActivated()
    {
        progress = 0;
        // Subscribe to game events
    }
    
    public override void OnCompleted()
    {
        GiveMoneyReward(500);
        GiveExpReward(100);
    }
}
```

### For Modders - Registering a Quest Pool

```csharp
using DailyQuestMod.Framework;

public class MyQuestPool : IDailyQuestPool
{
    public string PoolId => "MyModQuests";
    
    private List<IDailyQuest> quests;
    
    public void Initialize()
    {
        quests = new List<IDailyQuest>
        {
            new MyQuest(),
            // Add more quests...
        };
    }
    
    public List<IDailyQuest> SelectQuestsForToday(DateTime today)
    {
        // Your selection logic (random, weighted, etc.)
        return quests.Take(2).ToList();
    }
}

// In your ModBehaviour:
public override void OnAfterSetup()
{
    DailyQuestManager.RegisterQuestPool(new MyQuestPool());
}
```

## Core Concepts

### Quest Check Modes

**Scanning Mode:**
- Framework automatically checks `IsCompleted()` at regular intervals
- Good for: location checks, possession checks, time-based conditions
- Default interval: 0.5 seconds (configurable)

**Event-Driven Mode:**
- You manually call `DailyQuestManager.CompleteQuest(questId)` when done
- Good for: kill counts, item submissions, specific game events
- More efficient than scanning

### Quest Lifecycle

1. **OnActivated()** - Quest becomes active (subscribe to events, initialize state)
2. **IsCompleted()** - Check if quest objectives are met
3. **OnCompleted()** - Quest finished successfully (give rewards, cleanup)
4. **OnExpired()** - Day ended without completion (cleanup)

### Rewards

**Important:** The framework does NOT automatically give rewards. You must handle rewards in `OnCompleted()`:

```csharp
public override void OnCompleted()
{
    GiveMoneyReward(500);        // Give money
    GiveExpReward(100);          // Give experience
    GiveItemReward(itemId, 5);   // Give items
}
```

## Framework API

### Configuration

```csharp
// Max daily quests (-1 = unlimited)
DailyQuestManager.MaxDailyQuests = 10;

// Day crossing check interval (seconds)
DailyQuestManager.DayCrossingCheckInterval = 60f;
```

### Quest Management

```csharp
// Get active quests
List<IDailyQuest> active = DailyQuestManager.GetActiveQuests();

// Get completed quests
List<IDailyQuest> completed = DailyQuestManager.GetCompletedQuests();

// Complete a quest (event-driven mode)
DailyQuestManager.CompleteQuest("quest_id");

// Check quest status
bool isActive = DailyQuestManager.IsQuestActive("quest_id");
bool isDone = DailyQuestManager.IsQuestCompleted("quest_id");
```

### Trophy Integration

The mod automatically integrates with the Commemorative Trophy:

```csharp
// Check integration status
bool integrated = TrophyIntegration.IsIntegrated();

// Get the daily quest giver instance
DailyQuestGiver giver = TrophyIntegration.GetDailyQuestGiver();

// Manual integration (usually automatic)
TrophyIntegration.IntegrateWithTrophy();
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

## Helper Base Classes

### DailyQuestBase
Abstract base class with common functionality and reward helpers.

### SimpleScanningQuest
For quests that use scanning mode (automatic checking).

### SimpleEventQuest
For quests that use event-driven mode (manual completion).

See [QUICK_REFERENCE.md](QUICK_REFERENCE.md) for detailed examples.

## Example Quest Types

The mod includes example implementations:

- **KillBossQuest** - Defeat any boss creature (event-driven)
- **KillMobsQuest** - Kill specific mob types (event-driven)
- **SubmitItemsQuest** - Submit items to complete quest (scanning mode)
- **TestQuest** - Simple 10-second timer quest (for testing)
- **TestEventQuest** - Spacebar press counter quest (for testing)

## Architecture

```
Framework (Core)
├── IDailyQuest - Quest interface
├── IDailyQuestPool - Pool interface
├── DailyQuestManager - Static API
└── DailyQuestManagerInternal - Internal implementation

Helpers (Convenience)
├── DailyQuestBase - Base quest class
├── SimpleScanningQuest - Scanning quest base
└── SimpleEventQuest - Event quest base

Integration (Game)
├── DailyQuestGiver - NPC component
└── DailyQuestGiverView - UI view

Examples (Reference)
├── Pools/ - Example quest pools
└── Quests/ - Example quest implementations
```

## Requirements

- Duckov game
- Mod loader with ModBehaviour support
- .NET Standard 2.1

## Installation

1. Download the latest release
2. Extract to `Duckov/Mods/DailyQuestMod/`
3. Launch the game
4. Mod will be loaded automatically

## Development Status

**Current Phase:** Planning Complete  
**Next Phase:** Core Framework Implementation

See [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) for detailed roadmap.

## Contributing

Contributions are welcome! Please:

1. Read the design documents
2. Follow the coding standards
3. Test thoroughly
4. Document your changes
5. Submit a pull request

## Important Notes

### Quest IDs
- Must be unique across all pools
- Use namespacing: `"MyMod.MyQuest"` or `"mymod_quest_01"`

### Performance
- Scanning interval: 0.5-2.0 seconds recommended
- Use event-driven mode when possible for better performance

### Error Handling
- Framework catches and logs all errors from modder code
- Your quest won't crash the game
- Check logs for issues

## FAQ

**Q: When do quests refresh?**  
A: Every day at midnight UTC (00:00).

**Q: Can I complete a quest multiple times per day?**  
A: No, each quest can only be completed once per day.

**Q: What happens to incomplete quests?**  
A: They expire at midnight and are removed. `OnExpired()` is called for cleanup.

**Q: Do quest rewards automatically appear?**  
A: No, you must handle rewards in `OnCompleted()` callback.

**Q: Can I have unlimited daily quests?**  
A: Yes, set `DailyQuestManager.MaxDailyQuests = -1` (default).

**Q: How do I test my quests?**  
A: Use `DailyQuestManager.ForceRefresh()` to manually trigger a refresh.

**Q: Can I create quests from text files?**  
A: Yes, see FileBasedQuestPool example (Phase 5 of implementation, optional).

**Q: Where do I access daily quests?**  
A: Interact with the Commemorative Trophy building in your base. The mod automatically adds daily quests to it.

**Q: Can I add daily quests to other buildings/NPCs?**  
A: Yes! Add a `DailyQuestGiver` component to any GameObject with `InteractableBase`. See TrophyIntegration.cs for an example.

## License

[Specify your license here]

## Credits

- Framework design and implementation: [Your name]
- Based on Duckov modding system by TeamSoda

## Support

For issues, questions, or suggestions:
- GitHub Issues: [Your repository]
- Discord: [Your server]
- Email: [Your email]

## See Also

- [BossLiveMapMod](../BossLiveMapMod/) - Example mod showing boss detection (used for KillBossQuest)
- [MyDuckovMod](../MyDuckovMod/) - Mod template
- [Duckov Modding Guide](../duckov_modding_guide/) - General modding documentation

## Testing

**In-Game Testing:**
1. Build the Commemorative Trophy in your base
2. The mod will automatically add daily quests to it (check console for "[TrophyIntegration]" messages)
3. Interact with the trophy to see the daily quest menu
4. Press 'K' anywhere to open the daily quest UI directly (test keybind)

**Debug Mode:**
- Set `#if DEBUG` in ModBehaviour.cs to register test quests
- Test quests will appear in the daily quest pool
- Check console logs for "[DailyQuestMod]" prefix messages

---

**For detailed information, see [FRAMEWORK_DESIGN.md](FRAMEWORK_DESIGN.md) and [QUICK_REFERENCE.md](QUICK_REFERENCE.md).**