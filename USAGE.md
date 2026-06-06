# Usage

This document shows a few common flows for working with PSXMCL.

## 1. Read an FFT save from a memory card

### F#

```fsharp
open PSXMCL

let memoryCard = MemoryCard.read "Final Fantasy Tactics_1.mcd"
let fft = FinalFantasyTactics.load memoryCard.Files.[0]

printfn "Main character: %s" (FinalFantasyTacticsStrings.getMainCharacterName fft.MainCharName)
printfn "Job: %s" (FinalFantasyTacticsStrings.getJobName fft.MainCharJob)
printfn "Save date: %s" (FinalFantasyTacticsStrings.getSaveDateText fft.SaveDate)
```

### Visual Basic .NET

```vb
Imports PSXMCL

Dim memoryCard = MemoryCard.Read("Final Fantasy Tactics_1.mcd")
Dim fft = FinalFantasyTactics.Load(memoryCard.Files(0))

Console.WriteLine("Main character: " & FinalFantasyTacticsStrings.GetMainCharacterName(fft.MainCharName))
Console.WriteLine("Job: " & FinalFantasyTacticsStrings.GetJobName(fft.MainCharJob))
Console.WriteLine("Save date: " & FinalFantasyTacticsStrings.GetSaveDateText(fft.SaveDate))
```

### C#

```csharp
using PSXMCL;

var memoryCard = MemoryCard.Read("Final Fantasy Tactics_1.mcd");
var fft = FinalFantasyTactics.Load(memoryCard.Files[0]);

Console.WriteLine("Main character: " + FinalFantasyTacticsStrings.GetMainCharacterName(fft.MainCharName));
Console.WriteLine("Job: " + FinalFantasyTacticsStrings.GetJobName(fft.MainCharJob));
Console.WriteLine("Save date: " + FinalFantasyTacticsStrings.GetSaveDateText(fft.SaveDate));
```

## 2. Show inventory and fur-shop items

The inventory and fur-shop arrays are byte-indexed tables. Each byte position is the item ID and each value is the quantity.

### F#

```fsharp
open PSXMCL

let memoryCard = MemoryCard.read "save.mcd"
let fft = FinalFantasyTactics.load memoryCard.Files.[0]

let itemSlot = 0x01uy
let quantity = fft.PlayerInventory.[int itemSlot]
let display = FinalFantasyTacticsStrings.formatInventoryItem itemSlot quantity
printfn "%s" display

let furSlot = 0x2Auy
let furQuantity = fft.FurShopInventory.[int furSlot]
let furDisplay = FinalFantasyTacticsStrings.formatFurShopItem furSlot furQuantity
printfn "%s" furDisplay
```

### Visual Basic .NET

```vb
Imports PSXMCL

Dim memoryCard = MemoryCard.Read("save.mcd")
Dim fft = FinalFantasyTactics.Load(memoryCard.Files(0))

Dim itemSlot As Byte = &H1
Dim quantity As Byte = fft.PlayerInventory(itemSlot)
Dim display As String = FinalFantasyTacticsStrings.FormatInventoryItem(itemSlot, quantity)
Console.WriteLine(display)

Dim furSlot As Byte = &H2A
Dim furQuantity As Byte = fft.FurShopInventory(furSlot)
Dim furDisplay As String = FinalFantasyTacticsStrings.FormatFurShopItem(furSlot, furQuantity)
Console.WriteLine(furDisplay)
```

### C#

```csharp
using PSXMCL;

var memoryCard = MemoryCard.Read("save.mcd");
var fft = FinalFantasyTactics.Load(memoryCard.Files[0]);

byte itemSlot = 0x01;
byte quantity = fft.PlayerInventory[itemSlot];
string display = FinalFantasyTacticsStrings.FormatInventoryItem(itemSlot, quantity);
Console.WriteLine(display);

byte furSlot = 0x2A;
byte furQuantity = fft.FurShopInventory[furSlot];
string furDisplay = FinalFantasyTacticsStrings.FormatFurShopItem(furSlot, furQuantity);
Console.WriteLine(furDisplay);
```

## 3. Build a unit summary for a roster screen

### F#

```fsharp
open PSXMCL

let memoryCard = MemoryCard.read "save.mcd"
let fft = FinalFantasyTactics.load memoryCard.Files.[0]

for unit in fft.Units do
    let summary = FinalFantasyTacticsStrings.formatUnitSummary unit
    printfn "%s" summary
```

### Visual Basic .NET

```vb
Imports PSXMCL

Dim memoryCard = MemoryCard.Read("save.mcd")
Dim fft = FinalFantasyTactics.Load(memoryCard.Files(0))

For Each unit In fft.Units
    Dim summary As String = FinalFantasyTacticsStrings.FormatUnitSummary(unit)
    Console.WriteLine(summary)
Next
```

