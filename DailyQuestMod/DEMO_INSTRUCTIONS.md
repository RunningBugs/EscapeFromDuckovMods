# Daily Quest System - Demo Testing Instructions

**Version:** 1.0 - Full Demo Ready  
**Last Updated:** 2024

---

## 🎮 Quick Start

### Installation

1. Copy `DailyQuestMod.dll` from `bin/Release/` to:
   ```
   <Game Directory>/Duckov_Data/Mods/DailyQuestMod/DailyQuestMod.dll
   ```

2. Copy `info.ini` to the same folder

3. Launch the game

---

## ✅ What's Included in This Demo

### Test Quests (Always Available)

1. **Test Quest** - Scanning Mode
   - Automatically completes after 10 seconds
   - Tests automatic completion detection
   - Shows progress in description

2. **Test Event Quest** - Event-Driven Mode
   - Complete by pressing **Spacebar 3 times**
   - Tests manual completion triggering
   - Shows press counter in description

### Features Demonstrated

- ✅ Quest activation and tracking
- ✅ Scanning mode (automatic checking)
- ✅ Event-driven mode (manual triggering)
- ✅ Quest completion with rewards
- ✅ Real-time UI updates
- ✅ Quest expiration at midnight UTC
- ✅ Save/load persistence
- ✅ Trophy integration (Commemorative Trophy)

---

## 🎯 How to Test

### Method 1: Quick Test with K Key (RECOMMENDED)

1. **Load or start a game**
2. **Press 'K' key** anywhere in-game
3. **Daily Quest UI will appear** with a window showing:
   - Two test quests
   - Active/Completed tabs
   - Time until refresh
   - Quest descriptions and progress

4. **Test the quests:**
   - **Test Quest**: Just wait 10 seconds, it will auto-complete
   - **Test Event Quest**: Press Spacebar 3 times to complete

5. **Press 'K' again** to close the UI

### Method 2: Trophy Interaction

1. **Build or locate** the Commemorative Trophy in your base
2. **Approach the trophy** and press 'F' to interact
3. **Look for** "Daily Quests" option in the interaction menu
   - ⚠️ Note: May not appear if MultiInteraction isn't set up - use Method 1 instead

---

## 📝 Expected Behavior

### On Game Launch

Console should show:
```
[DailyQuestMod] Initializing Daily Quest Framework...
[DailyQuestGiverView] Instance created successfully
[DailyQuestMod] Registering test quest pool for demo
[TestQuestPool] Initializing...
[TestQuestPool] Initialized with 2 quests
[DailyQuestManager] Pool registered: TestQuestPool
[DailyQuestMod] Daily Quest Framework initialized successfully
```

### When Entering Base

Console should show:
```
[TrophyIntegration] Found trophy: Building_DemoTrophy(Clone)
[DailyQuestGiver] Awake completed successfully
[TrophyIntegration] Successfully integrated with Commemorative Trophy!
```

### When Pressing K Key

- UI window appears in center of screen
- Shows "Daily Quests" title
- Two tabs: "Active Quests" and "Completed Quests"
- Quest list with descriptions
- Timer showing time until refresh (next midnight UTC)
- Close button at bottom

### Testing Quest Completion

#### Test Quest (10 Second Timer)
1. Open UI (Press K)
2. See "Test Quest" - description shows remaining time
3. Wait 10 seconds
4. Quest auto-completes
5. Check console for:
   ```
   [TestQuest] Completed! Elapsed time: 10.0s
   [DailyQuestManager] Quest completed: test_simple_quest
   ```
6. Switch to "Completed Quests" tab to see it listed

#### Test Event Quest (Spacebar)
1. Open UI (Press K)
2. See "Test Event Quest" - description shows (0/3)
3. Press Spacebar 3 times
4. Description updates: (1/3), (2/3), (3/3)
5. Quest completes on 3rd press
6. Check console for:
   ```
   [TestEventQuest] Space pressed! Count: 1/3
   [TestEventQuest] Space pressed! Count: 2/3
   [TestEventQuest] Space pressed! Count: 3/3
   [DailyQuestManager] Quest completed: test_event_quest
   ```

---

## 🐛 Troubleshooting

