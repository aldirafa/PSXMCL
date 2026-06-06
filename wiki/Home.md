Welcome to the **PSXMCL** Wiki! PSXMCL is a lightweight, high-performance .NET library designed for reading, editing, and writing PlayStation (PSX) memory card files. It features a specialized save-data parsing layer specifically built for *Final Fantasy Tactics* (FFT) save slots.

#### Core Capabilities

1. **Low-Level Memory Card I/O (`MemoryCard`)**:
   - Parse and write `.mcd` (and similar raw) PlayStation memory card images (128 KB).
   - Read and reconstruct directory entries, track and register broken sectors, and manage block chain allocations.
   - Extract title frames (with Shift-JIS title decoding) and animated icon frames along with their 16-color CLUT palettes.

2. **FFT Save-Data Layer (`FinalFantasyTactics`)**:
   - Extract and repack character rosters (up to 20 units per save file).
   - Access player inventory and fur-shop stocks, which are represented as 256-byte item-ID lookup arrays.
   - Access and mutate global save properties such as the main character's level, job, save date, world-map position, and play time.

3. **Safe Editing Helpers (`FinalFantasyTacticsEnums`)**:
   - Leverage F#-friendly enums and flags to modify characters securely (e.g., setting equipment slots, reaction/support/movement abilities, palette indices, and gender/party status flags).

4. **Display Presentation Formatting (`FinalFantasyTacticsStrings`)**:
   - Turn raw IDs and bytes into display-ready strings (such as full character names, job/monster names, zodiac sign representations, and formatted unit summaries) perfect for UI development.
