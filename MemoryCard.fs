namespace PSXMCL

open System
open System.IO
open System.Text

/// <summary>
/// Parses PlayStation memory card files (128 KB, 16 blocks of 8 KB each).
/// </summary>
module MemoryCard =

    // ── Constants ──────────────────────────────────────────────────────────────

    /// <summary>Total size of a memory card file in bytes (128 KB).</summary>
    [<Literal>]
    let TotalSize = 131072 // 128 KB

    /// <summary>Size of a single block in bytes (8 KB).</summary>
    /// <remarks>Each block contains 64 frames of 128 bytes each.</remarks>
    [<Literal>]
    let BlockSize = 8192 // 8 KB

    /// <summary>Size of a single frame in bytes (128 bytes).</summary>
    [<Literal>]
    let FrameSize = 128

    /// <summary>Number of blocks on a memory card (16).</summary>
    /// <remarks>
    /// Block 0 is reserved for the directory and broken-sector records;
    /// blocks 1‥15 are used for file data.
    /// </remarks>
    [<Literal>]
    let BlockCount = 16

    /// <summary>Number of frames per block (64).</summary>
    [<Literal>]
    let FramesPerBlock = 64

    // ── Types ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Allocation state of a block, as indicated by the first 4 bytes of each
    /// directory entry (Block 0, Frames 1‥15).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Only <see cref="BlockAllocationState.InUseFirst"/> indicates the start of a valid file;
    /// all other states are either free or invalid.
    /// </para>
    /// <para>
    /// The <c>FreeDeleted</c> cases indicate blocks that were previously used but
    /// deleted. They may still hold residual data and should be treated as free space.
    /// </para>
    /// <para>
    /// The <see cref="BlockAllocationState.Unknown"/> case represents any unrecognized value, which
    /// may indicate corruption or non-standard formatting.
    /// </para>
    /// </remarks>
    type BlockAllocationState =
        /// <summary>First (or only) block of a valid file (<c>0x51</c>).</summary>
        /// <remarks>
        /// Subsequent blocks, if any, are linked via the <c>NextBlock</c> field
        /// in the directory entry.
        /// </remarks>
        | InUseFirst // 0x00000051 – first or only block of a file
        /// <summary>Middle block of a multi-block file (<c>0x52</c>).</summary>
        /// <remarks>
        /// Linked from the previous block and to the next via <c>NextBlock</c>.
        /// Does not carry file metadata (size or name).
        /// </remarks>
        | InUseMiddle // 0x00000052 – middle block
        /// <summary>Last block of a multi-block file (<c>0x53</c>).</summary>
        /// <remarks>
        /// Linked from the previous block; <c>NextBlock</c> is <c>0xFFFF</c>,
        /// indicating the end of the file. Does not carry file metadata.
        /// </remarks>
        | InUseLast // 0x00000053 – last block
        /// <summary>Free block on a freshly formatted card (<c>0xA0</c>).</summary>
        /// <remarks>
        /// Contents are undefined. This is the expected state for all blocks
        /// on a newly formatted memory card.
        /// </remarks>
        | FreeFormatted // 0x000000A0 – freshly formatted
        /// <summary>Free block; was the first (or only) block of a deleted file (<c>0xA1</c>).</summary>
        /// <remarks>May still contain residual data; treat as free space.</remarks>
        | FreeDeletedFirst // 0x000000A1 – deleted (first or only)
        /// <summary>Free block; was a middle block of a deleted file (<c>0xA2</c>).</summary>
        /// <remarks>May still contain residual data; treat as free space.</remarks>
        | FreeDeletedMiddle // 0x000000A2 – deleted (middle)
        /// <summary>Free block; was the last block of a deleted file (<c>0xA3</c>).</summary>
        /// <remarks>May still contain residual data; treat as free space.</remarks>
        | FreeDeletedLast // 0x000000A3 – deleted (last)
        /// <summary>Unrecognized allocation state value.</summary>
        /// <remarks>
        /// May indicate corruption or non-standard formatting. The raw value is
        /// preserved for diagnostic purposes.
        /// </remarks>
        | Unknown of uint32

    /// <summary>
    /// Display mode for a file's icon, as indicated by the first byte of the
    /// title frame (Block N, Frame 0).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Determines how many icon frames follow the title frame and how they
    /// should be displayed.
    /// </para>
    /// <para>
    /// The <see cref="Corrupted"/> case represents any unrecognized flag value.
    /// In such cases it is safest to assume no valid icon frames exist, as
    /// the expected count cannot be determined reliably.
    /// </para>
    /// </remarks>
    type IconDisplayFlag =
        /// <summary>Single static icon — 1 frame (<c>0x11</c>).</summary>
        /// <remarks>
        /// The title frame is followed by exactly 1 icon frame (Block N, Frame 1)
        /// containing the 16×16, 4-bpp bitmap. This is the most common display mode.
        /// </remarks>
        | Static // 0x11 – 1 frame
        /// <summary>Simple 2-frame animated icon (<c>0x12</c>).</summary>
        /// <remarks>
        /// The title frame is followed by 2 icon frames (Block N, Frames 1–2).
        /// The game alternates between them to create a blinking or shimmering effect.
        /// </remarks>
        | TwoFrames // 0x12 – animated, 2 frames
        /// <summary>3-frame animated icon (<c>0x13</c>).</summary>
        /// <remarks>
        /// The title frame is followed by 3 icon frames (Block N, Frames 1–3).
        /// The game cycles through all three for a more elaborate animation.
        /// </remarks>
        | ThreeFrames // 0x13 – animated, 3 frames
        /// <summary>Unrecognized icon display flag value.</summary>
        /// <remarks>
        /// May indicate corruption or non-standard formatting of the title frame.
        /// The raw byte is preserved for diagnostics. Assume no valid icon frames.
        /// </remarks>
        | Corrupted of byte

    /// <summary>
    /// One of the 15 directory entries stored in Block 0, Frames 1‥15.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each entry corresponds to a potential file on the memory card. Only entries
    /// where <c>State</c> = <see cref="BlockAllocationState.InUseFirst"/> are valid
    /// starting points for file parsing.
    /// </para>
    /// <para>
    /// <c>FileSize</c> and <c>FileName</c> are meaningful only for the first block
    /// of a file and should be ignored for middle and last blocks.
    /// </para>
    /// <para>
    /// <c>NextBlock</c> links blocks of a file together. A value of <c>0xFFFF</c>
    /// indicates the last (or only) block.
    /// </para>
    /// </remarks>
    type DirectoryEntry =
        {
            /// <summary>
            /// Allocation state of this block.
            /// </summary>
            State: BlockAllocationState
            /// <summary>
            /// File size in bytes. Meaningful only when
            /// <c>State</c> = <see cref="BlockAllocationState.InUseFirst"/>.
            /// </summary>
            FileSize: uint32
            /// <summary>
            /// Index of the next block in the chain (0‥14 → blocks 1‥15);
            /// <c>0xFFFF</c> = last/only block.
            /// </summary>
            NextBlock: uint16
            /// <summary>
            /// ASCII file name. Meaningful only when
            /// <c>State</c> = <see cref="BlockAllocationState.InUseFirst"/>.
            /// </summary>
            FileName: string
            /// <summary>Indicates whether the directory entry has a valid checksum.</summary>
            IsValid: bool
        }

    /// <summary>
    /// One of the 20 broken-sector records in Block 0, Frames 16‥35.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each record identifies a sector that must not be used for file storage.
    /// The sector number is an absolute address calculated as
    /// <c>Block × 64 + Frame</c>.
    /// </para>
    /// <para>
    /// A <c>SectorNumber</c> of <c>0xFFFFFFFF</c> indicates no (further) broken sectors.
    /// </para>
    /// </remarks>
    type BrokenSector =
        {
            /// <summary>
            /// Absolute sector number (<c>Block × 64 + Frame</c>);
            /// <c>0xFFFFFFFF</c> = none.
            /// </summary>
            SectorNumber: uint32
            /// <summary>Indicates whether the broken-sector record has a valid checksum.</summary>
            IsValid: bool
        }

    /// <summary>
    /// Title frame at Block N, Frame 0 (present in the first block of each file).
    /// </summary>
    type TitleFrame =
        {
            /// <summary>
            /// Display mode for the file's icon, determining how many icon frames follow.
            /// </summary>
            IconDisplayFlag: IconDisplayFlag
            /// <summary>Block number (1‥15) that contains this title frame.</summary>
            /// <remarks>
            /// Always located at Frame 0 of the block. Should match the corresponding
            /// directory entry.
            /// </remarks>
            BlockNumber: byte
            /// <summary>File title, decoded from the 64-byte Shift-JIS field.</summary>
            Title: string
            /// <summary>16-entry, 16-bit CLUT palette used to colorize the icon bitmap.</summary>
            Palette: uint16[]
            /// <summary>Raw bytes of the 64-byte Shift-JIS title field.</summary>
            RawTitle: byte[]
        }

    /// <summary>A single 16×16, 4-bpp icon bitmap (128 bytes).</summary>
    /// <remarks>
    /// <para>
    /// Each byte encodes two pixels: the high nibble is one pixel, the low nibble
    /// is the next. Colors are resolved via the <see cref="TitleFrame.Palette"/>.
    /// </para>
    /// <para>
    /// The number of frames for a file is determined by
    /// <see cref="TitleFrame.IconDisplayFlag"/>:
    /// <see cref="IconDisplayFlag.Static"/> = 1 frame,
    /// <see cref="IconDisplayFlag.TwoFrames"/> = 2 frames,
    /// <see cref="IconDisplayFlag.ThreeFrames"/> = 3 frames.
    /// </para>
    /// </remarks>
    type IconFrame =
        {
            /// <summary>Raw 16×16, 4-bpp icon bitmap (128 bytes).</summary>
            Bitmap: byte[]
        }

    /// <summary>
    /// A parsed save file spanning one or more blocks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Block indices are 1-based. The first block carries the directory entry,
    /// title frame, and icon frames; all subsequent blocks are entirely raw data.
    /// </para>
    /// <para>
    /// <c>Data</c> contains all payload bytes starting after the title and icon
    /// frames in the first block, concatenated with every subsequent block.
    /// </para>o
    /// </remarks>
    type SaveFile =
        {
            /// <summary>1-based block indices occupied by this file, in order.</summary>
            BlockIndices: int[]
            /// <summary>Directory entry from the first block of the file.</summary>
            DirectoryEntry: DirectoryEntry
            /// <summary>Title frame of the file, if present.</summary>
            TitleFrame: TitleFrame option
            /// <summary>Icon frames of the file, if present.</summary>
            IconFrames: IconFrame[]
            /// <summary>Raw payload bytes, excluding title and icon frames.</summary>
            Data: byte[]
        }

    /// <summary>
    /// Represents an entire parsed PlayStation memory card.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The memory card is 128 KB (131 072 bytes), divided into 16 blocks of 8 KB.
    /// Block 0 holds the header, directory, and broken-sector records;
    /// blocks 1‥15 hold file data.
    /// </para>
    /// <para>
    /// <see cref="HeaderValid"/> indicates whether Block 0, Frame 0 starts with
    /// the ASCII characters <c>MC</c> and carries a valid checksum.
    /// </para>
    /// </remarks>
    type MemoryCard =
        {
            /// <summary>
            /// Indicates whether the memory card header (Block 0, Frame 0) is valid.
            /// </summary>
            /// <remarks>
            /// A valid header starts with the ASCII characters <c>MC</c> and carries
            /// a correct checksum. An invalid header may indicate a corrupted or
            /// non-standard memory card image.
            /// </remarks>
            HeaderValid: bool
            /// <summary>15 directory entries (Block 0, Frames 1‥15).</summary>
            Directory: DirectoryEntry[]
            /// <summary>20 broken-sector records (Block 0, Frames 16‥35).</summary>
            BrokenSectors: BrokenSector[]
            /// <summary>Parsed save files present on the memory card.</summary>
            Files: SaveFile[]
            /// <summary>
            /// Original byte array of the memory card, for low-level analysis or debugging.
            /// </summary>
            RawData: byte[]
        }

    // ── Internal helpers ───────────────────────────────────────────────────────

    let private frameSlice (data: byte[]) block frame =
        let offset = block * BlockSize + frame * FrameSize
        data.[offset .. offset + FrameSize - 1]

    let private validateChecksum (frame: byte[]) =
        let computed = Array.fold (^^^) 0uy frame.[0 .. FrameSize - 2]
        computed = frame.[FrameSize - 1]

    let private readU32LE (buf: byte[]) off = BitConverter.ToUInt32(buf, off)
    let private readU16LE (buf: byte[]) off = BitConverter.ToUInt16(buf, off)

    let private readAsciiZ (buf: byte[]) off maxLen =
        let slice = buf.[off .. off + maxLen - 1]
        let len = defaultArg (Array.tryFindIndex ((=) 0uy) slice) maxLen
        Encoding.ASCII.GetString(slice, 0, len)

    let private decodeShiftJisTitle (raw: byte[]) =
        let rec scanLen i =
            if i >= 64 || raw.[i] = 0uy then
                i
            elif
                (raw.[i] >= 0x81uy && raw.[i] <= 0x9Fuy)
                || (raw.[i] >= 0xE0uy && raw.[i] <= 0xFCuy)
            then
                scanLen (i + 2)
            else
                scanLen (i + 1)

        let len = scanLen 0

        try
            Encoding.GetEncoding("shift_jis").GetString(raw, 0, len)
        with _ ->
            Encoding.ASCII.GetString(raw, 0, len)

    let private parseAllocationState (buf: byte[]) off =
        match readU32LE buf off with
        | 0x00000051u -> InUseFirst
        | 0x00000052u -> InUseMiddle
        | 0x00000053u -> InUseLast
        | 0x000000A0u -> FreeFormatted
        | 0x000000A1u -> FreeDeletedFirst
        | 0x000000A2u -> FreeDeletedMiddle
        | 0x000000A3u -> FreeDeletedLast
        | v -> Unknown v

    let private parseDirectoryFrame (frame: byte[]) =
        { State = parseAllocationState frame 0
          FileSize = readU32LE frame 4
          NextBlock = readU16LE frame 8
          FileName = readAsciiZ frame 0x0A 21 // 0Ah‥1Eh = 21 bytes
          IsValid = validateChecksum frame }

    let private parseBrokenSectorFrame (frame: byte[]) =
        { SectorNumber = readU32LE frame 0
          IsValid = validateChecksum frame }

    let private parseIconFlag =
        function
        | 0x11uy -> Static
        | 0x12uy -> TwoFrames
        | 0x13uy -> ThreeFrames
        | b -> Corrupted b

    let private parseTitleFrame (frame: byte[]) =
        let rawTitle = frame.[4..67] // 04h‥43h

        { IconDisplayFlag = parseIconFlag frame.[2]
          BlockNumber = frame.[3]
          Title = decodeShiftJisTitle rawTitle
          Palette = Array.init 16 (fun i -> readU16LE frame (0x60 + i * 2))
          RawTitle = rawTitle }

    let private iconFrameCount =
        function
        | Static -> 1
        | TwoFrames -> 2
        | ThreeFrames -> 3
        | Corrupted _ -> 0

    /// Follow the NextBlock linked list and return all 1-based block indices.
    let private collectBlockChain (dir: DirectoryEntry[]) startIdx =
        let rec follow idx acc depth =
            if depth > BlockCount then
                List.rev acc // guard against cycles
            else
                let acc' = idx :: acc

                match dir.[idx - 1].NextBlock with
                | 0xFFFFus -> List.rev acc'
                | nb -> follow (int nb + 1) acc' (depth + 1)

        follow startIdx [] 0

    let private parseFile (data: byte[]) (dir: DirectoryEntry[]) startBlockIdx =
        let blockChain = collectBlockChain dir startBlockIdx

        let firstBlockFrame = frameSlice data startBlockIdx 0

        let titleFrame =
            if firstBlockFrame.[0] = byte 'S' && firstBlockFrame.[1] = byte 'C' then
                Some(parseTitleFrame firstBlockFrame)
            else
                None

        let numIcons =
            titleFrame
            |> Option.map (fun t -> iconFrameCount t.IconDisplayFlag)
            |> Option.defaultValue 0

        let iconFrames =
            Array.init numIcons (fun i -> { Bitmap = frameSlice data startBlockIdx (i + 1) })

        // Data from first block starts after title + icon frames.
        let firstDataFrame = 1 + numIcons

        let firstBlockData =
            [| for f in firstDataFrame .. FramesPerBlock - 1 do
                   yield! frameSlice data startBlockIdx f |]

        // All subsequent blocks are entirely data.
        let tailData =
            blockChain
            |> List.tail
            |> List.toArray
            |> Array.collect (fun bi ->
                [| for f in 0 .. FramesPerBlock - 1 do
                       yield! frameSlice data bi f |])

        { BlockIndices = Array.ofList blockChain
          DirectoryEntry = dir.[startBlockIdx - 1]
          TitleFrame = titleFrame
          IconFrames = iconFrames
          Data = Array.append firstBlockData tailData }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Parses a memory card from a raw byte array.
    /// </summary>
    /// <param name="data">
    /// The raw byte array representing the memory card.
    /// Must be exactly <see cref="TotalSize"/> bytes (131 072 bytes / 128 KB).
    /// </param>
    /// <returns>A fully parsed <see cref="MemoryCard"/> value.</returns>
    /// <exception cref="System.Exception">
    /// Thrown when <paramref name="data"/> is not exactly <see cref="TotalSize"/> bytes long.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Validates the header, then parses all 15 directory entries,
    /// 20 broken-sector records, and each file whose first directory entry
    /// has <c>State</c> = <see cref="BlockAllocationState.InUseFirst"/>.
    /// </para>
    /// <para>
    /// The original <paramref name="data"/> array is stored verbatim in
    /// <see cref="MemoryCard.RawData"/> for further low-level inspection.
    /// </para>
    /// </remarks>
    let readFromBytes (data: byte[]) =
        if data.Length <> TotalSize then
            failwithf "Invalid memory card size: expected %d bytes, got %d" TotalSize data.Length

        let headerFrame = frameSlice data 0 0

        let headerValid =
            headerFrame.[0] = byte 'M'
            && headerFrame.[1] = byte 'C'
            && validateChecksum headerFrame

        let directory =
            Array.init 15 (fun i -> parseDirectoryFrame (frameSlice data 0 (i + 1)))

        let brokenSectors =
            Array.init 20 (fun i -> parseBrokenSectorFrame (frameSlice data 0 (16 + i)))

        let files =
            directory
            |> Array.mapi (fun i e -> (i + 1, e))
            |> Array.filter (fun (_, e) -> e.State = InUseFirst)
            |> Array.map (fun (bi, _) -> parseFile data directory bi)

        { HeaderValid = headerValid
          Directory = directory
          BrokenSectors = brokenSectors
          Files = files
          RawData = data }

    /// <summary>
    /// Reads and parses a memory card file from disk.
    /// </summary>
    /// <param name="path">Path to the memory card file.</param>
    /// <returns>A fully parsed <see cref="MemoryCard"/> value.</returns>
    /// <remarks>
    /// Reads the file as a raw byte array and delegates to
    /// <see cref="readFromBytes"/>.
    /// </remarks>
    let read (path: string) =
        File.ReadAllBytes(path) |> readFromBytes

    // ── Write helpers ──────────────────────────────────────────────────────────

    /// Writes SaveFile.Data back into the appropriate frames of a raw memory card buffer.
    let private writeDataIntoRaw (buf: byte[]) (saveFile: SaveFile) =
        let firstBlock = saveFile.BlockIndices.[0]
        let firstDataFrame = 1 + saveFile.IconFrames.Length
        let mutable dataIdx = 0

        // First block: only frames after the title + icon frames carry data.
        for f in firstDataFrame .. FramesPerBlock - 1 do
            let frameStart = firstBlock * BlockSize + f * FrameSize

            for b in 0 .. FrameSize - 1 do
                if dataIdx < saveFile.Data.Length then
                    buf.[frameStart + b] <- saveFile.Data.[dataIdx]

                dataIdx <- dataIdx + 1

        // Subsequent blocks are entirely data.
        for bi in saveFile.BlockIndices.[1..] do
            for f in 0 .. FramesPerBlock - 1 do
                let frameStart = bi * BlockSize + f * FrameSize

                for b in 0 .. FrameSize - 1 do
                    if dataIdx < saveFile.Data.Length then
                        buf.[frameStart + b] <- saveFile.Data.[dataIdx]

                    dataIdx <- dataIdx + 1

    /// <summary>
    /// Returns a new <see cref="MemoryCard"/> in which the raw bytes for the given
    /// <paramref name="saveFile"/> have been patched with the save file's current
    /// <see cref="SaveFile.Data"/>.
    /// </summary>
    /// <param name="saveFile">
    /// The save file whose <c>Data</c> should be written back.  The file is
    /// identified by its first block index, which must match one of the files
    /// already stored on the memory card.
    /// </param>
    /// <param name="mc">The memory card to update.</param>
    /// <returns>
    /// A new <see cref="MemoryCard"/> with an updated <see cref="MemoryCard.RawData"/>
    /// and updated <see cref="MemoryCard.Files"/> entry.
    /// </returns>
    /// <remarks>
    /// The title frame, icon frames, directory entries, and broken-sector records
    /// are left unchanged.  Only the data frames covered by
    /// <see cref="SaveFile.Data"/> are patched.
    /// </remarks>
    let withFileData (saveFile: SaveFile) (mc: MemoryCard) : MemoryCard =
        let newRaw = Array.copy mc.RawData
        writeDataIntoRaw newRaw saveFile

        let newFiles =
            mc.Files
            |> Array.map (fun f ->
                if f.BlockIndices.[0] = saveFile.BlockIndices.[0] then
                    saveFile
                else
                    f)

        { mc with
            RawData = newRaw
            Files = newFiles }

    /// <summary>
    /// Returns the memory card as a raw 128 KB byte array.
    /// </summary>
    /// <param name="mc">The memory card to serialise.</param>
    /// <returns>
    /// A copy of <see cref="MemoryCard.RawData"/>, which reflects any patches
    /// applied via <see cref="withFileData"/>.
    /// </returns>
    let writeToBytes (mc: MemoryCard) : byte[] = Array.copy mc.RawData

    /// <summary>
    /// Writes the memory card to a file on disk.
    /// </summary>
    /// <param name="path">Destination file path.</param>
    /// <param name="mc">The memory card to write.</param>
    /// <remarks>
    /// Writes <see cref="MemoryCard.RawData"/> verbatim.  Call
    /// <see cref="withFileData"/> first if you have modified save file data that
    /// needs to be reflected in the output.
    /// </remarks>
    let write (path: string) (mc: MemoryCard) : unit = File.WriteAllBytes(path, mc.RawData)
