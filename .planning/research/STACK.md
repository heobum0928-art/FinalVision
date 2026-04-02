# Stack Research — FinalVision v2.0

## Key Findings

**No new NuGet packages required.** All features implementable with existing packages.

### Recipe Editing UI
- `PropertyTools.Wpf 3.1.0` (already present) — PropertyGrid bound to InspectionParam
- WPF built-in `TabControl` for 5 Shot tabs
- Grab preview uses existing `Mat → BitmapFrame` pattern from MainView

### Image Management
- `System.IO` only — date>time folder hierarchy is pure code change in `Action_Inspection.SaveResultImage`
- `Ookii.Dialogs.Wpf 5.0.1` (already referenced) — Vista-style folder browser for directory load
- Image deletion: `File.Delete` + existing `CustomMessageBox`

### Tact Time Logging
- `ActionContext.Timer` / `SequenceContext.Timer` — Stopwatch instances already present
- Insert `Logging.PrintLog` calls reading `Timer.ElapsedMilliseconds`

## What NOT to Add
- Halcon/FAI (forbidden)
- MVVM framework (inconsistency with existing code)
- Entity Framework/SQLite (flat log files sufficient)
- Any additional NuGet package

## Build Order Recommendation
1. Recipe copy bug fix — unblocks reliable recipe editing
2. RecipeEditorWindow — PropertyGrid + InspectionParam pattern reuse
3. Image management — isolated path change in SaveResultImage
4. Tact time logging — one-line additions at known insertion points
5. Run/Grab role clarification — code-path labeling, no structural change

## Confidence: HIGH
All packages confirmed in packages.config + csproj. No new dependencies needed.
