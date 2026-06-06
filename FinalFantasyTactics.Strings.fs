namespace PSXMCL

open System
open System.Text

/// <summary>
/// Presentation helpers for Final Fantasy Tactics save data.
/// </summary>
/// <remarks>
/// This module turns raw save fields into strings suitable for editors and debug UIs.
/// It keeps parsing responsibilities in <see cref="FinalFantasyTactics"/> and only
/// handles lookup/formatting.
/// </remarks>
module FinalFantasyTacticsStrings =

    // ----- Private data and helper functions -----
    let private itemNames =
        [| "None"
           "Dagger"
           "Mythril Knife"
           "Blind Knife"
           "Mage Masher"
           "Platina Dagger"
           "Main Gauche"
           "Orichalcum"
           "Assassin Dagger"
           "Air Knife"
           "Zorlin Shape"
           "Hidden Knife"
           "Ninja Knife"
           "Short Edge"
           "Ninja Edge"
           "Spell Edge"
           "Sasuke Knife"
           "Iga Knife"
           "Koga Knife"
           "Broad Sword"
           "Long Sword"
           "Iron Sword"
           "Mythril Sword"
           "Blood Sword"
           "Coral Sword"
           "Ancient Sword"
           "Sleep Sword"
           "Platinum Sword"
           "Diamond Sword"
           "Ice Brand"
           "Rune Blade"
           "Nagrarock"
           "Materia Blade"
           "Defender"
           "Save the Queen"
           "Excalibur"
           "Ragnarok"
           "Chaos Blade"
           "Asura Knife"
           "Koutetsu Knife"
           "Bizen Boat"
           "Murasame"
           "Heaven's Cloud"
           "Kiyomori"
           "Muramasa"
           "Kikuichimoji"
           "Masamune"
           "Chirijiraden"
           "Battle Axe"
           "Giant Axe"
           "Slasher"
           "Rod"
           "Thunder Rod"
           "Flame Rod"
           "Ice Rod"
           "Poison Rod"
           "Wizard Rod"
           "Dragon Rod"
           "Faith Rod"
           "Oak Staff"
           "White Staff"
           "Healing Staff"
           "Rainbow Staff"
           "Wizard Staff"
           "Gold Staff"
           "Mace of Zeus"
           "Sage Staff"
           "Flail"
           "Flame Whip"
           "Morning Star"
           "Scorpion Tail"
           "Romanda Gun"
           "Mythril Gun"
           "Stone Gun"
           "Blaze Gun"
           "Glacier Gun"
           "Blast Gun"
           "Bow Gun"
           "Night Killer"
           "Cross Bow"
           "Poison Bow"
           "Hunting Bow"
           "Gastrafitis"
           "Long Bow"
           "Silver Bow"
           "Ice Bow"
           "Lightning Bow"
           "Windslash Bow"
           "Mythril Bow"
           "Ultimus Bow"
           "Yoichi Bow"
           "Perseus Bow"
           "Ramia Harp"
           "Bloody Strings"
           "Fairy Harp"
           "Battle Dict"
           "Monster Dict"
           "Papyrus Plate"
           "Madlemgen"
           "Javelin"
           "Spear"
           "Mythril Spear"
           "Partisan"
           "Oberisk"
           "Holy Lance"
           "Dragon Whisker"
           "Javelin"
           "Cypress Rod"
           "Battle Bamboo"
           "Musk Rod"
           "Iron Fan"
           "Gokuu Rod"
           "Ivory Rod"
           "Octagon Rod"
           "Whale Whisker"
           "C Bag"
           "FS Bag"
           "P Bag"
           "H Bag"
           "Persia"
           "Cashmere"
           "Ryozan Silk"
           "Shuriken"
           "Magic Shuriken"
           "Yagyu Darkness"
           "Fire Ball"
           "Water Ball"
           "Lightning Ball"
           "Escutcheon"
           "Buckler"
           "Bronze Shield"
           "Round Shield"
           "Mythril Shield"
           "Gold Shield"
           "Ice Shield"
           "Flame Shield"
           "Aegis Shield"
           "Diamond Shield"
           "Platina Shield"
           "Crystal Shield"
           "Genji Shield"
           "Kaiser Plate"
           "Venetian Shield"
           "Escutcheon"
           "Leather Helmet"
           "Bronze Helmet"
           "Iron Helmet"
           "Barbuta"
           "Mythril Helmet"
           "Gold Helmet"
           "Cross Helmet"
           "Diamond Helmet"
           "Platina Helmet"
           "Circlet"
           "Crystal Helmet"
           "Genji Helmet"
           "Grand Helmet"
           "Leather Hat"
           "Feather Hat"
           "Red Hood"
           "Headgear"
           "Triangle Hat"
           "Green Beret"
           "Twist Headband"
           "Holy Miter"
           "Black Hood"
           "Golden Hairpin"
           "Flash Hat"
           "Thief Hat"
           "Cachusha"
           "Barette"
           "Ribbon"
           "Leather Armor"
           "Linen Cuirass"
           "Bronze Armor"
           "Chain Mail"
           "Mythril Armor"
           "Plate Mail"
           "Gold Armor"
           "Diamond Armor"
           "Platina Armor"
           "Carabini Mail"
           "Crystal Mail"
           "Genji Armor"
           "Reflect Mail"
           "Maximillian"
           "Clothes"
           "Leather Outfit"
           "Leather Vest"
           "Chain Vest"
           "Mythril Vest"
           "Adaman Vest"
           "Wizard Outfit"
           "Brigandine"
           "Judo Outfit"
           "Power Sleeve"
           "Earth Clothes"
           "Secret Clothes"
           "Black Costume"
           "Rubber Costume"
           "Linen Robe"
           "Silk Robe"
           "Wizard Robe"
           "Chameleon Robe"
           "White Robe"
           "Black Robe"
           "Light Robe"
           "Robe of Lords"
           "Battle Boots"
           "Spike Shoes"
           "Germinas Boots"
           "Rubber Shoes"
           "Feather Boots"
           "Sprint Shoes"
           "Red Shoes"
           "Power Wrist"
           "Genji Gauntlet"
           "Magic Gauntlet"
           "Bracer"
           "Reflect Ring"
           "Defense Ring"
           "Magic Ring"
           "Cursed Ring"
           "Angel Ring"
           "Diamond Armlet"
           "Jade Armlet"
           "108 Gems"
           "N-Kai Armlet"
           "Defense Armlet"
           "Small Mantle"
           "Leather Mantle"
           "Wizard Mantle"
           "Elf Mantle"
           "Dracula Mantle"
           "Feather Mantle"
           "Vanish Mantle"
           "Chantage"
           "Cherche"
           "Setiemson"
           "Salty Rage"
           "Potion"
           "Hi-Potion"
           "X-Potion"
           "Ether"
           "Hi-Ether"
           "Elixir"
           "Antidote"
           "Eye Drop"
           "Echo Grass"
           "Maiden's Kiss"
           "Soft"
           "Holy Water"
           "Remedy"
           "Phoenix Down"
           "Unknown item 0xFE"
           "Unknown item 0xFF" |]

    let private jobNames =
        [| "None"
           "Squire"
           "Squire"
           "Squire"
           "Squire"
           "Holy Knight"
           "Arc Knight"
           "Squire"
           "Arc Knight"
           "Lune Knight"
           "Duke"
           "Duke"
           "Princess"
           "Holy Swordsman"
           "High Priest"
           "Dragoner"
           "Holy Priest"
           "Dark Knight"
           "Hell Knight"
           "Bishop"
           "Cleric"
           "Astrologist"
           "Engineer"
           "Dark Knight"
           "Cardinal"
           "Heaven Knight"
           "Hell Knight"
           "Arc Knight"
           "Delita's Sis"
           "Arc Duke"
           "Holy Knight"
           "Temple Knight"
           "White Knight"
           "Arc Witch"
           "Engineer"
           "Bi-Count"
           "Divine Knight"
           "Divine Knight"
           "Knight Blade"
           "Sorcerer"
           "White Knight"
           "Heaven Knight"
           "Divine Knight"
           "Engineer"
           "Cleric"
           "Assassin"
           "Assassin"
           "Divine Knight"
           "Cleric"
           "Phony Saint"
           "Soldier"
           "Arc Knight"
           "Holy Knight"
           "Chemist"
           "Priest"
           "Wizard"
           "Oracle"
           "Oracle"
           "Job 0x3A"
           "Job 0x3B"
           "Warlock"
           "Knight"
           "Angel of Death"
           "Archer"
           "Regulator"
           "Holy Angel"
           "Wizard"
           "Impure King"
           "Time Mage"
           "Ghost of Fury"
           "Oracle"
           "Summoner"
           "Holy Dragon"
           "Arch Angel"
           "Squire"
           "Chemist"
           "Knight"
           "Archer"
           "Monk"
           "Priest (White Mage)"
           "Wizard (Black Mage)"
           "Time Mage"
           "Summoner"
           "Thief"
           "Mediator (Orator)"
           "Oracle (Mystic)"
           "Geomancer"
           "Lancer (Dragoon)"
           "Samurai"
           "Ninja"
           "Calculator (Arithmetician)"
           "Bard"
           "Dancer"
           "Mime"
           "Chocobo"
           "Black Chocobo"
           "Red Chocobo"
           "Goblin"
           "Black Goblin"
           "Gobbledeguck (Gabbledegook)"
           "Bomb"
           "Grenade"
           "Explosive (Exploder)"
           "Red Panther"
           "Cuar (Coeurl)"
           "Vampire (Vampire Cat)"
           "Pisco Demon (Piscodaemon)"
           "Squidlarkin (Squidraken)"
           "Mindflare (Mindflayer)"
           "Skeleton"
           "Bone Snatch (Bonesnatch)"
           "Living Bone (Skeletal Fiend)"
           "Ghoul"
           "Gust (Ghast)"
           "Revnant (Revenant)"
           "Flotiball (Floating Eye)"
           "Ahriman"
           "Plague (Plague Horror)"
           "Juravis (Jura Aevis)"
           "Steel Hawk (Steelhawk)"
           "Cocatoris (Cockatrice)"
           "Uribo (Pig)"
           "Porky (Swine)"
           "Wildbow (Wild Boar)"
           "Woodman (Dryad)"
           "Trent"
           "Taiju (Elder Treant)"
           "Bull Demon (Wisenkin)"
           "Minitaurus (Minotaur)"
           "Sacred (Sekhret)"
           "Morbol (Malboro)"
           "Ochu"
           "Great Morbol (Greater Malboro)"
           "Behemoth"
           "King Behemoth (Behemoth King)"
           "Dark Behemoth"
           "Dragon"
           "Blue Dragon"
           "Red Dragon"
           "Hyudra (Hydra)"
           "Hydra (Greater Hydra)"
           "Tiamat"
           "None (Na-shi)"
           "None (Na-shi)"
           "Byblos"
           "Steel Giant (Automaton)"
           "None (Na-shi)"
           "None (Na-shi)"
           "None (Na-shi)"
           "None (Na-shi)"
           "Apanda (Reaver)"
           "Serpentarius"
           "Holy Dragon"
           "Archaic Demon (Archeodaemon)"
           "Ultima Demon (Ultima Daemon)"
           "Job 0x9B"
           "Job 0x9C"
           "Job 0x9D"
           "Job 0x9E"
           "Job 0x9F"
           "PSP ONLY (Dark Knight)"
           "PSP ONLY (Onion Knight)"
           "PSP ONLY (Sky Pirate)"
           "PSP ONLY (Game Hunter)"
           "PSP ONLY (Onion Knight)"
           "PSP ONLY (Deathknight)"
           "PSP ONLY (Templar)"
           "PSP ONLY (Celebrant)"
           "PSP ONLY (Dark Dragon)"
           "Job 0xA9"
           "Job 0xAA"
           "Job 0xAB"
           "Job 0xAC"
           "Job 0xAD"
           "Job 0xAE"
           "Job 0xAF"
           "Job 0xB0"
           "Job 0xB1"
           "Job 0xB2"
           "Job 0xB3"
           "Job 0xB4"
           "Job 0xB5"
           "Job 0xB6"
           "Job 0xB7"
           "Job 0xB8"
           "Job 0xB9"
           "Job 0xBA"
           "Job 0xBB"
           "Job 0xBC"
           "Job 0xBD"
           "Job 0xBE"
           "Job 0xBF"
           "Job 0xC0"
           "Job 0xC1"
           "Job 0xC2"
           "Job 0xC3"
           "Job 0xC4"
           "Job 0xC5"
           "Job 0xC6"
           "Job 0xC7"
           "Job 0xC8"
           "Job 0xC9"
           "Job 0xCA"
           "Job 0xCB"
           "Job 0xCC"
           "Job 0xCD"
           "Job 0xCE"
           "Job 0xCF"
           "Job 0xD0"
           "Job 0xD1"
           "Job 0xD2"
           "Job 0xD3"
           "Job 0xD4"
           "Job 0xD5"
           "Job 0xD6"
           "Job 0xD7"
           "Job 0xD8"
           "Job 0xD9"
           "Job 0xDA"
           "Job 0xDB"
           "Job 0xDC"
           "Job 0xDD"
           "Job 0xDE"
           "Job 0xDF"
           "Job 0xE0"
           "Job 0xE1"
           "Job 0xE2"
           "Job 0xE3"
           "Job 0xE4"
           "Job 0xE5"
           "Job 0xE6"
           "Job 0xE7"
           "Job 0xE8"
           "Job 0xE9"
           "Job 0xEA"
           "Job 0xEB"
           "Job 0xEC"
           "Job 0xED"
           "Job 0xEE"
           "Job 0xEF"
           "Job 0xF0"
           "Job 0xF1"
           "Job 0xF2"
           "Job 0xF3"
           "Job 0xF4"
           "Job 0xF5"
           "Job 0xF6"
           "Job 0xF7"
           "Job 0xF8"
           "Job 0xF9"
           "Job 0xFA"
           "Job 0xFB"
           "Job 0xFC"
           "Job 0xFD"
           "Job 0xFE"
           "Job 0xFF" |]

    let private trimNulls (value: string) = value.TrimEnd('\u0000')

    let private lookup (items: string array) (fallback: string) (index: byte) =
        let i = int index
        if i >= 0 && i < items.Length then items.[i] else fallback

    let private decodeAscii (bytes: byte[]) =
        Encoding.ASCII.GetString(bytes) |> trimNulls

    let private encodeAsciiFixed (length: int) (value: string) =
        if length <= 0 then
            invalidArg "length" "Length must be greater than zero."

        let source = Encoding.ASCII.GetBytes(if isNull value then "" else value)
        let target = Array.zeroCreate<byte> length
        let count = min source.Length length
        Array.blit source 0 target 0 count
        target

    let private quantityText (quantity: byte) =
        if quantity = 0uy then "" else sprintf "x%u" quantity


    // ----- Public formatting functions -----

    /// <summary>Gets the display name for an FFT item ID.</summary>
    /// <param name="itemId">Item ID from save data or a shop/inventory table.</param>
    /// <returns>The item name, or an unknown-item placeholder if the ID is not in the table.</returns>
    let getItemName (itemId: byte) =
        lookup itemNames (sprintf "Unknown item 0x%02X" itemId) itemId

    /// <summary>Gets the display name for an FFT job or monster ID.</summary>
    /// <param name="jobId">Job ID from save data.</param>
    /// <returns>The job or monster name, or an unknown-job placeholder if the ID is not in the table.</returns>
    let getJobName (jobId: byte) =
        lookup jobNames (sprintf "Unknown job 0x%02X" jobId) jobId

    /// <summary>Converts a parsed zodiac value into a user-facing string.</summary>
    /// <param name="zodiac">Parsed zodiac sign from <see cref="FinalFantasyTactics.ZodiacSign"/>.</param>
    /// <returns>A display name for the zodiac sign.</returns>
    let getZodiacName (zodiac: FinalFantasyTactics.ZodiacSign) =
        match zodiac with
        | FinalFantasyTactics.ZodiacSign.Aries -> "Aries"
        | FinalFantasyTactics.ZodiacSign.Taurus -> "Taurus"
        | FinalFantasyTactics.ZodiacSign.Gemini -> "Gemini"
        | FinalFantasyTactics.ZodiacSign.Cancer -> "Cancer"
        | FinalFantasyTactics.ZodiacSign.Leo -> "Leo"
        | FinalFantasyTactics.ZodiacSign.Virgo -> "Virgo"
        | FinalFantasyTactics.ZodiacSign.Libra -> "Libra"
        | FinalFantasyTactics.ZodiacSign.Scorpio -> "Scorpio"
        | FinalFantasyTactics.ZodiacSign.Sagittarius -> "Sagittarius"
        | FinalFantasyTactics.ZodiacSign.Capricorn -> "Capricorn"
        | FinalFantasyTactics.ZodiacSign.Aquarius -> "Aquarius"
        | FinalFantasyTactics.ZodiacSign.Pisces -> "Pisces"
        | FinalFantasyTactics.ZodiacSign.Serpentarius -> "Serpentarius"
        | FinalFantasyTactics.ZodiacSign.UnknownZodiac value -> sprintf "Unknown zodiac 0x%02X" value

    /// <summary>Gets a display name for a unit character-identity byte.</summary>
    /// <param name="identity">The raw character identity value from unit data.</param>
    /// <returns>A human-readable identity label.</returns>
    let getCharacterIdentityName (identity: byte) =
        match identity with
        | 0x80uy -> "Male"
        | 0x81uy -> "Female"
        | 0x82uy -> "Monster"
        | 0x01uy -> "Ramza"
        | 0x04uy -> "Delita"
        | 0x1Euy -> "Agrias"
        | 0x16uy -> "Mustadio"
        | 0x2Auy -> "Meliadoul"
        | 0x29uy -> "Rafa"
        | 0x12uy -> "Malak"
        | 0x22uy -> "Mustadio"
        | 0x32uy -> "Cloud"
        | _ -> sprintf "Character 0x%02X" identity

    /// <summary>Formats the save date tuple for display.</summary>
    /// <param name="month">Save month byte.</param>
    /// <param name="day">Save day byte.</param>
    /// <returns>A simple <c>MM/DD</c> string.</returns>
    let getSaveDateText (month: byte, day: byte) = sprintf "%02u/%02u" month day

    /// <summary>Formats the raw game-time counter for display.</summary>
    /// <param name="gameTime">Game time value from save data.</param>
    /// <returns>The game time rendered as an unsigned decimal string.</returns>
    let getGameTimeText (gameTime: uint32) = sprintf "%u" gameTime

    /// <summary>Decodes the main-character name bytes into a display string.</summary>
    /// <param name="nameBytes">Raw name bytes from <see cref="FinalFantasyTactics.FftSaveData.MainCharName"/>.</param>
    /// <returns>The decoded character name with trailing nulls removed.</returns>
    let getMainCharacterName (nameBytes: byte[]) = decodeAscii nameBytes

    /// <summary>
    /// Encodes a main-character name into a fixed 17-byte ASCII buffer for save editing.
    /// </summary>
    /// <param name="name">Display name to encode. Non-ASCII characters are replaced during ASCII encoding.</param>
    /// <returns>A 17-byte array, truncated when too long and null-padded when shorter.</returns>
    let encodeMainCharacterName (name: string) = encodeAsciiFixed 17 name

    /// <summary>Decodes a unit nickname byte array into a display string.</summary>
    /// <param name="nicknameBytes">Raw nickname bytes from <see cref="FinalFantasyTactics.UnitStats.Nickname"/>.</param>
    /// <returns>The decoded nickname with trailing nulls removed.</returns>
    let getUnitNickname (nicknameBytes: byte[]) = decodeAscii nicknameBytes

    /// <summary>
    /// Encodes a unit nickname into a fixed 16-byte ASCII buffer for save editing.
    /// </summary>
    /// <param name="nickname">Nickname to encode. Non-ASCII characters are replaced during ASCII encoding.</param>
    /// <returns>A 16-byte array, truncated when too long and null-padded when shorter.</returns>
    let encodeUnitNickname (nickname: string) = encodeAsciiFixed 16 nickname

    /// <summary>Formats an item quantity for display.</summary>
    /// <param name="quantity">The raw quantity byte.</param>
    /// <returns>An empty string for zero, otherwise a quantity suffix like <c>x3</c>.</returns>
    let formatQuantity (quantity: byte) = quantityText quantity

    /// <summary>Formats an inventory or shop stack as <c>name quantity</c>.</summary>
    /// <param name="itemId">Item ID byte used as the lookup key.</param>
    /// <param name="quantity">Item quantity byte.</param>
    /// <returns>A display string combining the item name and quantity.</returns>
    let formatItemStack (itemId: byte) (quantity: byte) =
        let name = getItemName itemId
        let amount = quantityText quantity

        if String.IsNullOrEmpty amount then
            name
        else
            sprintf "%s %s" name amount

    /// <summary>Formats a player inventory entry for display.</summary>
    /// <param name="slotIndex">Inventory byte index, which corresponds to the item ID.</param>
    /// <param name="quantity">Item quantity byte.</param>
    /// <returns>A formatted inventory entry string.</returns>
    let formatInventoryItem (slotIndex: byte) (quantity: byte) = formatItemStack slotIndex quantity

    /// <summary>Formats a fur-shop inventory entry for display.</summary>
    /// <param name="slotIndex">Fur-shop byte index, which corresponds to the item ID.</param>
    /// <param name="quantity">Item quantity byte.</param>
    /// <returns>A formatted fur-shop entry string.</returns>
    let formatFurShopItem (slotIndex: byte) (quantity: byte) = formatItemStack slotIndex quantity

    /// <summary>Formats a raw job ID as a display label.</summary>
    /// <param name="jobId">Job ID byte from save data.</param>
    /// <returns>The job name for the supplied ID.</returns>
    let formatJobLabel (jobId: byte) = getJobName jobId

    /// <summary>Builds a short summary string for a unit.</summary>
    /// <param name="unit">The parsed unit record.</param>
    /// <returns>A summary in the form <c>identity - job</c>.</returns>
    let formatUnitSummary (unit: FinalFantasyTactics.UnitStats) =
        let identity = getCharacterIdentityName unit.CharacterIdentity
        let job = getJobName unit.JobId
        sprintf "%s - %s" identity job
