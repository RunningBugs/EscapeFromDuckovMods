# Daily Quest Framework - Implementation Plan

**Version:** 1.0  
**Status:** Planning Phase

---

## Overview

This document outlines the step-by-step implementation plan for the Daily Quest Framework. The implementation is divided into phases, each building on the previous one.

---

## Phase 1: Core Framework

**Goal:** Implement the basic framework without UI or example quests.

### 1.1 Project Setup
- [x] Create `DailyQuestMod/` directory
- [x] Write design documents
- [ ] Create `DailyQuestMod.csproj`
- [ ] Create `info.ini`
- [ ] Set up project references (Assembly-CSharp, Unity assemblies)
- [ ] Create folder structure

### 1.2 Framework Interfaces
- [ ] Create `Framework/IDailyQuest.cs`
  - Quest interface with all properties and methods
  - QuestCheckMode enum
  - XML documentation comments
  
- [ ] Create `Framework/IDailyQuestPool.cs`
  - Pool interface
  - XML documentation comments

### 1.3 Framework Manager
- [ ] Create `Framework/DailyQuestManager.cs` (static API)
  - Configuration properties (MaxDailyQuests, DayCrossingCheckInterval)
  - Registration methods (RegisterQuestPool, UnregisterQuestPool)
  - Query methods (GetActiveQuests, GetCompletedQuests, etc.)
  - CompleteQuest method
  - Time properties (LastRefreshTime, TimeUntilNextRefresh)
  - Events (OnQuestsRefreshed, OnQuestCompleted, OnQuestExpired)
  - ForceRefresh method

- [ ] Create `Framework/DailyQuestManagerInternal.cs`
  - MonoBehaviour implementation
  - Quest data structure (QuestData class)
  - Update loop (day crossing check, scanning quests)
  - RefreshDailyQuests implementation
  - CompleteQuestInternal implementation
  - Quest ID uniqueness enforcement
  - Error handling (try-catch around all modder code)
  - State management

### 1.4 Save/Load System
- [ ] Implement save data structure
  - Last refresh date
  - Active quest IDs and pool IDs
  - Quest save data
  - Completed quest IDs

- [ ] Implement SaveState method
  - Serialize to JSON/binary
  - Save via SavesSystem
  - Namespace: "DailyQuests/"

- [ ] Implement LoadState method
  - Load from SavesSystem
  - Reconstruct quest instances from pools
  - Restore quest state via LoadSaveData
  - Handle missing pools/quests gracefully

### 1.5 Testing
- [ ] Create simple test quest (hardcoded)
- [ ] Create simple test pool
- [ ] Test registration
- [ ] Test day crossing logic (mock time)
- [ ] Test quest activation
- [ ] Test quest completion (scanning & event-driven)
- [ ] Test quest expiration
- [ ] Test save/load
- [ ] Test error handling
- [ ] Test quest ID uniqueness

**Deliverable:** Working framework that can register pools, activate quests, check completion, and handle refresh cycle.

---

## Phase 2: Helper Utilities

**Goal:** Provide convenience base classes for common quest patterns.

### 2.1 Base Classes
- [ ] Create `Helpers/DailyQuestBase.cs`
  - Abstract base implementing IDailyQuest
  - Default implementations for optional methods
  - Helper methods: GiveMoneyReward, GiveExpReward, GiveItemReward
  - Documentation and examples

- [ ] Create `Helpers/SimpleScanningQuest.cs`
  - Extends DailyQuestBase
  - CheckMode = Scanning
  - Default ScanInterval = 1.0f
  - Documentation

- [ ] Create `Helpers/SimpleEventQuest.cs`
  - Extends DailyQuestBase
  - CheckMode = EventDriven
  - NotifyCompleted() helper method
  - Documentation

### 2.2 Reward Helpers
- [ ] Implement money reward logic
  - EconomyManager.Add()
  - Notification/feedback

- [ ] Implement exp reward logic
  - EXPManager.AddExp()
  - Notification/feedback

