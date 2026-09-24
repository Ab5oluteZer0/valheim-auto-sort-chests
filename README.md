# Valheim Auto Sort Chests

BepInEx mod for [Valheim](https://www.valheimgame.com/) that adds a **Sort**
button to every container (chest) window, next to the built-in "Take All" /
"Stack All" buttons.

Clicking it:
1. Merges identical item stacks into as few stacks as possible (same rule
   the game itself uses to decide if two items can stack).
2. Lays out the remaining items in a consistent order: by item type, then
   alphabetically by name.

## Requirements

- Valheim (tested on 1.0.15)
- [BepInEx](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/) 5.4.x

## Installation (players)

1. Install BepInEx for Valheim if you haven't already (see link above, or
   use [r2modman](https://valheim.thunderstore.io/package/ebkr/r2modman/)).
2. Download `Mod3-AutoSortChests.dll` from the
   [latest release](../../releases/latest).
3. Drop it into `<Valheim install folder>\BepInEx\plugins\Mod3-AutoSortChests\`.
4. Launch the game and open any chest - you'll see the new "Sort" button.

## Building from source

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download) (or newer)
and a local Valheim install with BepInEx installed.

```bash
git clone https://github.com/Ab5oluteZer0/valheim-auto-sort-chests.git
cd valheim-auto-sort-chests
dotnet build -c Release -p:ValheimPath="C:\Path\To\Valheim"
```

If you don't pass `-p:ValheimPath`, the build looks for a `VALHEIM_PATH`
environment variable, then falls back to the default Steam location
(`C:\Program Files (x86)\Steam\steamapps\common\Valheim`).

The build automatically copies the built DLL into
`<Valheim>\BepInEx\plugins\Mod3-AutoSortChests\` for quick in-game testing.

## How it works

A Harmony postfix on `InventoryGui.Awake` clones the existing "Stack All"
button to create the new "Sort" button (same style, correctly anchored),
positioned between "Take All" and "Stack All". Sorting operates directly on
`Inventory.GetAllItems()` (the live internal list) and reassigns each item's
grid position - no reflection into private container internals needed beyond
one field lookup for the currently-open container.

## License

MIT - see [LICENSE](LICENSE).
