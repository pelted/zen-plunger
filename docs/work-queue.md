# Work Queue

This queue tracks implementation work against the architecture plan. Keep it practical: each item should move the app toward a buildable, testable cabinet launcher.

## Now

- [x] Create repository bootstrap script
- [x] Add GitHub CLI to development setup
- [x] Scaffold .NET solution and projects
- [x] Add initial core launch contracts
- [x] Add initial Windows Steam launcher
- [x] Add versioned JSON table catalog model
- [x] Add JSON table catalog store/import service
- [x] Add tests for Steam launch command construction
- [x] Move starter table list behind an `ITableCatalog` implementation
- [x] Add tests for JSON catalog load/save/import behavior
- [x] Add basic app composition instead of direct construction in `MainWindow`

## Phase 1: MVP Launcher

- [ ] Implement tray host behavior
- [x] Add show/hide overlay controller
- [x] Build basic fullscreen overlay window
- [x] Add keyboard input path for cabinet button simulation
- [x] Load table metadata from a local JSON file
- [ ] Add a UI flow for importing a table catalog file
- [x] Launch selected Pinball FX table through Steam
- [x] Add process monitoring for Pinball FX
- [ ] Add basic file logging

## Phase 2: Cabinet Setup Assistant

- [ ] Detect connected displays
- [ ] Model playfield, backglass, and DMD display roles
- [ ] Locate Pinball FX settings files
- [ ] Read and validate Pinball FX cabinet mode configuration
- [ ] Report actionable setup issues in the UI
- [ ] Add guided setup flow for cabinet mode basics

## Phase 3: External System Integration

- [ ] Detect DMD Extensions install
- [ ] Validate `DmdDevice64.dll`
- [ ] Detect DOFLinx install
- [ ] Read and validate `DOFLinx.INI`
- [ ] Validate `PATH_FX` and `PATH_FX_B2S`
- [ ] Add diagnostics view for DMD and DOF integration

## Phase 4: Advanced Features

- [ ] Explore fast table switching with Pinball FX kept warm
- [ ] Add optional input automation experiments behind a feature flag
- [ ] Add per-table cabinet metadata
- [ ] Add richer table artwork and filtering
- [ ] Add exportable diagnostics bundle

## Warm Launch Findings (2026-05-02)

- Cold launch is now working reliably through Steam after fixing the launch parameter mapping. Pinball FX wants the numeric `sourceTableId` for `-Table`, while stable `Table_*` ids should remain the catalog and cabinet-media naming keys.
- Warm launch through a second Steam command did not switch tables while Pinball FX was already running.
- Overlay focus handoff does work. Bringing up the overlay and returning to the game lands on the Pinball FX pause menu.
- Manual testing found a repeatable exit sequence from gameplay back to the in-game table menu: `Up`, `Enter`, `Left`, `Enter`.
- We tried multiple in-app automation approaches for that sequence, including focus restore, window-message key delivery, `SendInput`, scan-code input, extended-key flags, and slower timings.
- None of the in-app keyboard injection attempts actually drove the Pinball FX pause menu even though focus returned to the game. The likely issue is that Pinball FX is reading input through a path that our current app-level automation does not reach reliably.
- Next step for tomorrow: stop tuning WPF/Win32 key simulation in the main app and run a small proof-of-input spike with a different or lower-level automation path. Candidates include AutoHotkey with alternate send modes or a more hardware-like input injection approach.
- If that spike works, integrate the helper behind a feature flag with clear diagnostics. If it does not, treat true warm launch as unsupported for MVP and fall back to cold launch plus a cleaner relaunch flow.

## Backlog Notes

- Keep the supported Steam launch path reliable before investing heavily in automation.
- Any feature that touches cabinet hardware or external tools should produce clear diagnostics.
- Do not let table metadata, external tool detection, or UI state leak into unrelated layers.
- Keep the JSON catalog schema versioned so migrations and SQLite import remain tractable.