- [ ] Implement item reward logic
  - ItemAssetsCollection.InstantiateAsync()
  - PlayerStorage.Push()
  - Notification/feedback

### 2.3 Testing
- [ ] Test base classes with example quests
- [ ] Test reward helpers
- [ ] Verify simplification benefits

**Deliverable:** Helper classes that make quest creation easier and more consistent.

---

## Phase 3: UI Integration

**Goal:** Implement UI for viewing and interacting with daily quests.

### 3.1 Daily Quest Giver Component
- [ ] Create `Integration/DailyQuestGiver.cs`
  - Extend InteractableBase
  - Override InteractName (localized "Daily Quests")
  - OnInteractStart: open DailyQuestGiverView
  - OnInteractStop: close view
  - Documentation

### 3.2 Daily Quest Giver View
- [ ] Create `Integration/UI/DailyQuestGiverView.cs`
  - Based on QuestGiverView.cs structure
  - Singleton pattern (Instance property)
  - View lifecycle (Open/Close)
  - Quest list display
  - Quest detail panel
  - Tab system (Available/Completed)
  - Complete button (for manual submissions)
  - Refresh on quest state changes

- [ ] Create UI prefab (if needed)
  - Quest entry prefab
  - Detail panel
  - Tabs
  - Scrollable list

### 3.3 Quest Entry Display
- [ ] Quest entry component
  - Show title
  - Show completion status
  - Selection highlight
  - Click to show details

- [ ] Quest detail panel
  - Show title
  - Show description (with progress)
  - Show rewards (if applicable)
  - Complete button (if interactable)

### 3.4 Testing
- [ ] Test UI opening/closing
- [ ] Test quest list display
- [ ] Test quest details
- [ ] Test tab switching
- [ ] Test quest completion via UI
- [ ] Test with multiple quests
- [ ] Test with no quests

**Deliverable:** Working UI for viewing and completing daily quests.

---

## Phase 4: Example Implementations

**Goal:** Provide example quests and pools to demonstrate framework usage.

### 4.1 Example Quests
- [ ] Create `Examples/Quests/KillBossQuest.cs`
  - Event-driven
  - Subscribe to Health.OnDead
  - Check for boss type (characterIconType)
  - Track kill count
  - Give rewards
  - Documentation

- [ ] Create `Examples/Quests/KillMobsQuest.cs`
  - Event-driven
  - Subscribe to Health.OnDead
  - Check for specific mob preset (not boss, not friendly)
  - Track kill count (configurable)
  - Give rewards
  - Documentation

- [ ] Create `Examples/Quests/SubmitItemsQuest.cs`
  - Event-driven or scanning
  - Check item possession
  - Handle item submission
  - Give rewards
  - Documentation

- [ ] Create `Examples/Quests/ReachLocationQuest.cs`
  - Scanning mode
  - Check player distance to location
  - Give rewards
  - Documentation

### 4.2 Example Pool
- [ ] Create `Examples/Pools/ExampleQuestPool.cs`
  - Hardcoded quest instances
  - Random selection logic (with seed)
  - Documentation
  - Comments explaining selection logic

### 4.3 Testing
- [ ] Test each example quest individually
- [ ] Test example pool selection
- [ ] Test full daily cycle with examples
- [ ] Verify examples work as documented

**Deliverable:** Working example quests demonstrating framework capabilities.

---

## Phase 5: File-Based Quest Pool (Optional)

**Goal:** Allow quests to be defined in human-readable text files.

### 5.1 Quest Definition Format
- [ ] Design file format (INI-like or custom)
  - Quest metadata (id, title, description)
  - Quest type/handler
  - Quest parameters
  - Rewards
  - Example files

### 5.2 Quest Parser
- [ ] Create `Examples/Pools/QuestParser.cs`
  - Parse quest definition files
  - Validate format
  - Error handling and reporting
  - Support for comments

### 5.3 Quest Factory Registry
- [ ] Create quest factory system
  - Register quest type handlers
  - Map type string to factory function
  - Factory creates quest instance from parameters
  - Documentation

