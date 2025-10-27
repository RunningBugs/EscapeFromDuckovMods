# Daily Quest Framework - Testing Guide

**Version:** 1.0  
**Last Updated:** 2024

---

## Overview

This guide provides step-by-step instructions for testing the Daily Quest Framework in-game.

---

## Pre-Testing Checklist

### Build the Mod

```bash
cd DailyQuestMod
dotnet build -c Release
```

**Expected Result:** Build succeeds with 0 errors, 0 warnings

### Install the Mod

1. Copy `bin/Release/DailyQuestMod.dll` to game's `Mods/DailyQuestMod/` folder
2. Copy `info.ini` to `Mods/DailyQuestMod/`
3. Launch the game

---

## In-Game Testing

### Test 1: Mod Initialization

**Objective:** Verify the mod loads correctly

**Steps:**
1. Launch the game
2. Open the console (if available) or check the log file

**Expected Console Output:**
```
[DailyQuestMod] Initializing Daily Quest Framework...
[DailyQuestMod] Daily Quest Framework initialized successfully
[DailyQuestMod] Modders can now register quest pools via DailyQuestManager.RegisterQuestPool()
[DailyQuestMod] Will integrate with Commemorative Trophy when available
```

**Success Criteria:**
- ✅ No error messages
- ✅ Initialization message appears
- ✅ Framework is ready

---

### Test 2: Trophy Integration

**Objective:** Verify DailyQuestGiver is added to Commemorative Trophy

**Steps:**
1. Load a save game or start a new game
2. Build or locate the Commemorative Trophy in your base
3. Watch the console for integration messages

**Expected Console Output:**
```
[TrophyIntegration] Searching for Commemorative Trophy...
[TrophyIntegration] Found trophy: [TrophyName]
[TrophyIntegration] Added DailyQuestGiver component
[TrophyIntegration] Successfully integrated with Commemorative Trophy!
[DailyQuestMod] Trophy integration successful, stopping retry attempts
```

**Success Criteria:**
- ✅ Trophy found automatically
- ✅ DailyQuestGiver component added
- ✅ Integration successful message

**Troubleshooting:**
- If trophy not found, check console for search attempts every 5 seconds
- Try building the trophy if not present in base
- Check that the trophy has an InteractableBase component

---

### Test 3: UI Access via Keybind

**Objective:** Test direct UI access using the K key

**Steps:**
1. In-game, press the **K** key
2. Observe if the Daily Quest UI opens

**Expected Console Output:**
```
[DailyQuestMod] Test keybind pressed - attempting to open Daily Quest UI
[DailyQuestMod] Daily Quest UI opened successfully
```

**Expected Behavior:**
- ✅ Daily Quest UI window appears
- ✅ UI shows quest list (empty or with test quests)
- ✅ Can close the UI

**Success Criteria:**
- ✅ UI opens without errors
- ✅ UI is visible and interactive
- ✅ No crashes or exceptions

**Troubleshooting:**
- If "DailyQuestGiverView.Instance is null", the UI prefab may not be loaded
- Check console for error messages
- Verify DailyQuestGiverView is properly initialized

---

### Test 4: Trophy Interaction

**Objective:** Access daily quests through the Commemorative Trophy

**Steps:**
1. Approach the Commemorative Trophy
2. Press F to interact
3. Look for "Daily Quests" option in the interaction menu

**Expected Behavior:**
- ✅ Interaction menu appears with multiple options
- ✅ "Daily Quests" option is visible
- ✅ Selecting "Daily Quests" opens the daily quest UI

**Expected Console Output:**
```
[DailyQuestGiver] Opened daily quest UI
```

**Success Criteria:**
- ✅ MultiInteraction menu shows all options including "Daily Quests"
- ✅ Daily Quest UI opens when selected
- ✅ UI functions correctly

**Troubleshooting:**
- If "Daily Quests" option not visible, MultiInteraction may not be set up
- Check console for TrophyIntegration messages
- Verify trophy has DailyQuestGiver component

---

### Test 5: Quest Registration (Debug Mode)

**Objective:** Verify test quests are registered

**Prerequisites:** Build with `#if DEBUG` enabled in ModBehaviour.cs

**Steps:**
1. Open the Daily Quest UI (K key or trophy interaction)
2. Check if test quests appear in the quest list

