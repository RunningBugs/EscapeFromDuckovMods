# Daily Quest Framework - Design Decisions Log

**Version:** 1.0  
**Last Updated:** 2024

---

## Overview

This document records key design decisions made during the planning phase, including the rationale behind each decision and alternatives considered.

---

## 1. Framework vs. Game Quest System Integration

**Decision:** Create a separate, independent daily quest system instead of integrating with the game's existing Quest/QuestManager system.

**Rationale:**
- Independence: No conflicts with game quest IDs or state
- Flexibility: Can implement custom UI and behavior without game's constraints
- Simplicity: Don't need to work around game's quest assumptions
- Clean separation: Daily quests have different lifecycle than story quests
- No game code modification: Keeps mod self-contained

**Alternatives Considered:**
- **Option A:** Extend game's Quest system
  - Rejected: Would require modifying game code, potential conflicts with quest IDs, complex integration
- **Option B:** Hybrid approach (use Quest objects, separate manager)
  - Rejected: Still dependent on game's Task system, less flexible

**Trade-offs:**
- ✅ Complete control over functionality
- ✅ No game code modification needed
- ⚠️ Need to implement separate UI
- ⚠️ Can't leverage some existing quest UI features

---

## 2. Interface-Based vs. Task-Based Architecture

**Decision:** Use interface-based architecture (IDailyQuest) rather than extending the game's Task system.

**Rationale:**
- Maximum flexibility: Modders can implement quests any way they want
- Simplicity: Don't need to understand game's Task system
- Extensibility: Easy to add any kind of quest logic
- Decoupling: Framework doesn't depend on game's implementation details
- Learning curve: Simpler for modders to understand

**Alternatives Considered:**
- **Option A:** Extend game's Task base class
  - Rejected: Ties framework to game's architecture, requires understanding existing system
- **Option B:** Abstract factory pattern for Task creation
  - Rejected: Still dependent on Task system, more complex

**Trade-offs:**
- ✅ Simple, clear interface
- ✅ Complete implementation freedom
- ⚠️ Can't reuse existing Task implementations
- ⚠️ Need to implement own progress tracking

---

## 3. Quest Checking: Scanning vs. Event-Driven

**Decision:** Support both scanning (polling) and event-driven modes, let modder choose per quest.

**Rationale:**
- Efficiency: Event-driven is more efficient for event-based conditions
- Flexibility: Scanning is easier for state-based conditions (location, possession)
- Choice: Modder can choose the most appropriate approach
- Real-world use cases: Different quest types naturally fit different modes

**Alternatives Considered:**
- **Option A:** Scanning only
  - Rejected: Inefficient for event-based quests, constantly polling
- **Option B:** Event-driven only
  - Rejected: Complex for state-based checks, requires manual polling logic
- **Option C:** Automatic mode detection
  - Rejected: Too magical, hard to predict behavior

**Trade-offs:**
- ✅ Optimal performance for each quest type
- ✅ Modder has control
- ⚠️ Slightly more complex interface
- ⚠️ Need to implement both systems

**Implementation Details:**
- Scanning: Default 0.5s interval, configurable per quest
- Event-driven: Modder calls `CompleteQuest()` manually
- Framework handles both in Update loop

---

## 4. UI Integration Strategy

**Decision:** Create separate DailyQuestGiverView (copy/modify QuestGiverView) rather than modifying game's QuestGiverView.

**Rationale:**
- No game code modification: Keeps mod self-contained
- Flexibility: Can customize UI for daily quest needs
- Safety: Won't break game's quest UI
- Independence: Can evolve UI separately

**Alternatives Considered:**
- **Option A:** Modify QuestGiverView to accept interface
  - Rejected: Requires modifying game code
- **Option B:** Use existing QuestGiverView as-is
  - Rejected: Expects QuestGiver type, not compatible
- **Option C:** Create completely new UI from scratch
  - Considered acceptable, chose to base on existing for consistency

