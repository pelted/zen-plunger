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
- [ ] Launch selected Pinball FX table through Steam
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

## Backlog Notes

- Keep the supported Steam launch path reliable before investing heavily in automation.
- Any feature that touches cabinet hardware or external tools should produce clear diagnostics.
- Do not let table metadata, external tool detection, or UI state leak into unrelated layers.
- Keep the JSON catalog schema versioned so migrations and SQLite import remain tractable.