**Expected Quests:**
- "Test Quest" - 10 second timer quest
- OR "Test Event Quest" - spacebar press quest
(Alternates daily based on day of year)

**Expected Console Output:**
```
[DailyQuestMod] Debug mode - registering test quest pool
[TestQuestPool] Initializing...
[TestQuestPool] Initialized with 2 quests
[DailyQuestManager] Pool registered: TestQuestPool
[DailyQuestManager] First pool registered, triggering initial refresh
```

**Success Criteria:**
- ✅ Test quest pool registered
- ✅ Quest(s) appear in UI
- ✅ Quest details visible (title, description)

---

### Test 6: Quest Activation

**Objective:** Test quest lifecycle (activation)

**Steps:**
1. Open Daily Quest UI
2. Select a quest from the list
3. Observe quest activation

**Expected Console Output:**
```
[DailyQuestManager] Quest activated: [quest_id] ([quest_title])
[TestQuest] Activated at [time]
```

**Success Criteria:**
- ✅ Quest activates without errors
- ✅ OnActivated() callback executes
- ✅ Quest appears in active quests list

---

### Test 7: Quest Completion (Scanning Mode)

**Objective:** Test scanning mode quest completion

**Test Quest:** "Test Quest" (10 second timer)

**Steps:**
1. Activate the test quest
2. Wait 10 seconds
3. Observe quest completion

**Expected Console Output:**
```
[TestQuest] Activated at [time]
[DailyQuestManager] Quest completed: test_simple_quest (Test Quest)
[TestQuest] Completed! Elapsed time: 10.0s
[TestQuest] In a real quest, you would give rewards here
```

**Success Criteria:**
- ✅ Quest completes automatically after 10 seconds
- ✅ OnCompleted() callback executes
- ✅ Quest moves to completed list
- ✅ No errors during completion

---

### Test 8: Quest Completion (Event-Driven Mode)

**Objective:** Test event-driven quest completion

**Test Quest:** "Test Event Quest" (spacebar presses)

**Steps:**
1. Activate the test event quest
2. Press spacebar 3 times
3. Observe quest completion

**Expected Console Output:**
```
[TestEventQuest] Activated - press spacebar 3 times
[TestEventQuest] Space pressed! Count: 1/3
[TestEventQuest] Space pressed! Count: 2/3
[TestEventQuest] Space pressed! Count: 3/3
[DailyQuestManager] Quest completed: test_event_quest (Test Event Quest)
[TestEventQuest] Completed! Total presses: 3
```

**Success Criteria:**
- ✅ Quest tracks spacebar presses
- ✅ Quest completes after 3 presses
- ✅ OnCompleted() callback executes
- ✅ Event cleanup happens properly

---

### Test 9: Real Quest - Kill Boss

**Objective:** Test KillBossQuest functionality

**Prerequisites:** KillBossQuest must be registered in a quest pool

**Steps:**
1. Activate "Boss Hunter" quest
2. Find and kill a boss enemy
3. Verify quest completion

**Expected Console Output:**
```
[KillBossQuest] Activated - kill 1 boss(es)
[KillBossQuest] Progress: Boss killed! Progress: 1/1
[DailyQuestManager] Quest completed: daily_kill_boss
[KillBossQuest] Boss Hunter Complete! Defeated 1 boss(es)
```

**Success Criteria:**
- ✅ Quest tracks boss kills only (not regular enemies)
- ✅ Only counts player kills
- ✅ Quest completes when requirement met
- ✅ Rewards given (check money/exp)

---

### Test 10: Real Quest - Kill Mobs

**Objective:** Test KillMobsQuest functionality

**Prerequisites:** KillMobsQuest must be registered

**Steps:**
1. Activate "Mob Slayer" quest
2. Kill the specified number of enemies
3. Verify quest completion

**Expected Console Output:**
```
[KillMobsQuest] Activated - kill 5 [mob_name]
[KillMobsQuest] Progress: [mob_name] killed! Progress: 1/5
[KillMobsQuest] Progress: [mob_name] killed! Progress: 2/5
...
[DailyQuestManager] Quest completed: daily_kill_mobs_[name]
[KillMobsQuest] Mob Slayer Complete! Eliminated 5 [mob_name]
```

