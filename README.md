# PSXMCL

PSXMCL is a small .NET library for reading, editing, and writing PlayStation memory card data, with a focused Final Fantasy Tactics save-data layer on top.

The core pieces are:

- `MemoryCard` for reading and writing PSX Memory Card files. We tested it using `.mcd` files, but in theory it should also work with other formats.
- `FinalFantasyTactics` for parsing FFT save data into typed records.
- `FinalFantasyTacticsEnums` for enum-oriented edit helpers (palette, gender flags, equipment/ability slots, and typed IDs).
- `FinalFantasyTacticsStrings` for converting parsed FFT values into displayable text.

XML documentation is enabled, so consumers should see IntelliSense documentation in IDEs when the package is referenced.

## Quick Start

### F#

```fsharp
open PSXMCL

let mc = MemoryCard.read "save.mcd"
let fft = FinalFantasyTactics.load mc.Files.[0]

let mainName = FinalFantasyTacticsStrings.getMainCharacterName fft.MainCharName
let firstItemName = FinalFantasyTacticsStrings.getItemName fft.PlayerInventory.[1]
let firstJobName = FinalFantasyTacticsStrings.getJobName fft.MainCharJob
```

### Visual Basic .NET

```vb
Imports PSXMCL

Dim mc = MemoryCard.Read("save.mcd")
Dim fft = FinalFantasyTactics.Load(mc.Files(0))

Dim mainName = FinalFantasyTacticsStrings.GetMainCharacterName(fft.MainCharName)
Dim firstItemName = FinalFantasyTacticsStrings.GetItemName(fft.PlayerInventory(1))
Dim firstJobName = FinalFantasyTacticsStrings.GetJobName(fft.MainCharJob)
```

### C#

```csharp
using PSXMCL;

var mc = MemoryCard.Read("save.mcd");
var fft = FinalFantasyTactics.Load(mc.Files[0]);

var mainName = FinalFantasyTacticsStrings.GetMainCharacterName(fft.MainCharName);
var firstItemName = FinalFantasyTacticsStrings.GetItemName(fft.PlayerInventory[1]);
var firstJobName = FinalFantasyTacticsStrings.GetJobName(fft.MainCharJob);
```

## Editing Flow

1. Read a memory card with `MemoryCard.read`.
2. Parse the FFT save with `FinalFantasyTactics.load`.
3. Modify the parsed `FftSaveData` record.
4. Serialize it back with `FinalFantasyTactics.toSaveFile`.
5. Write the updated card with `MemoryCard.write`.

The following code snippet demonstrates a simple edit flow where we change the main character's name and job, then write it back to a new file.

### F#

```fsharp
open PSXMCL

let mc = MemoryCard.read "save.mcd"
let fft = FinalFantasyTactics.load mc.Files.[0]

// Modify the save data
let updatedFft =
    { fft with
        MainCharName = FinalFantasyTacticsStrings.encodeMainCharacterName "John"
        MainCharJob = 0x05uy }

let updatedSaveFile = FinalFantasyTactics.toSaveFile updatedFft mc.Files.[0]
let updatedMemoryCard = MemoryCard.withFileData updatedSaveFile mc
MemoryCard.write "save_modified.mcd" updatedMemoryCard
```

### Visual Basic .NET

```vb
Imports PSXMCL

Dim mc = MemoryCard.Read("save.mcd")
Dim fft = FinalFantasyTactics.Load(mc.Files(0))

' Modify the save data
Dim updatedFft = New FinalFantasyTactics.FftSaveData With {
    .MainCharName = FinalFantasyTacticsStrings.EncodeMainCharacterName("John"),
    .MainCharJob = 0x05S
}

Dim updatedSaveFile = FinalFantasyTactics.ToSaveFile(updatedFft, mc.Files(0))
Dim updatedMemoryCard = MemoryCard.WithFileData(updatedSaveFile, mc)
MemoryCard.Write("save_modified.mcd", updatedMemoryCard)
```

### C#

```csharp
using PSXMCL;

var mc = MemoryCard.Read("save.mcd");
var fft = FinalFantasyTactics.Load(mc.Files[0]);

// Modify the save data
var updatedFft = new FinalFantasyTactics.FftSaveData
{
    MainCharName = FinalFantasyTacticsStrings.EncodeMainCharacterName("John"),
    MainCharJob = 0x05
};

var updatedSaveFile = FinalFantasyTactics.ToSaveFile(updatedFft, mc.Files[0]);
var updatedMemoryCard = MemoryCard.WithFileData(updatedSaveFile, mc);
MemoryCard.Write("save_modified.mcd", updatedMemoryCard);
```

## Documentation

- See [USAGE.md](USAGE.md) for practical scenarios and snippets.
- See [FinalFantasyTactics.fs](FinalFantasyTactics.fs) for the save-data model.
- See [FinalFantasyTactics.Enums.fs](FinalFantasyTactics.Enums.fs) for enum-style edit helpers.
- See [FinalFantasyTactics.Strings.fs](FinalFantasyTactics.Strings.fs) for display helpers.
- See [MemoryCard.fs](MemoryCard.fs) for raw memory-card I/O.
