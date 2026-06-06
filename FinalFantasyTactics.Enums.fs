namespace PSXMCL

open System

/// <summary>
/// Enum-oriented helpers and typed value aliases for editing Final Fantasy Tactics save data.
/// </summary>
/// <remarks>
/// This module is intentionally focused on edit-time domain values. Display formatting
/// and text rendering stay in <see cref="FinalFantasyTacticsStrings"/>.
/// </remarks>
module FinalFantasyTacticsEnums =

    /// <summary>Opaque alias for item IDs used by inventory and equipment slots.</summary>
    type ItemId = byte

    /// <summary>Opaque alias for job IDs used by units and the main character.</summary>
    type JobId = byte

    /// <summary>Opaque alias for ability IDs used in reaction/support/movement slots.</summary>
    type AbilityId = uint16

    /// <summary>
    /// Known character identity values used in unit records.
    /// </summary>
    /// <remarks>
    /// This enum intentionally exposes common/safe constants and generic identities.
    /// The full 0x00-0x7F identity range is sparse and should still be treated as raw bytes when needed.
    /// </remarks>
    type CharacterIdentity =
        | Ramza = 0x01uy
        | Delita = 0x04uy
        | Agrias = 0x1Euy
        | Mustadio = 0x16uy
        | Rafa = 0x29uy
        | Malak = 0x12uy
        | Meliadoul = 0x2Auy
        | Cloud = 0x32uy
        | GenericMale = 0x80uy
        | GenericFemale = 0x81uy
        | GenericMonster = 0x82uy

    /// <summary>Known palette values used by unit records.</summary>
    type Palette =
        | Default = 0uy
        | Hokuten = 1uy
        | Nanten = 2uy
        | DeathCorps = 3uy
        | GlabadosChurch = 4uy

    /// <summary>
    /// Bit flags stored in <c>UnitStats.GenderFlags</c>.
    /// </summary>
    [<Flags>]
    type GenderFlags =
        | None = 0uy
        | Male = 0x80uy
        | Female = 0x40uy
        | Monster = 0x20uy
        | JoinAfterEvent = 0x10uy
        | LoadFormation = 0x08uy
        | IsEgg = 0x04uy
        | SaveFormation = 0x01uy

    /// <summary>Equipment slot selector for unit editing helpers.</summary>
    type EquipmentSlot =
        | Head = 0
        | Body = 1
        | Accessory = 2
        | RightHandWeapon = 3
        | RightHandShield = 4
        | LeftHandWeapon = 5
        | LeftHandShield = 6

    /// <summary>Ability slot selector for unit editing helpers.</summary>
    type AbilitySlot =
        | Reaction = 0
        | Support = 1
        | Movement = 2

    /// <summary>Converts a character identity enum value to its raw byte representation.</summary>
    let characterIdentityValue (value: CharacterIdentity) = byte value

    /// <summary>Converts a palette enum value to its raw byte representation.</summary>
    let paletteValue (value: Palette) = byte value

    /// <summary>Attempts to parse a raw palette byte into a known <see cref="Palette"/> value.</summary>
    /// <param name="value">Raw palette byte.</param>
    /// <returns><c>Some palette</c> for known values, otherwise <c>None</c>.</returns>
    let tryPalette (value: byte) =
        match value with
        | 0uy -> Some Palette.Default
        | 1uy -> Some Palette.Hokuten
        | 2uy -> Some Palette.Nanten
        | 3uy -> Some Palette.DeathCorps
        | 4uy -> Some Palette.GlabadosChurch
        | _ -> None

    /// <summary>Reads raw gender-flag bits as a typed <see cref="GenderFlags"/> value.</summary>
    let toGenderFlags (value: byte) : GenderFlags =
        LanguagePrimitives.EnumOfValue<byte, GenderFlags> value

    /// <summary>Converts typed <see cref="GenderFlags"/> back to the raw save-data byte.</summary>
    let fromGenderFlags (value: GenderFlags) = byte value

    /// <summary>Checks whether a given flag bit is set in the raw gender-flags byte.</summary>
    let hasGenderFlag (flag: GenderFlags) (value: byte) =
        let current = toGenderFlags value
        (current &&& flag) = flag

    /// <summary>Sets or clears a specific gender-flag bit in the raw gender-flags byte.</summary>
    let setGenderFlag (flag: GenderFlags) (enabled: bool) (value: byte) =
        let current = toGenderFlags value
        let updated = if enabled then current ||| flag else current &&& (~~~flag)
        fromGenderFlags updated

    /// <summary>Reads an equipment item ID from a selected unit slot.</summary>
    let getEquipment (slot: EquipmentSlot) (unit: FinalFantasyTactics.UnitStats) : ItemId =
        match slot with
        | EquipmentSlot.Head -> unit.Head
        | EquipmentSlot.Body -> unit.Body
        | EquipmentSlot.Accessory -> unit.Accessory
        | EquipmentSlot.RightHandWeapon -> unit.RightHandWeapon
        | EquipmentSlot.RightHandShield -> unit.RightHandShield
        | EquipmentSlot.LeftHandWeapon -> unit.LeftHandWeapon
        | EquipmentSlot.LeftHandShield -> unit.LeftHandShield
        | _ -> invalidArg "slot" "Unsupported equipment slot."

    /// <summary>Returns a copy of a unit with the selected equipment slot changed.</summary>
    let setEquipment (slot: EquipmentSlot) (itemId: ItemId) (unit: FinalFantasyTactics.UnitStats) =
        match slot with
        | EquipmentSlot.Head -> { unit with Head = itemId }
        | EquipmentSlot.Body -> { unit with Body = itemId }
        | EquipmentSlot.Accessory -> { unit with Accessory = itemId }
        | EquipmentSlot.RightHandWeapon -> { unit with RightHandWeapon = itemId }
        | EquipmentSlot.RightHandShield -> { unit with RightHandShield = itemId }
        | EquipmentSlot.LeftHandWeapon -> { unit with LeftHandWeapon = itemId }
        | EquipmentSlot.LeftHandShield -> { unit with LeftHandShield = itemId }
        | _ -> invalidArg "slot" "Unsupported equipment slot."

    /// <summary>Reads an ability ID from the selected unit ability slot.</summary>
    let getAbility (slot: AbilitySlot) (unit: FinalFantasyTactics.UnitStats) : AbilityId =
        match slot with
        | AbilitySlot.Reaction -> unit.ReactionAbility
        | AbilitySlot.Support -> unit.SupportAbility
        | AbilitySlot.Movement -> unit.MovementAbility
        | _ -> invalidArg "slot" "Unsupported ability slot."

    /// <summary>Returns a copy of a unit with the selected ability slot changed.</summary>
    let setAbility (slot: AbilitySlot) (abilityId: AbilityId) (unit: FinalFantasyTactics.UnitStats) =
        match slot with
        | AbilitySlot.Reaction ->
            { unit with
                ReactionAbility = abilityId }
        | AbilitySlot.Support -> { unit with SupportAbility = abilityId }
        | AbilitySlot.Movement ->
            { unit with
                MovementAbility = abilityId }
        | _ -> invalidArg "slot" "Unsupported ability slot."

    /// <summary>
    /// Validates whether an ability ID is within the commonly documented FFT range (0x0000-0x01FF).
    /// </summary>
    let isKnownAbilityRange (abilityId: AbilityId) = abilityId <= 0x01FFus