**Trade-offs:**
- ✅ No game code modification
- ✅ Full control over UI
- ⚠️ Code duplication from QuestGiverView
- ⚠️ Need to maintain separate UI

---

## 5. Time Tracking: UTC Midnight

**Decision:** Use UTC midnight for daily refresh, check for day crossing periodically.

**Rationale:**
- Consistency: Same refresh time for all players worldwide
- Simplicity: No timezone complexity
- Reliability: DateTime.UtcNow is stable and reliable
- Following game pattern: BlackMarket uses DateTime.UtcNow
- Clear boundary: Midnight is intuitive reset point

**Alternatives Considered:**
- **Option A:** Local time
  - Rejected: Timezone complexity, DST issues, inconsistent across players
- **Option B:** In-game time (GameClock)
  - Rejected: Not real-world daily, requirement was real-world day
- **Option C:** Fixed interval (24 hours from first quest)
  - Rejected: Not aligned to calendar days

**Implementation Details:**
- Store last refresh date (date only, no time)
- Check periodically if current date > last refresh date
- Default check interval: 60 seconds (configurable)

**Edge Cases Handled:**
- System time going backwards: reset to current time
- Long play session across midnight: refresh during session
- Game closed over midnight: refresh on next launch

---

## 6. Quest Pool Architecture

**Decision:** Registry-based system where modders register IDailyQuestPool implementations.

**Rationale:**
- Extensibility: Any mod can register quest pools
- Flexibility: Pools can generate quests any way (files, hardcode, procedural)
- Separation of concerns: Pool handles selection, framework handles lifecycle
- Multiple sources: Multiple mods can contribute quests

**Alternatives Considered:**
- **Option A:** Single hardcoded pool
  - Rejected: Not extensible
- **Option B:** File-based only
  - Rejected: Not flexible enough, some modders prefer code
- **Option C:** Event-based subscription
  - Rejected: More complex, less clear ownership

**Implementation Details:**
- Pools registered via `RegisterQuestPool()`
- Each pool has unique ID
- On refresh: all pools get `SelectQuestsForToday()` call
- Pools return list of quest instances to activate

---

## 7. Quest Limit Strategy

**Decision:** Unlimited quests by default, optional MaxDailyQuests setting with random sampling.

**Rationale:**
- Flexibility: Let modders decide quest count in their pools
- No artificial limits: Framework shouldn't impose restrictions
- Configurable: Can set limit if needed for balance
- Fairness: All pools get a chance to contribute

**Alternatives Considered:**
- **Option A:** Fixed limit (e.g., 10 quests)
  - Rejected: Arbitrary, might not fit all use cases
- **Option B:** Per-pool limits
  - Rejected: More complex, pools can self-limit
- **Option C:** Priority system
  - Rejected: Too complex, hard to configure

**Implementation:**
- Default: MaxDailyQuests = -1 (unlimited)
- If limit set: collect all quests, random sample to limit
- First-come-first-served if duplicate quest IDs

---

## 8. Reward Handling

**Decision:** Quests handle their own rewards in OnCompleted(), framework does NOT automatically give rewards.

**Rationale:**
- Flexibility: Modders can implement custom reward logic
- Extensibility: Not limited to money/exp/items (could be unlocks, buffs, etc.)
- Control: Quest knows best what rewards to give
- Simplicity: Framework doesn't need reward parsing/granting system
- No assumptions: Framework stays general-purpose

**Alternatives Considered:**
- **Option A:** Framework handles rewards from quest properties
  - Rejected: Less flexible, needs reward parsing, limited to predefined types
- **Option B:** Reward interface system
  - Rejected: Over-engineered for initial version
- **Option C:** Both (framework helpers + manual override)
  - Considered for future, kept simple for now

**Helper Methods Provided:**
- `GiveMoneyReward(amount)` - helper in DailyQuestBase
- `GiveExpReward(amount)` - helper in DailyQuestBase
- `GiveItemReward(itemId, amount)` - helper in DailyQuestBase