### UI Doesn't Show When Pressing K

**Check console for errors:**
- Look for `[DailyQuestGiverView] Instance created successfully`
- If missing, UI failed to initialize

**Solution:**
- Check that `DailyQuestMod.dll` is in correct folder
- Restart the game
- Check for mod conflicts in console

### No Quests Showing in UI

**Check console for:**
```
[TestQuestPool] Selected 2 quest(s) for today
[DailyQuestManager] Quest activated: test_simple_quest
[DailyQuestManager] Quest activated: test_event_quest
```

**If missing:**
- Quest pool didn't register
- Check for errors during mod initialization

**Solution:**
- Restart the game
- Check console for `[DailyQuestManager] Pool registered: TestQuestPool`

### Trophy Integration Failed

**Check console for:**
```
[TrophyIntegration] Commemorative Trophy not found, will retry later
```

**This is normal if:**
- Trophy not built yet
- Not in base scene

**Solution:**
- Use Method 1 (K key) instead
- Build Commemorative Trophy in your base

### Quest Not Completing

**Test Quest (10 seconds):**
- Make sure you wait full 10 seconds
- Quest checks every 0.5 seconds
- Progress shows in description

**Test Event Quest (Spacebar):**
- Make sure UI is open or closed (doesn't matter)
- Press Spacebar 3 times
- Each press should log to console
- Check if Input.GetKeyDown is working

---

## 🔧 Advanced Testing

### Test Daily Refresh

1. Open console
2. Check current time vs midnight UTC
3. Wait for midnight UTC or use force refresh (if available)
4. Quests should expire and regenerate

### Test Save/Load

1. Activate quests and make partial progress
2. Save game
3. Quit to menu
4. Load save
5. Quests should restore state (or refresh if different day)

### Test Multiple Quest Pools

Add your own quest pool in code:
```csharp
public class MyQuestPool : IDailyQuestPool
{
    public string PoolId => "MyQuests";
    // ... implement
}

// Register it:
DailyQuestManager.RegisterQuestPool(new MyQuestPool());
```

---

## 📊 Demo Statistics

**Framework Status:** ✅ Fully Operational
- Core framework: ✅ Working
- Trophy integration: ✅ Working
- UI system: ✅ Working (IMGUI)
- Test quests: ✅ Both modes working
- Save/load: ✅ Basic functionality
- Daily refresh: ✅ UTC midnight tracking

**Known Limitations:**
- UI is basic IMGUI (not Unity UI canvas)
- Trophy MultiInteraction may not show menu (use K key)
- No sound effects
- No fancy animations
- Test quests only (no real gameplay quests yet)

---

## 🎯 Next Steps for Full Production

1. **Create Real Quests:**
   - Boss killing quests (using KillBossQuest)
   - Mob killing quests (using KillMobsQuest)
   - Item submission quests (using SubmitItemsQuest)

2. **Create Production Quest Pool:**
   - Define actual quests in quest pool
   - Set proper rewards (money, exp, items)
   - Configure selection logic

3. **Improve UI:**
   - Use Unity UI canvas instead of IMGUI
   - Add better styling
   - Add quest icons
   - Add reward display

4. **Add Features:**
   - Quest notifications
   - Completion sound effects
   - Quest history view
   - Daily streak bonuses

---

## 💡 Tips

- **Press K anytime** to quickly check your daily quests
- Watch the console for detailed logging
- Test quests are designed to be quick (10 seconds / 3 presses)
- Quests refresh at midnight UTC every day
- You can complete quests in any order
- Uncompleted quests expire at day end

---

## 🎮 Demo Success Checklist

- [ ] Mod loads without errors
- [ ] UI opens with K key
- [ ] Two test quests visible
- [ ] Test Quest completes after 10 seconds
- [ ] Test Event Quest completes after 3 spacebar presses
- [ ] Completed quests show in "Completed Quests" tab
- [ ] Trophy integration successful (optional)
- [ ] Console shows all expected messages

---

**🎉 If all checks pass, the Daily Quest Framework is working perfectly!**

**Ready for production quest implementation.**

For questions or issues, check the console log for `[DailyQuestMod]` messages.