**Success Criteria:**
- ✅ Quest tracks specific mob kills (if preset specified)
- ✅ Excludes bosses and friendly NPCs
- ✅ Only counts player kills
- ✅ Quest completes correctly

---

### Test 11: Real Quest - Submit Items

**Objective:** Test SubmitItemsQuest functionality

**Prerequisites:** SubmitItemsQuest must be registered, player has required items

**Steps:**
1. Activate "Item Collector" quest
2. Collect the required items
3. Quest should auto-complete when items are possessed (scanning mode)

**Expected Console Output:**
```
[SubmitItemsQuest] Activated - collect and submit 10 [item_name]
[SubmitItemsQuest] Successfully consumed 10 [item_name]
[DailyQuestManager] Quest completed: daily_submit_items_[id]
[SubmitItemsQuest] Item Collector Complete! Submitted 10 [item_name]
```

**Success Criteria:**
- ✅ Quest description shows current item count
- ✅ Quest completes when items are possessed
- ✅ Items are consumed from inventory
- ✅ Rewards given correctly

---

### Test 12: Day Crossing

**Objective:** Test daily refresh at midnight UTC

**Steps:**
1. Check current time vs midnight UTC
2. If close to midnight, wait for day crossing
3. OR use `DailyQuestManager.ForceRefresh()` via console/script

**Expected Console Output:**
```
[DailyQuestManager] Day crossed, refreshing quests
[DailyQuestManager] Refreshing daily quests for [new_date]
[DailyQuestManager] Quest expired: [quest_id]
[TestQuestPool] Selecting quests for [new_date]
[DailyQuestManager] Quest activated: [new_quest_id] ([new_quest_title])
[DailyQuestManager] Refresh complete, X quests active
```

**Success Criteria:**
- ✅ Old incomplete quests expire (OnExpired called)
- ✅ Completed quests are cleared
- ✅ New quests are selected from pools
- ✅ New quests are activated
- ✅ Save state updated

---

### Test 13: Quest Expiration

**Objective:** Test quest expiration without completion

**Steps:**
1. Activate a quest
2. Do NOT complete it
3. Wait for day crossing or force refresh

**Expected Console Output:**
```
[DailyQuestManager] Quest expired: [quest_id]
[TestQuest] Expired without completion
```

**Success Criteria:**
- ✅ OnExpired() callback called
- ✅ Quest removed from active list
- ✅ Event subscriptions cleaned up (no memory leaks)

---

### Test 14: Save/Load

**Objective:** Test quest state persistence

**Steps:**
1. Activate quests and make partial progress
2. Save the game
3. Quit to main menu or close game
4. Load the save
5. Verify quest state restored

**Expected Behavior:**
- ✅ Last refresh date restored
- ✅ Quest progress restored (if same day)
- ✅ OR quests refreshed (if different day)

**Current Limitation:**
- Quest instances not fully restored from save (quests refresh on load)
- Save/load uses PlayerPrefs (temporary, needs SavesSystem integration)

---

### Test 15: Multiple Quest Pools

**Objective:** Test multiple mods registering quest pools

**Prerequisites:** Multiple quest pools registered

**Steps:**
1. Register multiple quest pools from different sources
2. Trigger refresh
3. Verify quests from all pools can be selected

**Expected Console Output:**
```
[DailyQuestManager] Pool registered: TestQuestPool
[DailyQuestManager] Pool registered: AnotherQuestPool
...
[TestQuestPool] Selecting quests for [date]
[AnotherQuestPool] Selecting quests for [date]
[DailyQuestManager] Refresh complete, X quests active
```

**Success Criteria:**
- ✅ All pools can register successfully
- ✅ All pools are queried during refresh
- ✅ Quests from multiple pools can coexist
- ✅ No duplicate quest ID conflicts

---

### Test 16: Max Quest Limit

**Objective:** Test configurable max daily quests limit

**Steps:**
1. Set `DailyQuestManager.MaxDailyQuests = 3`
2. Register pools that provide more than 3 quests
3. Trigger refresh
4. Verify only 3 quests are active

**Expected Console Output:**
```
[DailyQuestManager] Limiting quests from 5 to 3
[DailyQuestManager] Refresh complete, 3 quests active
```

**Success Criteria:**
- ✅ Quest count limited correctly
- ✅ Random sampling when limit exceeded
- ✅ Unlimited works when set to -1