### 5.4 File-Based Pool
- [ ] Create `Examples/Pools/FileBasedQuestPool.cs`
  - Scan directory for .txt files
  - Parse all quest files
  - Implement selection logic (configurable)
  - Meta.ini support (optional)
  - Error handling
  - Documentation

### 5.5 Example Quest Definitions
- [ ] Create `Examples/QuestDefinitions/BossKilling/` folder
  - Example boss kill quest files
  - Comments explaining format

- [ ] Create `Examples/QuestDefinitions/ItemSubmission/` folder
  - Example item submission quest files

- [ ] Create `Examples/QuestDefinitions/MobKilling/` folder
  - Example mob kill quest files

### 5.6 Testing
- [ ] Test quest file parsing
- [ ] Test quest factory system
- [ ] Test file-based pool
- [ ] Test with various quest types
- [ ] Test error handling (malformed files)

**Deliverable:** File-based quest system for easy content creation.

---

## Phase 6: NPC Integration

**Goal:** Add daily quest functionality to existing NPC (Jeff).

### 6.1 Find Jeff
- [ ] Locate Jeff NPC in game files (prefab or scene)
- [ ] Document Jeff's current setup
- [ ] Identify quest giver component

### 6.2 Add Multi-Interaction
- [ ] Determine if Jeff uses MultiInteraction or single InteractableBase
- [ ] If single: convert to MultiInteraction setup
- [ ] Add DailyQuestGiver component to Jeff
- [ ] Configure MultiInteraction to show both options:
  - Normal quests (existing QuestGiver)
  - Daily quests (new DailyQuestGiver)

### 6.3 Testing
- [ ] Test Jeff interaction in-game
- [ ] Verify multi-interaction menu shows both options
- [ ] Test daily quest UI from Jeff
- [ ] Test normal quest UI still works

**Deliverable:** Jeff NPC with daily quest functionality.

---

## Phase 7: Polish & Documentation

**Goal:** Improve user experience and provide comprehensive documentation.

### 7.1 Notifications
- [ ] Quest completed notification (toast/popup)
- [ ] Quest expired notification
- [ ] Daily refresh notification (optional)
- [ ] Reward received notification

### 7.2 Localization
- [ ] Localization keys for framework UI text
- [ ] Support for localized quest text
- [ ] Documentation for localization

### 7.3 Error Messages
- [ ] User-friendly error messages
- [ ] Debug logging for development
- [ ] Graceful degradation on errors

### 7.4 Documentation
- [ ] Update FRAMEWORK_DESIGN.md with any changes
- [ ] Update QUICK_REFERENCE.md with final API
- [ ] Create MODDER_GUIDE.md
  - Step-by-step tutorial
  - Best practices
  - Common pitfalls
  - FAQ

- [ ] Create README.md
  - Overview
  - Installation
  - Quick start
  - Links to other docs

- [ ] Code documentation
  - XML comments on all public APIs
  - Example code in comments
  - Warning comments where needed

### 7.5 Testing & QA
- [ ] Full playthrough test (multiple days)
- [ ] Test save/load at various states
- [ ] Test with mod disabled/re-enabled
- [ ] Test with other mods
- [ ] Test edge cases:
  - No quests available
  - All pools fail
  - System time change
  - Long game session across midnight

### 7.6 Performance
- [ ] Profile Update() method
- [ ] Optimize scanning loop if needed
- [ ] Test with many active quests (50+)
- [ ] Memory leak check

**Deliverable:** Polished, well-documented framework ready for release.

---

## Phase 8: Advanced Features (Future)

**Goal:** Optional enhancements based on user feedback.