---

## 9. Quest State Persistence

**Decision:** Implement save/load system with GetSaveData()/LoadSaveData() methods.

**Rationale:**
- Persistence: Progress survives logout/reload
- Player friendly: Don't lose progress if game crashes
- Expected behavior: Players expect progress to persist
- Professional: Complete quest system behavior
- Optional: Return null if no state needed

**Alternatives Considered:**
- **Option A:** No save/load
  - Rejected: Poor player experience, progress lost
- **Option B:** Automatic serialization
  - Rejected: Complex, reflection overhead, limited to serializable types
- **Option C:** Framework-managed state
  - Rejected: Less flexible, framework needs to know state structure

**Implementation:**
- Quest returns arbitrary object from GetSaveData()
- Framework stores in save file with quest ID
- On load: framework calls LoadSaveData() with saved object
- Namespace: "DailyQuests/{questId}/data"
- Save on: quest completion, day refresh, periodic autosave

---

## 10. Error Handling Strategy

**Decision:** Catch all errors from modder code, log them, and continue execution.

**Rationale:**
- Robustness: One bad quest won't crash the system
- Debug friendly: Errors logged for modders to fix
- Graceful degradation: Other quests continue working
- Safety: Framework protects itself and the game

**Areas Protected:**
- Quest.OnActivated()
- Quest.OnCompleted()
- Quest.OnExpired()
- Quest.IsCompleted()
- Pool.Initialize()
- Pool.SelectQuestsForToday()

**Implementation:**
```csharp
try {
    quest.OnActivated();
} catch (Exception e) {
    Debug.LogError($"Error activating quest {quest.QuestId}: {e}");
    // Continue with other quests
}
```

---

## 11. Quest ID Uniqueness

**Decision:** Enforce unique quest IDs across all pools, first-come-first-served on conflicts.

**Rationale:**
- Clarity: Clear identification of each quest
- No conflicts: Prevents two quests with same ID
- Simple resolution: First registered wins
- Logging: Warn about duplicates

**Alternatives Considered:**
- **Option A:** Allow duplicates, namespace by pool
  - Rejected: Confusing, which quest is "quest_01"?
- **Option B:** Enforce uniqueness, reject duplicates
  - Chosen approach
- **Option C:** Automatic ID prefix by pool
  - Rejected: Less control for modders

**Best Practice:**
- Use namespacing: "MyMod.MyQuest"
- Or prefixing: "mymod_quest_01"
- Document in modder guide

---

## 12. Helper Base Classes

**Decision:** Provide DailyQuestBase, SimpleScanningQuest, SimpleEventQuest helper classes.

**Rationale:**
- Convenience: Reduce boilerplate for common cases
- Best practices: Encourage good patterns
- Reward helpers: Consistent reward granting
- Optional: Can still implement interface directly

**Alternatives Considered:**
- **Option A:** Interface only
  - Rejected: Too much boilerplate for simple quests
- **Option B:** Code generation
  - Rejected: Over-engineered
- **Option C:** Many specialized base classes
  - Rejected: Too many options, confusing

**Provided:**
- DailyQuestBase - general base with reward helpers
- SimpleScanningQuest - for scanning mode
- SimpleEventQuest - for event-driven mode

---

## 13. File-Based Quest System (Phase 5)

**Decision:** File-based quest pool is an example implementation, NOT part of core framework.

**Rationale:**
- Framework agnostic: Core doesn't assume quest source
- Optional: Not everyone needs file-based quests
- Extensibility: Multiple file formats possible
- Example: Shows how to implement a pool
- Flexibility: Modders can create own parsers

**Implementation Approach:**
- FileBasedQuestPool implements IDailyQuestPool
- Custom parser reads quest definition files
- Factory pattern for creating quest instances
- Meta.ini for selection configuration
- Documentation as reference

---

## 14. Progress Display