### C#

```csharp
using PSXMCL;

var memoryCard = MemoryCard.Read("save.mcd");
var fft = FinalFantasyTactics.Load(memoryCard.Files[0]);

foreach (var unit in fft.Units)
{
    string summary = FinalFantasyTacticsStrings.FormatUnitSummary(unit);
    Console.WriteLine(summary);
}
```

## 4. Edit a save and write it back

### F# 

```fsharp
open PSXMCL

let memoryCard = MemoryCard.read "save.mcd"
let fft = FinalFantasyTactics.load memoryCard.Files.[0]

let editedFft =
    { fft with
        MainCharName = FinalFantasyTacticsStrings.encodeMainCharacterName "Ramza"
        MainCharLevel = 99uy
        MainCharJob = 0x35uy }

let updatedSaveFile = FinalFantasyTactics.toSaveFile editedFft memoryCard.Files.[0]
let updatedMemoryCard = MemoryCard.withFileData updatedSaveFile memoryCard
MemoryCard.write "save_modified.mcd" updatedMemoryCard
```

## 5. Use enum-oriented edit helpers

### F#

```fsharp
open PSXMCL

let memoryCard = MemoryCard.read "save.mcd"
let fft = FinalFantasyTactics.load memoryCard.Files.[0]

let firstUnit = fft.Units.[0]

let editedUnit =
    firstUnit
    |> FinalFantasyTacticsEnums.setEquipment FinalFantasyTacticsEnums.EquipmentSlot.Head 0x90uy
    |> FinalFantasyTacticsEnums.setAbility FinalFantasyTacticsEnums.AbilitySlot.Reaction 0x003Aus

let updatedUnits = Array.copy fft.Units
updatedUnits.[0] <- editedUnit

let editedFft = { fft with Units = updatedUnits }
```

## 6. Use lookup helpers directly

### Visual Basic .NET

```vb
Imports PSXMCL

Dim memoryCard = MemoryCard.Read("save.mcd")
Dim fft = FinalFantasyTactics.Load(memoryCard.Files(0))

Dim editedFft = New FinalFantasyTactics.FftSaveData With {
    .MainCharLevel = 99,
    .MainCharJob = &H35
}

Dim updatedSaveFile = FinalFantasyTactics.ToSaveFile(editedFft, memoryCard.Files(0))
Dim updatedMemoryCard = MemoryCard.WithFileData(updatedSaveFile, memoryCard)
MemoryCard.Write("save_modified.mcd", updatedMemoryCard)
```

### C#

```csharp
using PSXMCL;

var memoryCard = MemoryCard.Read("save.mcd");
var fft = FinalFantasyTactics.Load(memoryCard.Files[0]);

var editedFft = new FinalFantasyTactics.FftSaveData
{
    MainCharLevel = 99,
    MainCharJob = 0x35
};

var updatedSaveFile = FinalFantasyTactics.ToSaveFile(editedFft, memoryCard.Files[0]);
var updatedMemoryCard = MemoryCard.WithFileData(updatedSaveFile, memoryCard);
MemoryCard.Write("save_modified.mcd", updatedMemoryCard);
```

### F#

### F#

```fsharp
open PSXMCL

let itemName = FinalFantasyTacticsStrings.getItemName 0x01uy
let jobName = FinalFantasyTacticsStrings.getJobName 0x35uy
let zodiacName = FinalFantasyTacticsStrings.getZodiacName FinalFantasyTactics.ZodiacSign.Scorpio

printfn "%s / %s / %s" itemName jobName zodiacName
```

### Visual Basic .NET

```vb
Imports PSXMCL

Dim itemName As String = FinalFantasyTacticsStrings.GetItemName(&H1)
Dim jobName As String = FinalFantasyTacticsStrings.GetJobName(&H35)
Dim zodiacName As String = FinalFantasyTacticsStrings.GetZodiacName(FinalFantasyTactics.ZodiacSign.Scorpio)

Console.WriteLine($"{itemName} / {jobName} / {zodiacName}")
```

### C#

```csharp
using PSXMCL;

string itemName = FinalFantasyTacticsStrings.GetItemName(0x01);
string jobName = FinalFantasyTacticsStrings.GetJobName(0x35);
string zodiacName = FinalFantasyTacticsStrings.GetZodiacName(FinalFantasyTactics.ZodiacSign.Scorpio);

Console.WriteLine($"{itemName} / {jobName} / {zodiacName}");
```

## Notes

- `FinalFantasyTactics` works with parsed save data, not raw memory-card sectors.
- `FinalFantasyTacticsStrings` is intentionally presentation-only and does not mutate data.
- If you need additional display helpers for abilities, equipment slots, or character identities, they can be added alongside the current lookup functions.