### Potential Features
- [ ] Quest chains (multi-day sequences)
- [ ] Quest dependencies (prerequisite quests)
- [ ] Difficulty tiers
- [ ] Streak bonuses
- [ ] Quest history view
- [ ] Quest preview (see tomorrow's quests)
- [ ] Manual quest selection (player chooses)
- [ ] Quest categories/tags
- [ ] Progress bars in UI
- [ ] Custom quest icons
- [ ] Sound effects for completion
- [ ] Quest abandonment
- [ ] Quest reroll (limited)
- [ ] Weekly quests
- [ ] Seasonal quests

---

## Current Status

**Phase:** Planning Complete  
**Next Step:** Begin Phase 1 - Core Framework

---

## Notes

### Development Environment
- Unity version: (check game version)
- .NET Standard 2.1
- Required references:
  - Assembly-CSharp
  - UnityEngine
  - UnityEngine.UI
  - TextMeshPro
  - Cysharp.Threading.Tasks (UniTask)

### Coding Standards
- Follow game's coding style
- Use XML documentation comments
- Include error handling
- Log important events
- Use meaningful variable names
- Keep methods focused and small

### Testing Strategy
- Unit test each component in isolation
- Integration test with game systems
- Playtest with real gameplay
- Test edge cases and error conditions
- Test performance with many quests

### Git Workflow (if applicable)
- Feature branches for each phase
- Commit after each working feature
- Tag versions (v0.1-phase1, v0.2-phase2, etc.)
- Maintain changelog

---

## Timeline Estimates

**Phase 1:** 8-12 hours (core framework)  
**Phase 2:** 3-4 hours (helper utilities)  
**Phase 3:** 6-8 hours (UI integration)  
**Phase 4:** 4-6 hours (example implementations)  
**Phase 5:** 6-8 hours (file-based quests, optional)  
**Phase 6:** 2-3 hours (NPC integration)  
**Phase 7:** 4-6 hours (polish & docs)  

**Total:** 33-47 hours (without Phase 5)  
**With Phase 5:** 39-55 hours

*Note: Times are estimates and may vary based on unforeseen issues.*

---

## Success Criteria

### Minimum Viable Product (MVP)
- ✅ Framework can register quest pools
- ✅ Framework refreshes quests daily at midnight UTC
- ✅ Quests can be completed (scanning & event-driven)
- ✅ Quest state persists across sessions
- ✅ UI displays daily quests
- ✅ Example quests demonstrate functionality
- ✅ Jeff NPC provides daily quests

### Full Release
- ✅ All MVP criteria
- ✅ Helper base classes for easy quest creation
- ✅ Comprehensive documentation
- ✅ Example implementations for all quest types
- ✅ File-based quest pool (if Phase 5 included)
- ✅ Notifications for important events
- ✅ Stable, tested, performant

---

## Risk Assessment

### High Risk
- **Save/load compatibility**: Changes to save format require migration
- **Performance with many quests**: Scanning many quests could impact FPS
- **Time zone edge cases**: Daylight saving, time changes

### Medium Risk
- **UI integration**: May need iteration to get UX right
- **NPC modification**: Jeff's setup may be complex
- **Quest ID conflicts**: Multiple mods using same IDs

### Low Risk
- **Core framework**: Well-designed interfaces minimize risk
- **Example quests**: Straightforward implementations
- **Documentation**: Time-consuming but low risk

### Mitigation Strategies
- Save format versioning and migration code
- Performance profiling and optimization
- Thorough testing of time logic
- Iterative UI development with feedback
- Clear ID naming conventions in documentation

---

## Dependencies

### Required Game Systems
- SavesSystem (for persistence)
- EconomyManager (for money rewards)
- EXPManager (for experience rewards)
- ItemAssetsCollection (for item rewards)
- PlayerStorage (for item storage)
- Health.OnDead event (for kill quests)
- CharacterMainControl (for player reference)
- InteractableBase (for NPC interaction)
- View system (for UI)

### Optional Game Systems
- Localization system (for translated text)
- Audio system (for sound effects)
- Notification system (for toasts)

---

## Next Steps

1. Review this implementation plan
2. Confirm Phase 1 approach
3. Set up project structure
4. Begin implementing Phase 1.1 (Project Setup)
5. Continue with Phase 1.2 onwards

**Ready to begin implementation when approved.**