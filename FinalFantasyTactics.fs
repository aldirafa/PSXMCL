namespace PSXMCL

open System

/// <summary>
/// Loader and writer for Final Fantasy Tactics save data stored within a PSX memory card
/// <see cref="MemoryCard.SaveFile"/>.
/// </summary>
/// <remarks>
/// <para>
/// An FFT save occupies exactly one memory card block (8 KB).  The block is
/// structured as follows:
/// </para>
/// <list type="bullet">
///   <item><description>Frame 0 (bytes 0x0000–0x007F): title frame ("SC" header, Shift-JIS title, palette).</description></item>
///   <item><description>Frame 1 (bytes 0x0080–0x00FF): icon bitmap (16×16, 4 bpp).</description></item>
///   <item><description>Frames 2–63 (bytes 0x0100–0x1FFF): FFT game data, represented by <see cref="MemoryCard.SaveFile.Data"/>.</description></item>
/// </list>
/// <para>
/// All offset constants in this module are relative to the start of
/// <see cref="MemoryCard.SaveFile.Data"/> (i.e. the fftref.md table offset minus 0x100).
/// </para>
/// </remarks>
module FinalFantasyTactics =

    // ── Save-data offsets (relative to SaveFile.Data, i.e. fftref offset − 0x100) ──

    [<Literal>]
    let private OffMainCharName = 0x0001 // 0x0101 in fftref; 17 bytes

    [<Literal>]
    let private MainCharNameLen = 17

    [<Literal>]
    let private OffMainCharJob = 0x0012 // 0x0112

    [<Literal>]
    let private OffMainCharLevel = 0x0013 // 0x0113

    [<Literal>]
    let private OffDate = 0x0014 // 0x0114; byte 0 = month, byte 1 = day

    [<Literal>]
    let private OffMapPosition = 0x0016 // 0x0116

    [<Literal>]
    let private OffGameTime = 0x0020 // 0x0120; 4 bytes LE

    [<Literal>]
    let private OffWorldStats = 0x0384 // 0x0484; 20 units × 0xE0 bytes

    [<Literal>]
    let private UnitCount = 20

    /// <summary>Size of one real unit record in the save file (224 bytes).</summary>
    [<Literal>]
    let UnitSize = 0xE0

    [<Literal>]
    let private OffPlayerInventory = 0x1504 // 0x1604; 256 bytes

    [<Literal>]
    let private InventorySize = 256

    [<Literal>]
    let private OffFurShopInventory = 0x1604 // 0x1704; 256 bytes

    [<Literal>]
    let private OffPlayerOptions = 0x1B84 // 0x1C84; 4 bytes

    [<Literal>]
    let private PlayerOptionsSize = 4

    // ── Types ─────────────────────────────────────────────────────────────────────

    /// <summary>Zodiac sign stored in unit byte 0x06 (upper nibble).</summary>
    type ZodiacSign =
        | Aries
        | Taurus
        | Gemini
        | Cancer
        | Leo
        | Virgo
        | Libra
        | Scorpio
        | Sagittarius
        | Capricorn
        | Aquarius
        | Pisces
        | Serpentarius
        | UnknownZodiac of byte

    /// <summary>
    /// Real-unit stats record as stored in the save file world-stats area
    /// (one record per roster slot, 0xE0 bytes each).
    /// </summary>
    /// <remarks>
    /// Fields are decoded from the raw 0xE0-byte block according to fftref.md.
    /// <see cref="RawData"/> preserves the original 0xE0 bytes and is used as the
    /// base buffer when serialising back, so any undocumented fields are round-tripped
    /// transparently.
    /// </remarks>
    type UnitStats =
        {
            /// <summary>Character identity (0x00–0x7F special, 0x80 generic male, 0x81 female, 0x82 monster).</summary>
            CharacterIdentity: byte
            /// <summary>Party ID; 0xFF means the unit is not on the formation screen.</summary>
            PartyId: byte
            /// <summary>Current job ID.</summary>
            JobId: byte
            /// <summary>Palette index.</summary>
            Palette: byte
            /// <summary>
            /// Gender/flag byte.
            /// bit 7 = Male, bit 6 = Female, bit 5 = Monster,
            /// bit 4 = Join after event, bit 3 = Load Formation,
            /// bit 2 = Is Egg, bit 0 = Save Formation.
            /// </summary>
            GenderFlags: byte
            /// <summary>
            /// Birthday encoded as a day-of-year value (9 bits:
            /// low 8 bits from byte 0x05, bit 0 from byte 0x06).
            /// </summary>
            Birthday: uint16
            /// <summary>Zodiac sign (upper nibble of byte 0x06).</summary>
            Zodiac: ZodiacSign
            /// <summary>Secondary skillset ID.</summary>
            SecondarySkillset: byte
            /// <summary>Reaction ability ID (2 bytes at unit offset 0x08).</summary>
            ReactionAbility: uint16
            /// <summary>Support ability ID (2 bytes at unit offset 0x0A).</summary>
            SupportAbility: uint16
            /// <summary>Movement ability ID (2 bytes at unit offset 0x0C).</summary>
            MovementAbility: uint16
            /// <summary>Head equipment item ID.</summary>
            Head: byte
            /// <summary>Body equipment item ID.</summary>
            Body: byte
            /// <summary>Accessory item ID.</summary>
            Accessory: byte
            /// <summary>Right-hand weapon item ID.</summary>
            RightHandWeapon: byte
            /// <summary>Right-hand shield item ID.</summary>
            RightHandShield: byte
            /// <summary>Left-hand weapon item ID.</summary>
            LeftHandWeapon: byte
            /// <summary>Left-hand shield item ID.</summary>
            LeftHandShield: byte
            /// <summary>Current experience points.</summary>
            Experience: byte
            /// <summary>Unit level.</summary>
            Level: byte
            /// <summary>Brave value.</summary>
            Brave: byte
            /// <summary>Faith value.</summary>
            Faith: byte
            /// <summary>Raw HP (24-bit LE at unit offsets 0x19–0x1B).</summary>
            RawHp: uint32
            /// <summary>Raw MP (24-bit LE at unit offsets 0x1C–0x1E).</summary>
            RawMp: uint32
            /// <summary>Raw SP (24-bit LE at unit offsets 0x1F–0x21).</summary>
            RawSp: uint32
            /// <summary>Raw PA (24-bit LE at unit offsets 0x22–0x24).</summary>
            RawPa: uint32
            /// <summary>Raw MA (24-bit LE at unit offsets 0x25–0x27).</summary>
            RawMa: uint32
            /// <summary>Unlocked-jobs flags (3 bytes at unit offsets 0x28–0x2A; bit-packed, 20 jobs).</summary>
            UnlockedJobs: byte[]
            /// <summary>
            /// Job action-ability and R/S/M learn flags (57 bytes at unit offsets 0x2B–0x63;
            /// 19 jobs x 3 bytes each: action 1-8, action 9-16, R/S/M; not including Mime).
            /// </summary>
            JobAbilities: byte[]
            /// <summary>
            /// Job levels (10 bytes at unit offsets 0x64–0x6D;
            /// 2 jobs packed per byte: high nibble = first job, low nibble = second).
            /// </summary>
            JobLevels: byte[]
            /// <summary>
            /// Current JP for each of 20 jobs (uint16 LE each; Base through Mime,
            /// at unit offsets 0x6E–0x95).
            /// </summary>
            JobJp: uint16[]
            /// <summary>
            /// Total accumulated JP for each of 20 jobs (uint16 LE each;
            /// at unit offsets 0x96–0xBD).
            /// </summary>
            TotalJobJp: uint16[]
            /// <summary>Unit nickname raw bytes (16 bytes at unit offsets 0xBE–0xCD).</summary>
            Nickname: byte[]
            /// <summary>Unit name ID (uint16 LE at unit offsets 0xCE–0xCF).</summary>
            NameId: uint16
            /// <summary>
            /// Full raw 0xE0-byte unit record.
            /// Used as the serialisation base so undocumented fields are preserved.
            /// </summary>
            RawData: byte[]
        }

    /// <summary>
    /// Full FFT save data loaded from a <see cref="MemoryCard.SaveFile"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="RawData"/> is a copy of <see cref="MemoryCard.SaveFile.Data"/> and is used
    /// as the serialisation base so that undocumented regions are round-tripped unchanged.
    /// Call <see cref="toSaveFile"/> to write modified data back to a <see cref="MemoryCard.SaveFile"/>.
    /// </remarks>
    type FftSaveData =
        {
            /// <summary>Main character name (raw bytes, 17 bytes at save-data offset 0x0001).</summary>
            MainCharName: byte[]
            /// <summary>Main character current job ID.</summary>
            MainCharJob: byte
            /// <summary>Main character level.</summary>
            MainCharLevel: byte
            /// <summary>Save date: (month, day).</summary>
            SaveDate: byte * byte
            /// <summary>Party map position.</summary>
            MapPosition: byte
            /// <summary>Game time (4 bytes LE at save-data offset 0x0020).</summary>
            GameTime: uint32
            /// <summary>Up to 20 roster unit records.</summary>
            Units: UnitStats[]
            /// <summary>
            /// Player shop inventory (256 bytes, one byte per item ID indicating quantity,
            /// at save-data offset 0x1504).
            /// </summary>
            PlayerInventory: byte[]
            /// <summary>
            /// Fur shop inventory (256 bytes, at save-data offset 0x1604).
            /// </summary>
            FurShopInventory: byte[]
            /// <summary>Player options (4 bytes at save-data offset 0x1B84).</summary>
            PlayerOptions: byte[]
            /// <summary>
            /// Full raw copy of <see cref="MemoryCard.SaveFile.Data"/> (7 936 bytes).
            /// Used as the serialisation base; call <see cref="toSaveFile"/> to produce an
            /// updated <see cref="MemoryCard.SaveFile"/>.
            /// </summary>
            RawData: byte[]
        }

    // ── Internal helpers ──────────────────────────────────────────────────────────

    let private readU16LE (buf: byte[]) off = BitConverter.ToUInt16(buf, off)

    let private readU24LE (buf: byte[]) off =
        uint32 buf.[off]
        ||| (uint32 buf.[off + 1] <<< 8)
        ||| (uint32 buf.[off + 2] <<< 16)

    let private readU32LE (buf: byte[]) off = BitConverter.ToUInt32(buf, off)

    let private writeU16LE (buf: byte[]) off (value: uint16) =
        buf.[off] <- byte value
        buf.[off + 1] <- byte (value >>> 8)

    let private writeU24LE (buf: byte[]) off (value: uint32) =
        buf.[off] <- byte (value &&& 0xFFu)
        buf.[off + 1] <- byte ((value >>> 8) &&& 0xFFu)
        buf.[off + 2] <- byte ((value >>> 16) &&& 0xFFu)

    let private writeU32LE (buf: byte[]) off (value: uint32) =
        buf.[off] <- byte (value &&& 0xFFu)
        buf.[off + 1] <- byte ((value >>> 8) &&& 0xFFu)
        buf.[off + 2] <- byte ((value >>> 16) &&& 0xFFu)
        buf.[off + 3] <- byte ((value >>> 24) &&& 0xFFu)

    let private parseZodiac (b: byte) =
        match b &&& 0xF0uy with
        | 0x00uy -> Aries
        | 0x10uy -> Taurus
        | 0x20uy -> Gemini
        | 0x30uy -> Cancer
        | 0x40uy -> Leo
        | 0x50uy -> Virgo
        | 0x60uy -> Libra
        | 0x70uy -> Scorpio
        | 0x80uy -> Sagittarius
        | 0x90uy -> Capricorn
        | 0xA0uy -> Aquarius
        | 0xB0uy -> Pisces
        | 0xC0uy -> Serpentarius
        | v -> UnknownZodiac v

    let private zodiacByte (z: ZodiacSign) =
        match z with
        | Aries -> 0x00uy
        | Taurus -> 0x10uy
        | Gemini -> 0x20uy
        | Cancer -> 0x30uy
        | Leo -> 0x40uy
        | Virgo -> 0x50uy
        | Libra -> 0x60uy
        | Scorpio -> 0x70uy
        | Sagittarius -> 0x80uy
        | Capricorn -> 0x90uy
        | Aquarius -> 0xA0uy
        | Pisces -> 0xB0uy
        | Serpentarius -> 0xC0uy
        | UnknownZodiac v -> v

    let private parseUnit (data: byte[]) baseOff =
        let b off = data.[baseOff + off]

        // Birthday: 8 low bits from byte 0x05, bit 0 from byte 0x06
        let birthday = uint16 (b 0x05) ||| (uint16 (b 0x06 &&& 0x01uy) <<< 8)

        { CharacterIdentity = b 0x00
          PartyId = b 0x01
          JobId = b 0x02
          Palette = b 0x03
          GenderFlags = b 0x04
          Birthday = birthday
          Zodiac = parseZodiac (b 0x06)
          SecondarySkillset = b 0x07
          ReactionAbility = readU16LE data (baseOff + 0x08)
          SupportAbility = readU16LE data (baseOff + 0x0A)
          MovementAbility = readU16LE data (baseOff + 0x0C)
          Head = b 0x0E
          Body = b 0x0F
          Accessory = b 0x10
          RightHandWeapon = b 0x11
          RightHandShield = b 0x12
          LeftHandWeapon = b 0x13
          LeftHandShield = b 0x14
          Experience = b 0x15
          Level = b 0x16
          Brave = b 0x17
          Faith = b 0x18
          RawHp = readU24LE data (baseOff + 0x19)
          RawMp = readU24LE data (baseOff + 0x1C)
          RawSp = readU24LE data (baseOff + 0x1F)
          RawPa = readU24LE data (baseOff + 0x22)
          RawMa = readU24LE data (baseOff + 0x25)
          UnlockedJobs = data.[baseOff + 0x28 .. baseOff + 0x2A]
          JobAbilities = data.[baseOff + 0x2B .. baseOff + 0x63]
          JobLevels = data.[baseOff + 0x64 .. baseOff + 0x6D]
          JobJp = Array.init 20 (fun i -> readU16LE data (baseOff + 0x6E + i * 2))
          TotalJobJp = Array.init 20 (fun i -> readU16LE data (baseOff + 0x96 + i * 2))
          Nickname = data.[baseOff + 0xBE .. baseOff + 0xCD]
          NameId = readU16LE data (baseOff + 0xCE)
          RawData = data.[baseOff .. baseOff + UnitSize - 1] }

    let private serializeUnit (unit: UnitStats) : byte[] =
        let buf = Array.copy unit.RawData

        buf.[0x00] <- unit.CharacterIdentity
        buf.[0x01] <- unit.PartyId
        buf.[0x02] <- unit.JobId
        buf.[0x03] <- unit.Palette
        buf.[0x04] <- unit.GenderFlags

        buf.[0x05] <- byte unit.Birthday
        // byte 0x06: upper nibble = zodiac, bit 0 = birthday overflow bit
        buf.[0x06] <- zodiacByte unit.Zodiac ||| (byte (unit.Birthday >>> 8) &&& 0x01uy)

        buf.[0x07] <- unit.SecondarySkillset
        writeU16LE buf 0x08 unit.ReactionAbility
        writeU16LE buf 0x0A unit.SupportAbility
        writeU16LE buf 0x0C unit.MovementAbility

        buf.[0x0E] <- unit.Head
        buf.[0x0F] <- unit.Body
        buf.[0x10] <- unit.Accessory
        buf.[0x11] <- unit.RightHandWeapon
        buf.[0x12] <- unit.RightHandShield
        buf.[0x13] <- unit.LeftHandWeapon
        buf.[0x14] <- unit.LeftHandShield
        buf.[0x15] <- unit.Experience
        buf.[0x16] <- unit.Level
        buf.[0x17] <- unit.Brave
        buf.[0x18] <- unit.Faith

        writeU24LE buf 0x19 unit.RawHp
        writeU24LE buf 0x1C unit.RawMp
        writeU24LE buf 0x1F unit.RawSp
        writeU24LE buf 0x22 unit.RawPa
        writeU24LE buf 0x25 unit.RawMa

        Array.blit unit.UnlockedJobs 0 buf 0x28 (min unit.UnlockedJobs.Length 3)
        Array.blit unit.JobAbilities 0 buf 0x2B (min unit.JobAbilities.Length 57)
        Array.blit unit.JobLevels 0 buf 0x64 (min unit.JobLevels.Length 10)

        for i in 0..19 do
            writeU16LE buf (0x6E + i * 2) unit.JobJp.[i]

        for i in 0..19 do
            writeU16LE buf (0x96 + i * 2) unit.TotalJobJp.[i]

        Array.blit unit.Nickname 0 buf 0xBE (min unit.Nickname.Length 16)
        writeU16LE buf 0xCE unit.NameId

        buf

    // ── Public API ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Loads FFT save data from a <see cref="MemoryCard.SaveFile"/>.
    /// </summary>
    /// <param name="saveFile">
    /// A save file parsed from a PSX memory card.  Must contain a valid FFT save
    /// (i.e. <see cref="MemoryCard.SaveFile.Data"/> must be at least 7 936 bytes).
    /// </param>
    /// <returns>A fully parsed <see cref="FftSaveData"/> value.</returns>
    /// <exception cref="System.Exception">
    /// Thrown when <paramref name="saveFile"/> does not contain enough data to hold
    /// a complete FFT save.
    /// </exception>
    let load (saveFile: MemoryCard.SaveFile) : FftSaveData =
        let d = saveFile.Data
        let minRequired = OffPlayerOptions + PlayerOptionsSize

        if d.Length < minRequired then
            failwithf
                "Save file data too short for an FFT save: expected at least %d bytes, got %d"
                minRequired
                d.Length

        { MainCharName = d.[OffMainCharName .. OffMainCharName + MainCharNameLen - 1]
          MainCharJob = d.[OffMainCharJob]
          MainCharLevel = d.[OffMainCharLevel]
          SaveDate = (d.[OffDate], d.[OffDate + 1])
          MapPosition = d.[OffMapPosition]
          GameTime = readU32LE d OffGameTime
          Units = Array.init UnitCount (fun i -> parseUnit d (OffWorldStats + i * UnitSize))
          PlayerInventory = d.[OffPlayerInventory .. OffPlayerInventory + InventorySize - 1]
          FurShopInventory = d.[OffFurShopInventory .. OffFurShopInventory + InventorySize - 1]
          PlayerOptions = d.[OffPlayerOptions .. OffPlayerOptions + PlayerOptionsSize - 1]
          RawData = Array.copy d }

    /// <summary>
    /// Serialises modified <see cref="FftSaveData"/> back into a
    /// <see cref="MemoryCard.SaveFile"/>, returning a new save file with an updated
    /// <see cref="MemoryCard.SaveFile.Data"/> byte array.
    /// </summary>
    /// <param name="fft">The (possibly modified) FFT save data to serialise.</param>
    /// <param name="saveFile">
    /// The original <see cref="MemoryCard.SaveFile"/> whose metadata (directory
    /// entry, title frame, icon frames, block indices) is preserved unchanged.
    /// </param>
    /// <returns>
    /// A new <see cref="MemoryCard.SaveFile"/> whose <c>Data</c> reflects all changes
    /// in <paramref name="fft"/>.  Pass the result to
    /// <see cref="MemoryCard.withFileData"/> and then <see cref="MemoryCard.write"/>
    /// to persist the changes to disk.
    /// </returns>
    let toSaveFile (fft: FftSaveData) (saveFile: MemoryCard.SaveFile) : MemoryCard.SaveFile =
        let buf = Array.copy fft.RawData

        Array.blit fft.MainCharName 0 buf OffMainCharName (min fft.MainCharName.Length MainCharNameLen)
        buf.[OffMainCharJob] <- fft.MainCharJob
        buf.[OffMainCharLevel] <- fft.MainCharLevel

        let month, day = fft.SaveDate
        buf.[OffDate] <- month
        buf.[OffDate + 1] <- day
        buf.[OffMapPosition] <- fft.MapPosition

        writeU32LE buf OffGameTime fft.GameTime

        for i in 0 .. UnitCount - 1 do
            if i < fft.Units.Length then
                let unitBytes = serializeUnit fft.Units.[i]
                Array.blit unitBytes 0 buf (OffWorldStats + i * UnitSize) UnitSize

        Array.blit fft.PlayerInventory 0 buf OffPlayerInventory (min fft.PlayerInventory.Length InventorySize)
        Array.blit fft.FurShopInventory 0 buf OffFurShopInventory (min fft.FurShopInventory.Length InventorySize)
        Array.blit fft.PlayerOptions 0 buf OffPlayerOptions (min fft.PlayerOptions.Length PlayerOptionsSize)

        { saveFile with Data = buf }