**Decision:** Text-based progress in Description, no progress bars initially.

**Rationale:**
- Simplicity: Easier to implement
- Flexibility: Quest controls format
- No UI changes: Use existing quest detail panel
- Future proof: Can add progress bars later if needed

**Example:**
```csharp
public override string Description => $"Kill enemies ({kills}/{required})";
```

**Alternatives Considered:**
- **Option A:** Progress bar interface
  - Deferred to future (Phase 8)
- **Option B:** Percentage property
  - Rejected: Not all quests have numeric progress

---

## 15. Initialization Timing

**Decision:** Pool.Initialize() may be called immediately on registration or deferred until first refresh (to be decided during implementation).

**Rationale:**
- Performance: Avoid loading all quests at startup if not needed
- Flexibility: Some pools might need game systems fully initialized
- Uncertainty: Need to test both approaches

**Options:**
- **Immediate:** Call on RegisterQuestPool()
  - Pro: Errors caught early
  - Con: Startup performance hit
- **Deferred:** Call on first refresh
  - Pro: Better startup performance
  - Con: Errors not caught until later

**Resolution:** Decide during Phase 1 implementation based on testing.

---

## 16. Multi-Interaction Setup for NPCs

**Decision:** Use game's existing MultiInteraction system for NPCs with multiple interaction options.

**Rationale:**
- Reuse existing system: Game already has multi-interaction menu
- Familiar UX: Players know this pattern
- Simple integration: Just add DailyQuestGiver component
- No game modification: Uses existing components

**Implementation:**
- NPC has MultiInteraction component
- MultiInteraction references both QuestGiver and DailyQuestGiver
- Player sees menu: "Quests" and "Daily Quests"
- Each opens respective UI

---

## 17. Notification System

**Decision:** Notifications for quest completion and expiration (Phase 7 - polish).

**Rationale:**
- Player feedback: Confirm actions
- Important events: Shouldn't miss completion/expiration
- Deferred: Not critical for MVP
- Extensibility: Quests can show own notifications

**Implementation:**
- Framework fires events (OnQuestCompleted, OnQuestExpired)
- UI/notification system subscribes to events
- Toast messages or similar
- Optional: Modders can handle in quest itself

---

## 18. Localization

**Decision:** Support localization keys for framework UI, allow quests to use localization (Phase 7).

**Rationale:**
- International support: Game supports multiple languages
- Best practice: Framework should be localizable
- Quest content: Let modders handle quest text localization
- Deferred: Not critical for MVP

---

## 19. Performance Considerations

**Decisions Made:**
- Default scan interval: 0.5 seconds (configurable)
- Day crossing check: 60 seconds (configurable)
- Recommend event-driven over scanning when possible
- Profile and optimize if needed

**Rationale:**
- Balance: Responsive but not wasteful
- Configurable: Let users tune for their needs
- Best practices: Guide modders to efficient patterns

---

## 20. Future Extensibility

**Decisions Deferred to Future (Phase 8):**
- Quest dependencies/prerequisites
- Quest chains (multi-day sequences)
- Difficulty tiers
- Streak bonuses
- Quest history view
- Quest preview (see tomorrow)
- Manual quest selection
- Progress bars
- Quest icons

**Rationale:**
- MVP first: Get core working well
- Avoid feature creep: Keep initial version simple
- Learn from usage: See what users actually need
- Maintain compatibility: Can add via optional interfaces

---

## Summary

The framework design prioritizes:

1. **Flexibility** - Support multiple implementation approaches
2. **Simplicity** - Clear interfaces, minimal boilerplate
3. **Independence** - Separate from game's quest system
4. **Extensibility** - Easy for modders to add content
5. **Robustness** - Error handling, state persistence
6. **Performance** - Efficient checking, configurable intervals

These decisions create a solid foundation for a general-purpose daily quest system that can support a wide variety of implementations and use cases.

---

**Next Steps:** Begin Phase 1 implementation based on these decisions.