---

### Test 17: Error Handling

**Objective:** Test framework robustness with errors

**Steps:**
1. Create a quest with intentional error in IsCompleted()
2. Activate the quest
3. Verify error is caught and logged without crashing

**Expected Behavior:**
- ✅ Error logged to console
- ✅ Game continues running
- ✅ Other quests not affected
- ✅ Framework remains stable

---

### Test 18: UI Interaction

**Objective:** Test full UI workflow

**Steps:**
1. Open daily quest UI
2. Switch between "Active" and "Completed" tabs
3. Select different quests
4. View quest details
5. Close UI

**Success Criteria:**
- ✅ Tabs switch correctly
- ✅ Quest list updates
- ✅ Quest details display correctly
- ✅ UI closes without errors
- ✅ No visual glitches

---

## Performance Testing

### Memory Leak Test

**Steps:**
1. Activate and complete 50+ quests over multiple days
2. Monitor memory usage
3. Check for increasing memory consumption

**Success Criteria:**
- ✅ Memory usage stable
- ✅ No unbounded growth
- ✅ Event subscriptions cleaned up

### Quest Scanning Performance

**Steps:**
1. Activate 20+ scanning mode quests simultaneously
2. Monitor frame rate
3. Check update loop performance

**Success Criteria:**
- ✅ No significant FPS drop
- ✅ Scan intervals respected
- ✅ No lag spikes

---

## Troubleshooting

### UI Doesn't Open

**Possible Causes:**
- DailyQuestGiverView.Instance is null
- View prefab not loaded
- Unity UI system not initialized

**Solutions:**
- Check console for initialization errors
- Verify View.Awake() is called
- Ensure singleton is set up correctly

### Trophy Integration Fails

**Possible Causes:**
- Trophy not found in scene
- Building system not initialized
- Trophy doesn't have InteractableBase

**Solutions:**
- Build the trophy in base
- Wait for building system to initialize
- Check console for search attempts
- Verify trophy GameObject structure

### Quests Don't Complete

**Possible Causes:**
- IsCompleted() returns false
- Event not subscribed correctly
- CompleteQuest() not called

**Solutions:**
- Add debug logging to IsCompleted()
- Verify event subscriptions in OnActivated()
- For event-driven: ensure CompleteQuest() is called
- Check console for errors

### Day Crossing Doesn't Work

**Possible Causes:**
- Time check logic error
- DayCrossingCheckInterval too long
- System time issues

**Solutions:**
- Use ForceRefresh() for testing
- Check LastRefreshTime vs current time
- Reduce DayCrossingCheckInterval for testing
- Verify DateTime.UtcNow is correct

---

## Test Results Template

```
Test Date: [Date]
Game Version: [Version]
Mod Version: [Version]

| Test # | Test Name                  | Status | Notes |
|--------|---------------------------|--------|-------|
| 1      | Mod Initialization        | ⬜     |       |
| 2      | Trophy Integration        | ⬜     |       |
| 3      | UI Access via Keybind     | ⬜     |       |
| 4      | Trophy Interaction        | ⬜     |       |
| 5      | Quest Registration        | ⬜     |       |
| 6      | Quest Activation          | ⬜     |       |
| 7      | Scanning Quest Completion | ⬜     |       |
| 8      | Event Quest Completion    | ⬜     |       |
| 9      | Kill Boss Quest           | ⬜     |       |
| 10     | Kill Mobs Quest           | ⬜     |       |
| 11     | Submit Items Quest        | ⬜     |       |
| 12     | Day Crossing              | ⬜     |       |
| 13     | Quest Expiration          | ⬜     |       |
| 14     | Save/Load                 | ⬜     |       |
| 15     | Multiple Quest Pools      | ⬜     |       |
| 16     | Max Quest Limit           | ⬜     |       |
| 17     | Error Handling            | ⬜     |       |
| 18     | UI Interaction            | ⬜     |       |

Status: ✅ Pass | ❌ Fail | ⬜ Not Tested | ⚠️ Partial
```

---

## Post-Testing

After completing tests, update IMPLEMENTATION_STATUS.md with results and any issues found.

Report bugs with:
- Test number and name
- Steps to reproduce
- Expected vs actual behavior
- Console output
- Screenshots (if applicable)

---

**Happy Testing!** 🎮