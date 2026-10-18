# 22 - Save and load design (Windows, Steam Auto-Cloud, engine-free state)

Researched 2026-10-18 from Unity manual/script reference, Steamworks docs, Microsoft docs and a few community write-ups. No Unity commands were run, nothing under `Assets/` or `ProjectSettings/` was edited. Tags: **[fetched]** = I read the page; **[search-only]** = seen only in a search summary; **[derived]** = my inference from fetched facts; **UNSOURCED GUESS** = no source. Builds on files 11 (IDs not references, plain C# state) and 13 (domain reload off, statics); it does not repeat them.

## 1. Question

How should BookBoy save and load? Project facts: 3 rooms now, up to 7; about 100 to 120 items per room; an item is data (id plus family/group/number); runtime state is either on a shelf slot (container id, shelf index, slot index) or loose in a baked pile (position and rotation). Campaign state: finds found/installed, rooms tidied, doors open, ability unlocks and upgrade points, run timer and score per zone, per-zone leaderboards later (Steam). Enter Play Mode skip-domain-reload is on. The owner implements it tonight, so the answer must be concrete.

## 2. Findings

### 2.1 Where to save on Windows

- `Application.persistentDataPath` on Windows (Editor and Player) "usually" resolves to `%userprofile%\AppData\LocalLow\<companyname>\<productname>`, via the Windows known-folder API. Both company and product name are in the path, and the page also mentions a GUID derived from the Bundle Identifier on some platforms. Relative paths passed to some APIs resolve against the project folder in the Editor, so always build full paths with `Path.Combine(Application.persistentDataPath, ...)`. [fetched] https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html
- `FOLDERID_LocalAppDataLow` = `%USERPROFILE%\AppData\LocalLow`; `FOLDERID_SavedGames` = `%USERPROFILE%\Saved Games`. [fetched] https://learn.microsoft.com/en-us/windows/win32/shell/knownfolderid
- Microsoft's Xbox/GDK PC FAQ: for titles that manage their own saves, "we recommend saving to the Windows *Saved Games* known folder"; "Don't save game data under the user's *Documents* folder" because OneDrive syncs Documents and "can cause conflicts with cloud save synchronization". Create the folders if missing so players can find and back up saves. [fetched] https://devdocs.xbox.com/build/gdk-and-engines/guides/pc-faq
- Steam Auto-Cloud has a root for each of these: `WinMyDocuments`, `WinAppDataLocal`, `WinAppDataLocalLow`, `WinAppDataRoaming`, `WinSavedGames`, plus `{64BitSteamID}` / `{Steam3AccountID}` placeholders for the subdirectory. [fetched] https://partner.steamgames.com/doc/features/cloud
- A Unity-on-Steam guide: use real save files in `Application.persistentDataPath`; on Windows set root `WinAppDataLocalLow` with subdirectory `<company>/<product>`; give all saves the same extension so one pattern matches; avoid PlayerPrefs with Auto-Cloud because on Windows it lives in the registry. [fetched] https://www.gamedeveloper.com/game-platforms/quick-guide-to-steam-cloud-saves
- `persistentDataPath` called from a MonoBehaviour constructor or instance field initializer throws; use Awake/Start. [search-only] https://discussions.unity.com/t/can-only-be-called-from-the-main-thread/730817 (the specific `get_persistentDataPath is not allowed to be called from a MonoBehaviour constructor` message came from a Japanese forum post in the search results.)
- Inference, not stated by any source: if the company or product name in Player Settings changes after release the folder changes, so the Auto-Cloud subdirectory would no longer match. Unity's own default company is "DefaultCompany" (seen in a forum path). [derived] https://discussions.unity.com/t/path-of-application-persistentdatapath-on-linux/650027

### 2.2 JSON vs binary; which JSON library

- `JsonUtility`: fields only; `[Serializable]` plain classes/structs; no `Dictionary<>`; no top-level primitives or arrays (wrap them); missing JSON fields keep the constructed defaults; unknown JSON fields are ignored; "significantly faster than popular .NET JSON solutions"; callable from a background thread; `ToJson(obj, true)` pretty-prints. [fetched] https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/code-reload-serialization/script-serialization/json-serialization.md
- `FromJson` returns null for null or empty input; if the type has a parameterless constructor it runs (so field initializers supply defaults for missing fields). The page documents no exceptions. [fetched] https://docs.unity3d.com/6000.6/Documentation/ScriptReference/JsonUtility.FromJson.html
- Malformed (non-empty) JSON throws (`ArgumentException: JSON parse error`) per forum reports; one Unity reply called the throw a safety signal of broken input. Anecdotal. [search-only] https://discussions.unity.com/t/jsonutility-fromjson-error-invalid-value/635192 and https://discussions.unity.com/t/jsonutility-fromjson-throwing-errors/923116
- Unity serialization rules: no nested containers or dictionaries; custom classes cannot be null (replaced by default objects) unless `[SerializeReference]`. [fetched via file 11] https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/code-reload-serialization/script-serialization/rules.md
- Newtonsoft: package `com.unity.nuget.newtonsoft-json`, docs version 3.2.2, "corresponds to Newtonsoft.Json version 13.0.2". The page says nothing on IL2CPP/AOT or Unity types. [fetched] https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@3.2/manual/index.html . **It is not in this project's `Packages/manifest.json`** (grep found only `com.unity.test-framework`), so adding it is a manifest change that CLAUDE.md says needs approval. [fetched, local]
- Newtonsoft and `Vector3`/`Quaternion`/`Color`: serializing them directly loops through the `normalized` property; the usual fix is a plain DTO of floats. [search-only] https://discussions.unity.com/t/attmepting-to-serialize-my-scriptableobject-with-newtonsoft-json-returns-a-self-referencing-loop-error/1554002 . Same conclusion in either library: save plain numbers, not Unity structs.
- `System.Text.Json` in Unity: community reports from the 2021.2 era say it needs several dependency DLLs; no official Unity doc found for Unity 6. [search-only] https://discussions.unity.com/t/systen-text-json-net-standard-2-0/868255 . Not recommended.
- Binary: no source found that recommends it for a save this small. UNSOURCED GUESS on size: 120 loose items at roughly 100 bytes of JSON each is about 12 KB per room, under 100 KB for 7 rooms. Steam's only hard figure I found is 100 MB per single write call. [fetched] https://partner.steamgames.com/doc/features/cloud

### 2.3 Schema versioning and migrations

- Meta's save guidance: version the schema, increment "when *anything* changes in the save data structure"; on mismatch migrate (recommended) or discard; migrate one version step at a time and chain; if a step is missing the data must be discarded; defer loading until the opening screen so problems can be reported. [fetched] https://developers.meta.com/horizon/documentation/unity/ps-save-game-best-practices/
- "Put a schema version in every save payload before you have any players"; chain small ordered functions; migrate on read; keep the raw payload until the upgraded one is written; test by loading a save from your oldest shipped build. [fetched] https://crux.supercraft.host/blog/player-data-save-migrations-every-patch/
- Because `JsonUtility` ignores unknown fields and defaults missing ones (see 2.2), additive changes are free; an older build that loads and re-saves a newer file would silently drop the new fields. [derived from the JsonUtility page]

### 2.4 Atomic writes, backups, corruption

- `File.Replace(source, dest, backup)` replaces the destination's contents with the source, creates a backup of the replaced file, throws `FileNotFoundException` if source or destination is missing, requires the files to be on the same volume (a backup on another volume is deleted), and if the backup file exists it is overwritten. Pass null for no backup. [fetched] https://learn.microsoft.com/en-us/dotnet/api/system.io.file.replace
- The `File.Replace` page does not say "atomic". The Win32 `ReplaceFileW` page lists failure states instead: `ERROR_UNABLE_TO_MOVE_REPLACEMENT` (1176): with a backup specified "the replaced and replacement files retain their original file names"; `..._2` (1177): the replacement still exists under its own name but the original has been renamed (to the backup name if given); `ERROR_UNABLE_TO_REMOVE_REPLACED` (1175): both keep their names. All three files must be on one volume. [fetched] https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-replacefilew . Claims that ReplaceFile is atomic come from third parties (a Zed commit, a PostgreSQL thread) and stay unconfirmed by Microsoft. [search-only] https://antonymale.co.uk/windows-atomic-file-writes.html
- `File.Move(src, dst, overwrite)` exists only for .NET Core 3.0+ / .NET 5+ per the page's monikers, not .NET Standard 2.1; the two-argument `Move` throws `IOException` if the destination exists. The documented fallback "Copy with overwrite then Delete" is explicitly "not atomic". [fetched] https://learn.microsoft.com/en-us/dotnet/api/system.io.file.move . Whether Unity 6.6's runtime exposes the overload was not checked. [derived: do not rely on it]
- `FileStream.Flush()` flushes the OS I/O buffer too; `Flush(true)` ("flushToDisk") also clears all intermediate file buffers. [fetched] https://learn.microsoft.com/en-us/dotnet/api/system.io.filestream.flush
- The recurring failure being guarded against: a forced shutdown leaving a zero-length or truncated file because data was not flushed. [fetched] https://antonymale.co.uk/windows-atomic-file-writes.html
- Deleting the target before renaming the new file is criticised because a crash in between loses both. [search-only] https://hackorum.dev/topics/37779
- Meta: store a checksum, and on integrity failure "load the previous file, remove the bad save, and notify the player"; keep at least two alternating files; "Don't delete a slot's previous file until the new file is fully written"; prevent overlapping autosaves with a semaphore/queue. [fetched] https://developers.meta.com/horizon/documentation/unity/ps-save-game-best-practices/

### 2.5 Autosave timing and quitting

- `Application.quitting` fires when the Player quits and the quit "cannot be cancelled"; it is not raised if the Player is forced to quit or crashes; also fires when leaving Play mode in the Editor. It is a **static event**; the page never mentions unsubscribing. [fetched] https://docs.unity3d.com/ScriptReference/Application-quitting.html
- `Application.wantsToQuit` can cancel a quit, but only as a request; "not guaranteed to quit"; return value is ignored when leaving Play mode in the Editor. [fetched] https://docs.unity3d.com/ScriptReference/Application-wantsToQuit.html
- `OnApplicationQuit`: the page warns not to rely on it on mobile; says nothing about killing the process on Windows. [fetched] https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnApplicationQuit.html
- File 13: with domain reload off, static events keep subscribers across Play sessions, and Unity says to unregister static handlers on exit. So `Application.quitting += ...` must be paired with `-=` in `OnDisable`. [fetched, from 13] https://docs.unity.com/en-us/engine/6000.6/manual/programming-environment/code-reload-serialization/code-reload/code-reloading-editor/domain-reloading.md
- Saving at level start is easier to restore than mid-level state (Meta). [fetched] https://developers.meta.com/horizon/documentation/unity/ps-save-game-best-practices/
- "Not mid-throw": no source addresses physics-in-flight saves. [derived] A book in flight has no stable "container or pile" state, so gate the write on a settled condition (see section 5).

### 2.6 Restoring baked piles: positions vs a seed

- `Random.InitState(seed)` reproduces a sequence "on demand", and Unity suggests storing a level as a single integer seed; but the page says nothing about determinism across Unity versions or platforms. [fetched] https://docs.unity3d.com/ScriptReference/Random.InitState.html
- `System.Random(seed)`: same seed gives the same series "for replaying games", but "the example may produce different sequences of random numbers if run on different versions of .NET". Persisting the generated values themselves, or writing your own PRNG, avoids the dependency (the page's inference, not an explicit recommendation). [fetched] https://learn.microsoft.com/en-us/dotnet/api/system.random
- BookBoy's `PilePlanner.Plan(..., seed)` uses `System.Random(seed)` and only plans pile **centres and item-to-pile assignment**; the actual item poses in a pile are produced by the builder/scene. [fetched, local: `Assets/_Project/Scripts/Rules/PilePlanner.cs`] File 13 says Unity is moving to CoreCLR, which is the "different .NET version" case above. [fetched, from 13]
- Net: a seed alone cannot reproduce exact poses; positions can. [derived]

### 2.7 Loading into a scene with inactive rooms

- `FindObjectsByType` does not include inactive objects unless you pass `FindObjectsInactive.Include`; the page does not say how it treats an active child under an inactive parent ("check the Manual or test this directly"). [fetched] https://docs.unity3d.com/ScriptReference/Object.FindObjectsByType.html
- `Instantiate` runs `Awake`/`OnEnable` only if the object is active in the hierarchy. [fetched, from 11] https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Object.Instantiate.html
- `GetInstanceID` is not reliable across sessions. [fetched, from 11] https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/object/getinstanceid.md
- Derived: build the `itemId -> view` map from serialized references or a one-time `FindObjectsByType<Book>(FindObjectsInactive.Include)` at scene start, not per restore and not via `GameObject.Find`. Apply state to inactive rooms' views directly (set transform, slot, parent) without needing them active.

### 2.8 Steam Auto-Cloud: what it needs, syncs, quotas, conflicts

- Prerequisite: set "Byte quota per user" and "Number of files allowed per user" on the Steam Cloud Settings page and publish, then configure Auto-Cloud. Both are per user, per game; no defaults are listed. [fetched] https://partner.steamgames.com/doc/features/cloud
- Auto-Cloud entry = Root + Subdirectory (relative, `.` for none) + Pattern (`*` wildcard) + OS restriction (Cross-Platform default and "strongly" recommended) + Recursive flag. Files synced under a specific OS restriction "won't sync properly to other platforms later". Steam creates `steam_autocloud.vdf` in each configured location. [fetched] same URL.
- When: Steam syncs at app launch and exit; changed matching files upload afterward. Best practice from the page: keep files small, split saves into separate files by how often they change, keep machine-specific settings (video) out of synced saves. [fetched] same URL.
- Dynamic Cloud Sync (Steam Deck suspend example): Steam notifies the game that local files changed; the game must handle `RemoteStorageLocalFileChange_t` and reload from disk; wrap writes in `BeginFileWriteBatch`/`EndFileWriteBatch`. Needs the Steamworks API in the game. [fetched] same URL; API page: https://partner.steamgames.com/doc/api/ISteamRemoteStorage
- Limits: 100 MB for one `FileWrite` call; 256 MB may pick a non-optimal storage endpoint; Cloud file path max 260 characters; names must be valid on all OSes. [fetched] same URLs.
- **Conflict handling is not described in the Steamworks docs** (neither cloud page nor ISteamRemoteStorage page). Third-party/support pages describe a user-facing dialog offering "download cloud files" or "upload local files" with timestamps, and warn against choosing by timestamp alone. [search-only] https://help.steampowered.com/faqs/view/68D2-35AB-09A9-7678 (the page body did not load for me, only the search summary) and https://evezone.evetech.co.za/daily-drop/steam-cloud-conflict-save-selection-guide
- Pitfalls: changing the save path does not remove old files from the cloud and they can fill the file quota so new saves fail to upload; demos need the "shared cloud app ID"; per-Steam-user folder via `{64BitSteamID}` needs the Steam ID at runtime. [fetched] https://www.gamedeveloper.com/game-platforms/quick-guide-to-steam-cloud-saves
- Local testing: `testappcloudpaths <AppId>`, `set_spew_level 4 4`, log at `%Steam Install%\logs\cloud_log.txt`; publish changes then wait up to 10 minutes or restart Steam; enable cloud for developers only first. [fetched] https://partner.steamgames.com/doc/features/cloud

### 2.9 Tamper resistance and leaderboards

- Community consensus: local save editing cannot be fully prevented; encryption is security by obscurity; a checksum plus sanity checks ("fact check" the save) is the cheap option; heavy protection is "normally not worth the effort" for single-player. Forum-level evidence only. [search-only] https://forum.unity.com/threads/encrypt-savegames.582292/ , https://forum.gdevelop.io/t/protect-against-write-read-an-external-save-file/31933
- Steam leaderboards: up to 10,000 per title, one entry per player per board, int32 score plus up to 64 int32 details; `KeepBest` or `ForceUpdate`; the web "Writes" setting set to **Trusted** stops clients setting scores (only the Web API can). The page says nothing about Steam validating scores. [fetched] https://partner.steamgames.com/doc/features/leaderboards

## 3. Numbers and rules of thumb

- Save size estimate: about 12 KB of JSON per visited room, under 100 KB total at 7 rooms (UNSOURCED GUESS, check by printing `json.Length`). Steam single-write limit 100 MB, so size is irrelevant to the limits.
- Quota to configure in Steamworks: byte quota 10 to 50 MB, file count 10 to 20 (the Unity-on-Steam guide used 30 to 50 MB / "10" for a roughly 10 MB save). [fetched, guide above] Choose generously; old files from path changes also count.
- Backups: keep exactly one `.bak` (the file that `File.Replace` produces) plus one `.pre-vN.bak` per migration.
- Autosave throttle: debounce about 2 s after a trigger, hard minimum 5 s between writes, plus a 90 s timer while dirty. UNSOURCED GUESS, tune by feel.
- Serialization budget: UNSOURCED GUESS that `JsonUtility.ToJson` for about 360 small DTOs takes a few milliseconds on a PC; measure once with the Profiler before moving the write to a thread.
- Write order: temp file in the same directory, `Flush(true)`, then `File.Replace(tmp, main, bak)` when main exists, else `File.Move(tmp, main)`.
- Load order: main, then `.bak`, else new game. Refuse (do not overwrite) a file whose `schema` is higher than the build supports.
- Floats: round positions to 3 decimals (1 mm) and rotations to 4 decimals when building the DTO. UNSOURCED GUESS that this is accurate enough for books on shelves and piles.

## 4. Confidence and unknowns

- High: persistentDataPath on Windows; Auto-Cloud root/subdirectory/pattern model and sync timing; JsonUtility rules; `File.Replace` semantics and failure states; `Application.quitting` not guaranteed on crash or kill; leaderboard Trusted writes.
- Medium: that `File.Replace` is crash-atomic on NTFS (Microsoft docs do not state it; third parties say yes); Meta and supercraft guidance is platform/server flavoured but the pattern is general.
- Low / not found: how Steam Auto-Cloud resolves conflicts programmatically (not documented in Steamworks pages I read; the dialog description is third-party); whether the `*.sav` pattern excludes `slot1.sav.bak` (a `*` wildcard mask is documented but exact matching rules are not) [derived: it should]; `JsonUtility` handling of enums (UNSOURCED GUESS: stored as integers, so avoid enums in DTOs); exact `JsonUtility` float formatting and nesting depth limit (not checked); whether `FindObjectsByType(Include)` finds active children under an inactive room parent (page says test it); whether `File.Move(…, overwrite)` is available in Unity 6.6's runtime; timing of JSON for this size.
- Design question for Kolby, not research: does "New Game" erase per-zone best times, or only campaign progress? Does resuming a saved mid-run timer disqualify the run from the leaderboard (save-scum risk)?

## 5. Implication for BookBoy (recommended design, smallest sensible scope)

### 5.1 Decisions

1. **Location:** `Application.persistentDataPath/saves/` (resolves to `...\LocalLow\<Company>\<Product>\saves\`). Not Documents (OneDrive conflicts), not PlayerPrefs (registry). Before the first save, **set Company and Product name in Player Settings to their final values** (`DefaultCompany` is the default) and never change them; changing `ProjectSettings` is a guardrail item, so ask first. Saved Games (`%USERPROFILE%\Saved Games`) is Microsoft's preferred spot and Steam has a `WinSavedGames` root, but Unity has no property for it (needs `SHGetKnownFolderPath` P/Invoke), so skip unless Kolby wants it.
2. **Format:** JSON through `JsonUtility`. No new package, no manifest change, works from Edit Mode tests, human-readable for debugging. DTOs only hold ints, floats, strings, bools, lists of `[Serializable]` classes. No dictionaries, no enums (use strings), no Unity structs (floats), no references.
3. **One save per playthrough, file name takes a slot argument:** `slot1.sav`. API is `Write(slotId)`/`Read(slotId)` so adding slots later costs nothing, but build no slot UI. Settings (mouse, video) go in `settings.json` **outside** `saves/` so Auto-Cloud never syncs machine-specific data.
4. **Atomic write + one backup:** write `slot1.sav.tmp` with `FileStream(FileMode.Create, FileShare.None)`, `Flush(true)`, close; if `slot1.sav` exists call `File.Replace(tmp, main, main + ".bak")`, else `File.Move(tmp, main)`. Catch `IOException`/`UnauthorizedAccessException`, retry up to 3 times with a short wait (a virus scanner or the Steam client may hold the file; a similar failure is reported for `MoveFileEx` [search-only]), then log and carry on; a failed save must never crash or block play. On startup delete a leftover `.tmp`.
5. **Corruption handling (a result enum, not exceptions):** `Missing` (new game), `Ok`, `RecoveredFromBackup`, `TooNew` (file schema > build: do not load, do not overwrite, tell the player to update), `Unreadable` (both main and `.bak` bad: rename main to `slot1.sav.corrupt` once, start new, show a message). A load is "bad" when the file is empty, `FromJson` throws or returns null, `schema` is missing/zero, or validation fails. **No checksum tonight**: truncation is already caught by parse failure, and the temp-then-replace plus `.bak` covers the realistic failures; add a `sha256` of the payload only if Kolby wants bit-rot detection (Meta recommends one; optional here).
6. **Versioning:** integer `schema` at the top (starts at 1) plus `gameVersion` string for diagnostics. Migration is plain C# on typed DTOs: probe with a tiny `SchemaProbe { int schema; }` (unknown fields ignored), then `FromJson` into the frozen `SaveFileV{n}` class, then `V1ToV2(...)` and so on. Keep old DTO classes and one fixture JSON per shipped schema in the test project. Additive changes (new field with a default) need no bump. Before the first migration of a file, copy it to `slot1.sav.pre-v{n}.bak`.
7. **Save IDs, never references:** everything is a stable string id: item id (derive from room/family/group/number, **not** from a build-time counter, so rebuilding Lamplit does not renumber), container id (`r1-bc03`), find id, upgrade node id, room number, shelf index, slot index. Drop (with a log line, not an error) any id the current catalog no longer knows. `CampaignCatalog.FindById` returning null already makes `MarkFound` safe for that.
8. **Piles: store poses, not a seed.** For each visited room save every item that is not on a shelf as `{id, position, rotation}` in **room-local** coordinates (so moving a room root does not break saves), rounded as in section 3. Also save `layoutVersion` per room (an integer the builder bumps whenever it changes shelf counts or bake positions); if the saved `layoutVersion` differs from the scene's, discard that room's item list only (the room restores to its fresh baked state) and keep campaign progress. Never-visited rooms save no items and restore from the baked scene. Why not a seed: `PilePlanner` uses `System.Random`, whose sequences "may differ on different versions of .NET", and the seed only gives pile centres, not item poses.
9. **Derived state is not saved.** Doors: `CampaignState.IsDoorOpen(room)` is "room 1, or previous room finished" (plus admin-forced overrides); save the finished-room flags, not door flags. Do not save the admin `_forced` doors. Upgrades granted by installed finds are re-applied by `CampaignState.ApplyTo(UpgradeState)`; save purchased/unlocked node ids and points anyway (simple, idempotent, `Grant` ignores duplicates).
10. **Awarded shelves must be saved**, or loading re-awards the per-shelf upgrade points: `FinishedTracker` awards one point the first time a shelf becomes finished. Save its awarded shelf indices (add a read accessor and a restore method) and restore them **before** any shelf-finished detection runs. Restore with an `isRestoring` flag that suppresses fanfare, sounds, score and event callbacks, and make `FinishRoom` calls on restore silent.
11. **Autosave triggers** (all just call `SaveService.RequestSave(reason)`, which sets a dirty flag): room tidied (`FinishRoom` returned true), find installed, upgrade unlocked, first entry into a room, pause menu opened, and `Application.quitting`; plus a 90 s timer while dirty. No per-placement saves.
12. **Settled-gate (not mid-throw/mid-physics):** the write happens only when `ThrownInFlightCount == 0`, no pickup/place animation is running, and no room transition is in progress; otherwise the request stays queued and is retried each frame, still debounced and single-flight. Quit is the exception (it must write now): a book still in flight is saved as loose at its current pose; carried books are saved as ids in `carried`.
13. **Timer/score:** save `runElapsedSeconds`, `runScore`, `runActive` per zone and best values. Write a `resumed` flag when a run continues from a save; leaderboards can decide later whether to accept resumed runs.
14. **Steam Auto-Cloud (config for Kolby, nothing for code tonight):** Root `WinAppDataLocalLow`, Subdirectory `<Company>/<Product>/saves`, Pattern `*.sav`, Cross-Platform OS setting, not recursive. Quota 20 MB / 20 files, published, with "developers only" on until tested (`testappcloudpaths <AppId>`). `.tmp`, `.bak`, `.corrupt` do not end in `.sav`, so they are not matched [derived]. Do not integrate `ISteamRemoteStorage` for v1; add `BeginFileWriteBatch`/`EndFileWriteBatch` and the change callback only when Steamworks is added and Steam Deck matters. Conflict dialog is Steam's, not ours: make the file self-describing (`savedUtc`, `playSeconds`, `schema`) so a human can tell which is newer, and rely on the `TooNew` guard when one machine has an older build.
15. **Tamper resistance: none for the file.** Cheap sanity checks on load instead (ids exist, shelf/slot indices in range, points non-negative, an installed find was also found, a finished room's predecessor is finished). When Steam leaderboards arrive, set the board's Writes to Trusted if cheating matters, or accept honour-system scores; Steam documents no score validation.
16. **Domain/scene reload off:** no static state in the save code. `SaveService` is a scene MonoBehaviour found by serialized reference; it subscribes to `Application.quitting` in `OnEnable` and unsubscribes in `OnDisable` (static event, see file 13). It reads the save in `Start` on every Play and must reset all plain-C# states first (`CampaignState.Reset()`, `UpgradeState.Reset()`, `FinishedTracker.Reset()`), because with scene reload off, non-serialized fields from the last Play can still be alive. In the Editor use `slot_dev.sav` (`#if UNITY_EDITOR`) so playtesting never touches a real save.

### 5.2 Class split

Plain C#, no UnityEngine, in `BookBoy.Rules` (already `noEngineReferences`):
- `SaveFileDto` (+ `ProgressDto`, `RoomDto`, `ItemOnShelfDto`, `LooseItemDto`, `ZoneRunDto`, all `[System.Serializable]` public fields) - the shape of the JSON.
- `SaveSnapshot.Capture(CampaignState, UpgradeState, FinishedTracker, IReadOnlyList<RoomState>, run data)` and `SaveSnapshot.Restore(dto, ...)` - copies between live plain state and DTO; needs small additions to `CampaignState` (`FoundIds`, `InstalledIds`, `FinishedRooms` accessors) and `UpgradeState` (`UnlockedIds`, `Restore(points, totalEarned, ids)`).
- `RoomState` (per room: `layoutVersion`, `visited`, list of `ItemState` = id plus `OnShelf(container, shelf, slot)` or `Loose(pos, rot)`) - this is the file 11 state layer; it is also what a room keeps when unloaded.
- `SaveMigrations` (`V1ToV2` ...), `SaveValidator` (returns a list of problems and repaired DTO), `SaveFileIO` (System.IO only: `TryWrite(path, text)`, `ReadWithFallback(path, parse)`, takes the directory as a parameter so tests use a temp folder).

Engine-aware, new asmdef `BookBoy.Persistence` (references `BookBoy.Rules` and UnityEngine):
- `SaveJson` - thin wrapper over `JsonUtility.ToJson/FromJson`, `SchemaProbe`, and the migration dispatcher (it is the only place that calls JsonUtility, so swapping libraries later touches one file). Edit Mode tests can still call it because they run inside the Editor.
- `SaveService : MonoBehaviour` - paths, `RequestSave`, debounce/timer/settled gate, quit hook, load-on-Start, result enum to UI.
- `RoomSaveBinder` (one per room or one shared) - builds `RoomState` from the live `Book` views and applies a `RoomState` to views by id via the registry; sets transforms, parents, slot occupancy, zero velocities, keeps idle books in their idle (trigger/no-Rigidbody) state per file 11/15.

### 5.3 Recommended file layout

```
Assets/_Project/Scripts/Rules/Save/
    SaveDto.cs            // DTO classes only
    SaveSnapshot.cs       // Capture/Restore (plain C#)
    SaveMigrations.cs
    SaveValidator.cs
    SaveFileIO.cs         // tmp + Replace + .bak + fallback, System.IO only
    RoomState.cs          // ItemState, RoomState (file 11 state layer)
Assets/_Project/Scripts/Persistence/
    BookBoy.Persistence.asmdef
    SaveJson.cs
    SaveService.cs
    RoomSaveBinder.cs
Assets/_Project/Tests/EditMode/Save/
    SaveRoundTripTests.cs, SaveMigrationTests.cs, SaveFileIOTests.cs, SaveValidatorTests.cs
    Fixtures/save_v1_min.json, save_v1_full.json, save_corrupt_truncated.json
(runtime, per player)  %USERPROFILE%\AppData\LocalLow\<Company>\<Product>\
    saves\slot1.sav            // Steam Auto-Cloud syncs this (*.sav)
    saves\slot1.sav.bak        // previous good save (not synced)
    saves\slot1.sav.pre-v1.bak // only after a migration
    saves\slot1.sav.corrupt    // only after an unrecoverable load
    settings.json              // not synced
```

New asmdef and any asmdef reference lists are build-system additions; per CLAUDE.md present the file list first (this document is that plan).

### 5.4 Sample save JSON (schema 1)

```json
{
  "schema": 1,
  "gameVersion": "0.3.0",
  "savedUtc": "2026-10-18T21:04:11Z",
  "playSeconds": 5321.4,
  "progress": {
    "found": ["apron", "shoes", "glasses"],
    "installed": ["apron", "shoes"],
    "finishedRooms": [1],
    "unlockedNodes": ["arms1", "arms2", "feet1"],
    "points": 3,
    "totalEarned": 9,
    "carried": ["r2-g05-n03"]
  },
  "zones": [
    { "room": 1, "bestSeconds": 412.8, "bestScore": 9120, "runActive": false, "runElapsed": 0.0, "runScore": 0, "resumed": false },
    { "room": 2, "bestSeconds": 0.0, "bestScore": 0, "runActive": true, "runElapsed": 188.2, "runScore": 2100, "resumed": true }
  ],
  "rooms": [
    {
      "room": 1, "visited": true, "layoutVersion": 3,
      "awardedShelves": [0, 1, 2, 3, 4, 5, 6, 7],
      "shelved": [
        { "item": "r1-g01-n01", "container": "r1-bc01", "shelf": 0, "slot": 0 },
        { "item": "r1-g01-n02", "container": "r1-bc01", "shelf": 0, "slot": 1 }
      ],
      "loose": []
    },
    {
      "room": 2, "visited": true, "layoutVersion": 1,
      "awardedShelves": [0],
      "shelved": [
        { "item": "r2-g03-n01", "container": "r2-bc02", "shelf": 1, "slot": 0 }
      ],
      "loose": [
        { "item": "r2-g05-n01", "px": 2.314, "py": 0.062, "pz": -1.808, "qx": 0.0, "qy": 0.7071, "qz": 0.0, "qw": 0.7071 }
      ]
    },
    { "room": 3, "visited": false, "layoutVersion": 0, "awardedShelves": [], "shelved": [], "loose": [] }
  ]
}
```

(Positions are room-local. Doors are not stored; they follow from `finishedRooms`. Field names are illustrative; the DTO classes decide the final names.)

### 5.5 Step-by-step build and test plan (Edit Mode first, then the scene)

Build order, each step small and testable:
1. DTOs + `SaveSnapshot.Capture/Restore` for **progress only** (found, installed, finishedRooms, upgrades, points, awardedShelves). Test: build a `CampaignState` with several finds installed and rooms finished, capture, `JsonUtility.ToJson`, `FromJson`, restore into fresh objects, assert equal including `IsDoorOpen(2)` true and `IsDoorOpen(3)` false, and that `ApplyTo` grants the same nodes.
2. `SaveFileIO` with a temp directory (`Path.GetTempPath()` + GUID). Tests: first write creates main and no `.bak`; second write creates `.bak` equal to the first content; `.tmp` is gone after success; empty main falls back to `.bak`; garbage main falls back to `.bak`; both bad returns `Unreadable`; main missing but `.bak` present loads the backup; a read-only/locked main (open a `FileStream` with `FileShare.None`) makes the write fail softly after retries and leaves the old file intact.
3. Corruption cases from fixtures: truncated JSON, zero bytes, JSON `{}` (schema 0), wrong types. All must return a result, never throw.
4. Version guard: fixture with `schema: 999` returns `TooNew` and a following `Write` call is refused (file unchanged on disk).
5. Migration scaffold: with only schema 1 existing, add a test-only `V0ToV1` fixture to prove the chain runs, then the "missing step" case returns `Unreadable` with the dead-end version in the log.
6. `RoomState` capture/restore for **items** using fake plain objects: shelved and loose items round-trip; unknown item id is dropped with a warning; shelf/slot out of range is dropped; `layoutVersion` mismatch discards only that room's items; unvisited room round-trips empty.
7. Re-award test: restore a state with awarded shelves, run `FinishedTracker.Update` with those shelves finished, assert 0 newly awarded points.
8. `SaveService` glue in the scene with a **dev slot**: add a debug key/menu to Save and Load. Manual checks via Play Mode: tidy part of room 1, save, stop, Play, load, confirm shelves/loose poses; confirm rooms 2 and 3 (inactive) restore their loose items before first activation; check no fanfare/sound on load.
9. Gate test: throw a book and request a save the same frame; assert the write waits until the book settles (log the delay); quit while a book is in flight and confirm it reloads as loose.
10. Domain/scene reload off check: Play, load, stop, Play again without recompiling; state must come from the file, not leftovers (reset-then-load order). Do one Play with "Reload Scene" on once for comparison, only with approval to change the setting.
11. Editor-side file check (not Unity APIs): with the game closed, inspect `saves\` in LocalLow for `slot_dev.sav` and `.bak`; open the JSON and confirm it is readable and under about 100 KB.
12. Steam (later, after a Steam build exists): configure Auto-Cloud as in 5.1 item 14, run `testappcloudpaths <AppId>`, quit the game, check `cloud_log.txt`, repeat on a second PC; also test deliberately making the two PCs diverge to see the conflict dialog.

### 5.6 Common pitfalls (checklist)

- Writing only `Application.persistentDataPath` before company/product names are final: the folder name changes and Auto-Cloud misses it.
- Putting saves under Documents (OneDrive) or in PlayerPrefs (registry, not synced).
- Truncating the target directly (`File.WriteAllText(main, ...)`): a crash leaves a zero-byte save.
- Assuming `File.Replace` works for the first save (it throws `FileNotFoundException` if the destination is missing), or that `.tmp` and the target are on different drives.
- Saving enums, Unity structs, dictionaries, or `UnityEngine.Object` references; null nested classes silently become default objects.
- A newer-build save loaded by an older build and then re-saved: unknown fields are dropped. Guard with the `TooNew` refusal.
- Restoring by replaying game events (re-awarding points, re-triggering "room tidied" fanfare, re-opening doors with animation).
- Looking up views with `GameObject.Find`/default `FindObjectsByType` while rooms are `SetActive(false)`: inactive objects are skipped.
- `Application.quitting` subscribed without unsubscribing (static event; stacks across Play sessions with domain reload off).
- Saving while a book is mid-flight, or on every shelf placement.
- Item ids derived from a build-time counter or transform order: the next builder run renumbers them and old saves point at the wrong books.
- Leaving old save files in the Steam cloud after changing the path or extension: they fill the file quota.
- Syncing `settings.json`: video/input options differ per machine.
- Time: store UTC; display local only in UI.
