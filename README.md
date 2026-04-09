# OPERATOR — Unlimited Spare Magazines
### Ammo still depletes · Full reload animations · Unlimited pouch

---

## What this mod does

- ✅ Bullets still deplete from your magazine (realistic feel)
- ✅ Full reload animations, sounds, and hand IK play normally
- ✅ You never run out of spare magazines in your pouch
- ✅ The HUD pouch always shows magazines as available
- ✅ The "no mags" voice line / animation is suppressed

## What it patches (from your game's own metadata)

| Patch | Method | Effect |
|---|---|---|
| Postfix | `PlayerNetworking.UserCode_CMD_ClientReloadMag` | Restores `magcount` to 99 after every reload |
| Prefix | `PlayerNetworking.UserCode_CMD_OnReloadNoMags` | Blocks the "out of mags" CMD |
| Prefix | `PlayerNetworking.UserCode_RPC_OnReloadNoMags` | Blocks the server sync for "out of mags" |
| Postfix | `MagManagerUI.RefreshMags` | Calls `RefillAllMags()` to keep HUD icons visible |

---

## Build & Install

### Step 1 — Install BepInEx IL2CPP

1. Download **BepInEx_Unity_IL2CPP_x64** from:
   https://github.com/BepInEx/BepInEx/releases

2. Extract into your OPERATOR game folder:
   ```
   C:\Program Files (x86)\Steam\steamapps\common\OPERATOR\
   ```

3. Launch OPERATOR once and close it.
   BepInEx will generate `BepInEx\interop\` with game stubs (takes ~1 min).

---

### Step 2 — Collect DLLs

Create a `libs\` folder next to `OperatorUnlimitedSpare.csproj` and copy:

**From `OPERATOR\BepInEx\core\`:**
```
0Harmony.dll
BepInEx.Core.dll
BepInEx.Unity.IL2CPP.dll
BepInEx.Unity.Common.dll
Il2CppInterop.Runtime.dll
```

**From `OPERATOR\BepInEx\interop\`:**
```
Assembly-CSharp.dll   ← the IL2CPP interop stub (NOT GameAssembly.dll)
```

---

### Step 3 — Build

Requires .NET 6 SDK: https://dotnet.microsoft.com/download/dotnet/6.0

```
dotnet build -c Release
```

Output: `bin\Release\net6.0\OperatorUnlimitedSpare.dll`

---

### Step 4 — Install

Copy `OperatorUnlimitedSpare.dll` to:
```
OPERATOR\BepInEx\plugins\
```

---

### Step 5 — Verify

Launch OPERATOR and check:
```
OPERATOR\BepInEx\LogOutput.log
```

Look for:
```
[Info  : UnlimitedSpareMags] [UnlimitedSpare] Plugin loaded!
[Info  : UnlimitedSpareMags] [UnlimitedSpare] All patches applied!
```

---

## Troubleshooting

**Mags still run out:**
The game may have updated and renamed `magcount` or the CMD method.
Check `LogOutput.log` for Harmony patch errors and report the new method name.

**Game crashes on launch:**
Make sure you installed the **IL2CPP** build of BepInEx (not Mono).

**HUD shows 0 mags but reloading still works:**
This is a display glitch — the `RefreshMags` postfix should fix it.
If not, try also patching `MagManagerUI.RefillAllMags` directly.

---

## ⚠️ Multiplayer Warning

Only use in **singleplayer / offline** mode.
The server validates magazine counts via Mirror networking — in co-op,
other players may see desynced mag states, and repeated use in PvP could trigger a ban.
