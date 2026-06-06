The library is organized under the root namespace `PSXMCL`. Below is a detailed breakdown of the main modules, record types, and functions.

---

### 1. Module `MemoryCard`

Handles low-level PlayStation memory card files.

#### Core Types

* **`MemoryCard`** (Record)
  * `HeaderValid: bool` — Checks for the ASCII `MC` signature and valid checksum in Block 0.
  * `Directory: DirectoryEntry[]` — The 15 directory entries.
  * `BrokenSectors: BrokenSector[]` — Records identifying damaged sectors to exclude.
  * `Files: SaveFile[]` — Extracted save files.
  * `RawData: byte[]` — The raw 128 KB card image bytes.

* **`SaveFile`** (Record)
  * `BlockIndices: int[]` — Indices of the physical blocks occupied by the file.
  * `DirectoryEntry: DirectoryEntry` — Directory information for the file.
  * `TitleFrame: TitleFrame option` — Optional title and icon details.
  * `IconFrames: IconFrame[]` — Extracted animated icon frame bitmaps (16x16, 4-bpp).
  * `Data: byte[]` — Concatenated raw payload bytes.

* **`DirectoryEntry`** (Record)
  * `State: BlockAllocationState` — Current block allocation state (e.g., in use, free, deleted).
  * `FileSize: uint32` — File size in bytes.
  * `NextBlock: uint16` — Link to the next block in the chain (`0xFFFF` = end of chain).
  * `FileName: string` — ASCII file name on the card.
  * `IsValid: bool` — True if directory entry checksum validates.

* **`TitleFrame`** (Record)
  * `IconDisplayFlag: IconDisplayFlag` — Decides how many icon frames are present and how they animate.
  * `BlockNumber: byte` — Block index of this file (1 to 15).
  * `Title: string` — Shift-JIS decoded title string.
  * `Palette: uint16[]` — 16-color CLUT palette used to draw icons.
  * `RawTitle: byte[]` — Raw bytes of the title buffer.

#### Key Functions

* **`read` / `Read`**
  * **F#**: `val read: path: string -> MemoryCard`
  * **C#/VB**: `public static MemoryCard Read(string path)`
  * Reads a 128 KB memory card file from the file system.

* **`readFromBytes` / `ReadFromBytes`**
  * **F#**: `val readFromBytes: data: byte[] -> MemoryCard`
  * **C#/VB**: `public static MemoryCard ReadFromBytes(byte[] data)`
  * Parses a memory card image directly from a byte array.

* **`withFileData` / `WithFileData`**
  * **F#**: `val withFileData: saveFile: SaveFile -> mc: MemoryCard -> MemoryCard`
  * **C#/VB**: `public static MemoryCard WithFileData(SaveFile saveFile, MemoryCard mc)`
  * Returns a copy of the memory card where a save file has been updated or written back.

* **`write` / `Write`**
  * **F#**: `val write: path: string -> mc: MemoryCard -> unit`
  * **C#/VB**: `public static void Write(string path, MemoryCard mc)`
  * Writes a memory card representation to the file system.

---

### 2. Module `FinalFantasyTactics`

Parses and packages Final Fantasy Tactics save payloads.

#### Core Types

* **`FftSaveData`** (Record)
  * `MainCharName: byte[]` — Raw name array (17 bytes).
  * `MainCharJob: byte` — Current job ID of the main character.
  * `MainCharLevel: byte` — Current level.
  * `SaveDate: byte * byte` — Tuple representing the save date `(month, day)`.
  * `MapPosition: byte` — Party's coordinate/location on the world map.
  * `GameTime: uint32` — Total play time counter.
  * `Units: UnitStats[]` — Array of 20 unit records.
  * `PlayerInventory: byte[]` — Quantities of player inventory indexed by item ID (256 bytes).
  * `FurShopInventory: byte[]` — Quantities of fur-shop items (256 bytes).
  * `PlayerOptions: byte[]` — 4 bytes of player configuration toggles.
  * `RawData: byte[]` — Clean copy of the original payload for round-tripping.

* **`UnitStats`** (Record)
  * `CharacterIdentity: byte` — Special character ID or generic category flag (e.g., `0x80` for male, `0x81` for female).
  * `PartyId: byte` — Roster block placement (`0xFF` indicates off-formation).
  * `JobId: byte` — Current active job ID.
  * `Palette: byte` — Costume palette color.
  * `GenderFlags: byte` — Gender, monster, egg, and joining attributes.
  * `Birthday: uint16` — Birth date encoded as day-of-year.
  * `Zodiac: ZodiacSign` — Zodiac sign.
  * `SecondarySkillset: byte` — Secondary action ability set.
  * `ReactionAbility / SupportAbility / MovementAbility: uint16` — Equipped passive slots.
  * `Head / Body / Accessory / RightHandWeapon / RightHandShield / LeftHandWeapon / LeftHandShield: byte` — Equipment slots.
  * `Experience / Level / Brave / Faith: byte` — Progression stats.
  * `RawHp / RawMp / RawSp / RawPa / RawMa: uint32` — Unscaled baseline attributes.
  * `JobJp / TotalJobJp: uint16[]` — Active and accumulated JP arrays (20 jobs).
  * `Nickname: byte[]` — Name buffer (16 bytes).

