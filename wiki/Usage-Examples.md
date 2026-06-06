Here are common examples illustrating how to interact with PSXMCL across **F#**, **C#**, and **VB.NET**.

---

### Example 1: Loading a Memory Card & Outputting Save Info

This example shows how to read a memory card file, parse the first file found within it, and print basic save slot metadata.

#### F#
```fsharp
open PSXMCL

let mc = MemoryCard.read "save.mcd"
let fft = FinalFantasyTactics.load mc.Files.[0]

printfn "Main Character: %s" (FinalFantasyTacticsStrings.getMainCharacterName fft.MainCharName)
printfn "Active Job: %s (Lv. %d)" (FinalFantasyTacticsStrings.getJobName fft.MainCharJob) fft.MainCharLevel
printfn "Save Date: %s" (FinalFantasyTacticsStrings.getSaveDateText fft.SaveDate)
```

#### C#
```csharp
using PSXMCL;
using System;

var mc = MemoryCard.Read("save.mcd");
var fft = FinalFantasyTactics.Load(mc.Files[0]);

Console.WriteLine($"Main Character: {FinalFantasyTacticsStrings.GetMainCharacterName(fft.MainCharName)}");
Console.WriteLine($"Active Job: {FinalFantasyTacticsStrings.GetJobName(fft.MainCharJob)} (Lv. {fft.MainCharLevel})");
Console.WriteLine($"Save Date: {FinalFantasyTacticsStrings.GetSaveDateText(fft.SaveDate)}");
```

#### VB.NET
```vb
Imports PSXMCL

Module Program
    Sub Main()
        Dim mc = MemoryCard.Read("save.mcd")
        Dim fft = FinalFantasyTactics.Load(mc.Files(0))

        Console.WriteLine("Main Character: " & FinalFantasyTacticsStrings.GetMainCharacterName(fft.MainCharName))
        Console.WriteLine("Active Job: " & FinalFantasyTacticsStrings.GetJobName(fft.MainCharJob) & " (Lv. " & fft.MainCharLevel & ")")
        Console.WriteLine("Save Date: " & FinalFantasyTacticsStrings.GetSaveDateText(fft.SaveDate))
    End Sub
End Module
```

---

### Example 2: Modifying and Saving Main Character Properties

This example demonstrates how to load a save file, modify main character stats (name, level, job), repackage it, and write the updated memory card back to disk.

#### F#
```fsharp
open PSXMCL

let mc = MemoryCard.read "save.mcd"
let fft = FinalFantasyTactics.load mc.Files.[0]

// Copy and update record values
let updatedFft = 
    { fft with 
        MainCharName = FinalFantasyTacticsStrings.encodeMainCharacterName "Ramza"
        MainCharLevel = 99uy
        MainCharJob = 0x05uy } // Knight

let updatedSave = FinalFantasyTactics.toSaveFile updatedFft mc.Files.[0]
let updatedMc = MemoryCard.withFileData updatedSave mc

MemoryCard.write "save_modified.mcd" updatedMc
```

#### C#
```csharp
using PSXMCL;

var mc = MemoryCard.Read("save.mcd");
var fft = FinalFantasyTactics.Load(mc.Files[0]);

// Record-like updates are performed via standard instantiation
var updatedFft = fft with {
    MainCharName = FinalFantasyTacticsStrings.EncodeMainCharacterName("Ramza"),
    MainCharLevel = 99,
    MainCharJob = 0x05
};

var updatedSave = FinalFantasyTactics.ToSaveFile(updatedFft, mc.Files[0]);
var updatedMc = MemoryCard.WithFileData(updatedSave, mc);

MemoryCard.Write("save_modified.mcd", updatedMc);
```