* **`ZodiacSign`** (Discriminated Union)
  * `Aries` | `Taurus` | `Gemini` | `Cancer` | `Leo` | `Virgo` | `Libra` | `Scorpio` | `Sagittarius` | `Capricorn` | `Aquarius` | `Pisces` | `Serpentarius` | `UnknownZodiac of byte`

#### Key Functions

* **`load` / `Load`**
  * **F#**: `val load: saveFile: SaveFile -> FftSaveData`
  * **C#/VB**: `public static FftSaveData Load(SaveFile saveFile)`
  * Parses a `SaveFile` payload into a fully structured FFT save representation.

* **`toSaveFile` / `ToSaveFile`**
  * **F#**: `val toSaveFile: fft: FftSaveData -> saveFile: SaveFile -> SaveFile`
  * **C#/VB**: `public static SaveFile ToSaveFile(FftSaveData fft, SaveFile saveFile)`
  * Re-serializes modified FFT save data back into a `SaveFile` object.

---

### 3. Module `FinalFantasyTacticsEnums`

Exposes enums, masks, and safe modifier utilities.

#### Core Enums

* **`CharacterIdentity`**: Common values (e.g., `Ramza = 0x01uy`, `Delita = 0x04uy`, `Agrias = 0x1Euy`, `GenericMale = 0x80uy`, etc.).
* **`Palette`**: `Default = 0uy`, `Hokuten = 1uy`, `Nanten = 2uy`, `DeathCorps = 3uy`, `GlabadosChurch = 4uy`.
* **`GenderFlags`** (Flags): `Male = 0x80uy`, `Female = 0x40uy`, `Monster = 0x20uy`, `JoinAfterEvent = 0x10uy`, `LoadFormation = 0x08uy`, `IsEgg = 0x04uy`, `SaveFormation = 0x01uy`.
* **`EquipmentSlot`**: `Head`, `Body`, `Accessory`, `RightHandWeapon`, `RightHandShield`, `LeftHandWeapon`, `LeftHandShield`.
* **`AbilitySlot`**: `Reaction`, `Support`, `Movement`.

#### Modifier Functions

* **`hasGenderFlag`**
  * `hasGenderFlag (flag: GenderFlags) (value: byte) : bool`
  * Checks whether a specific gender/roster flag bit is set.

* **`setGenderFlag`**
  * `setGenderFlag (flag: GenderFlags) (enabled: bool) (value: byte) : byte`
  * Sets or clears a specific flag bit on the raw gender byte.

* **`getEquipment`**
  * `getEquipment (slot: EquipmentSlot) (unit: UnitStats) : ItemId`
  * Reads the item ID for the selected equipment slot on a unit.

* **`setEquipment`**
  * `setEquipment (slot: EquipmentSlot) (itemId: ItemId) (unit: UnitStats) : UnitStats`
  * Returns a copy of the unit record with the selected equipment slot updated.

* **`getAbility`**
  * `getAbility (slot: AbilitySlot) (unit: UnitStats) : AbilityId`
  * Reads the ability ID for the selected passive/reaction/movement slot on a unit.

* **`setAbility`**
  * `setAbility (slot: AbilitySlot) (abilityId: AbilityId) (unit: UnitStats) : UnitStats`
  * Returns a copy of the unit record with the selected ability slot updated.

---

### 4. Module `FinalFantasyTacticsStrings`

Encodes/decodes ASCII name fields and formats display texts.

#### Key Functions

* **`getMainCharacterName`**
  * `getMainCharacterName (nameBytes: byte[]) : string`
  * Decodes the main character's raw name bytes into a string.

* **`encodeMainCharacterName`**
  * `encodeMainCharacterName (name: string) : byte[]`
  * Encodes a string name into a fixed 17-byte buffer with null padding.

* **`getUnitNickname`**
  * `getUnitNickname (nicknameBytes: byte[]) : string`
  * Decodes a unit's raw nickname bytes into a string.

* **`encodeUnitNickname`**
  * `encodeUnitNickname (nickname: string) : byte[]`
  * Encodes a string nickname into a fixed 16-byte buffer with null padding.

* **`getItemName`**
  * `getItemName (itemId: byte) : string`
  * Translates a raw item ID into its English in-game display name.

* **`getJobName`**
  * `getJobName (jobId: byte) : string`
  * Translates a raw job ID into its English in-game display name (including monsters).

* **`formatItemStack`**
  * `formatItemStack (itemId: byte) (quantity: byte) : string`
  * Formats an inventory slot nicely as `Name xQuantity` (or just `Name` if quantity is zero).

* **`formatUnitSummary`**
  * `formatUnitSummary (unit: UnitStats) : string`
  * Produces a readable description of a unit in the format `CharacterName - ActiveJob`.