#### VB.NET
```vb
Imports PSXMCL

Module Program
    Sub Main()
        Dim mc = MemoryCard.Read("save.mcd")
        Dim fft = FinalFantasyTactics.Load(mc.Files(0))

        Dim updatedFft = New FinalFantasyTactics.FftSaveData With {
            .MainCharName = FinalFantasyTacticsStrings.EncodeMainCharacterName("Ramza"),
            .MainCharLevel = 99,
            .MainCharJob = &H5,
            .SaveDate = fft.SaveDate,
            .MapPosition = fft.MapPosition,
            .GameTime = fft.GameTime,
            .Units = fft.Units,
            .PlayerInventory = fft.PlayerInventory,
            .FurShopInventory = fft.FurShopInventory,
            .PlayerOptions = fft.PlayerOptions,
            .RawData = fft.RawData
        }

        Dim updatedSave = FinalFantasyTactics.ToSaveFile(updatedFft, mc.Files(0))
        Dim updatedMc = MemoryCard.WithFileData(updatedSave, mc)

        MemoryCard.Write("save_modified.mcd", updatedMc)
    End Sub
End Module
```

---

### Example 3: Modifying Unit Equipment and Abilities Safely

This example uses the type-safe helpers under `FinalFantasyTacticsEnums` to modify a unit's active inventory and capabilities.

#### F#
```fsharp
open PSXMCL

let mc = MemoryCard.read "save.mcd"
let fft = FinalFantasyTactics.load mc.Files.[0]

let firstUnit = fft.Units.[0]

// Equip headgear and set reaction ability
let editedUnit = 
    firstUnit
    |> FinalFantasyTacticsEnums.setEquipment FinalFantasyTacticsEnums.EquipmentSlot.Head 0x90uy // Ribbon
    |> FinalFantasyTacticsEnums.setAbility FinalFantasyTacticsEnums.AbilitySlot.Reaction 0x003Aus // Auto-Potion

let updatedUnits = Array.copy fft.Units
updatedUnits.[0] <- editedUnit

let updatedFft = { fft with Units = updatedUnits }
```

#### C#
```csharp
using PSXMCL;

var mc = MemoryCard.Read("save.mcd");
var fft = FinalFantasyTactics.Load(mc.Files[0]);

var firstUnit = fft.Units[0];

// Edit head slot and reaction slot
var editedUnit = FinalFantasyTacticsEnums.SetEquipment(
    FinalFantasyTacticsEnums.EquipmentSlot.Head, 
    0x90, 
    firstUnit
);
editedUnit = FinalFantasyTacticsEnums.SetAbility(
    FinalFantasyTacticsEnums.AbilitySlot.Reaction, 
    0x003A, 
    editedUnit
);

var updatedUnits = (FinalFantasyTactics.UnitStats[])fft.Units.Clone();
updatedUnits[0] = editedUnit;

var updatedFft = fft with { Units = updatedUnits };
```

#### VB.NET
```vb
Imports PSXMCL

Module Program
    Sub Main()
        Dim mc = MemoryCard.Read("save.mcd")
        Dim fft = FinalFantasyTactics.Load(mc.Files(0))

        Dim firstUnit = fft.Units(0)

        Dim editedUnit = FinalFantasyTacticsEnums.SetEquipment(
            FinalFantasyTacticsEnums.EquipmentSlot.Head, 
            &H90, 
            firstUnit
        )
        editedUnit = FinalFantasyTacticsEnums.SetAbility(
            FinalFantasyTacticsEnums.AbilitySlot.Reaction, 
            &H3A, 
            editedUnit
        )

        Dim updatedUnits = DirectCast(fft.Units.Clone(), FinalFantasyTactics.UnitStats())
        updatedUnits(0) = editedUnit

        ' Copy attributes over to a new record
        Dim updatedFft = New FinalFantasyTactics.FftSaveData With {
            .Units = updatedUnits,
            .MainCharName = fft.MainCharName,
            .MainCharLevel = fft.MainCharLevel,
            .MainCharJob = fft.MainCharJob,
            .SaveDate = fft.SaveDate,
            .MapPosition = fft.MapPosition,
            .GameTime = fft.GameTime,
            .PlayerInventory = fft.PlayerInventory,
            .FurShopInventory = fft.FurShopInventory,
            .PlayerOptions = fft.PlayerOptions,
            .RawData = fft.RawData
        }
    End Sub
End Module
```
