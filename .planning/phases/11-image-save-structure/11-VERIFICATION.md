---
phase: 11-image-save-structure
verified: 2026-04-03T07:00:00Z
status: passed
score: 11/11 must-haves verified
gaps: []
human_verification:
  - test: "Run inspection cycle in real camera mode, confirm D:\Log\{yyyyMMdd}\{HHmmss_fff}\ folder is created and contains {ShotName}_NG.jpg + {ShotName}_NG_annotated.jpg"
    expected: "Two files per NG shot appear inside a date>time sub-folder under D:\Log"
    why_human: "Requires physical camera + live inspection run; cannot be triggered programmatically without starting the WPF application"
  - test: "Confirm OK images are absent by default (SaveOkImage=false) and present after enabling SaveOkImage=true in Settings"
    expected: "Default run produces zero OK files; after toggling SaveOkImage ON, OK files appear"
    why_human: "Runtime behavior guarded by SystemSetting.SaveOkImage toggle — requires UI interaction"
---

# Phase 11: image-save-structure Verification Report

**Phase Goal:** 검사 이미지가 날짜>시간 하위폴더 계층으로 저장되고 NG만 기본 저장된다
**Verified:** 2026-04-03T07:00:00Z
**Status:** PASSED
**Re-verification:** No — initial verification

---

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | ImageFolderManager.BeginInspection() creates a date>time folder under SystemSetting.ImageSavePath | VERIFIED | ImageFolderManager.cs lines 21–42: reads `SystemSetting.Handle.ImageSavePath`, builds `{basePath}\{yyyyMMdd}\{HHmmss_fff}`, calls `Directory.CreateDirectory` |
| 2 | Millisecond collision produces _2, _3 suffix folders instead of overwriting | VERIFIED | ImageFolderManager.cs lines 30–38: `if (Directory.Exists(folderPath))` loop with suffix counter |
| 3 | InspectionSequenceContext.CurrentFolderPath is set once per inspection cycle in Clear() | VERIFIED | Sequence_Inspection.cs line 29: `CurrentFolderPath = ImageFolderManager.BeginInspection()` inside `Clear()` override |
| 4 | SystemSetting.ImageSavePath defaults to D:\Log | VERIFIED | SystemSetting.cs line 60: `= @"D:\Log";  //260403 hbk` — old BaseDirectory+Image removed |
| 5 | SaveResultImage uses InspectionSequenceContext.CurrentFolderPath instead of hardcoded D:\Log | VERIFIED | Action_Inspection.cs line 483: `ImageFolderManager.GetSavePath(_FolderPath, Name, isOK)`. Zero occurrences of `D:\Log` or `DateTime.Now` in SaveResultImage |
| 6 | OK images are not saved when SaveOkImage is false (default) | VERIFIED | Action_Inspection.cs line 475: `if (isOK && !setting.SaveOkImage) return;` — original guard preserved |
| 7 | NG images are saved as original + annotated pair in the time-folder | VERIFIED | Action_Inspection.cs lines 483–505: both GetSavePath and GetAnnotatedSavePath called, both saved async |
| 8 | Original image saved as {ShotName}_{OK|NG}.jpg | VERIFIED | ImageFolderManager.cs line 53: `string.Format("{0}_{1}.jpg", shotName, resultStr)` |
| 9 | Annotated image saved as {ShotName}_{OK|NG}_annotated.jpg in same folder | VERIFIED | ImageFolderManager.cs line 63: `string.Format("{0}_{1}_annotated.jpg", shotName, resultStr)` |
| 10 | Annotated image null-guarded (SIMUL mode safe) | VERIFIED | Action_Inspection.cs line 495: `if (annotated != null && !annotated.IsDisposed)` |
| 11 | Images saved asynchronously via Task.Factory.StartNew with Mat.Clone() | VERIFIED | Action_Inspection.cs lines 484–491 (original) and 498–505 (annotated): `image.Clone()` + `Task.Factory.StartNew` + `mat.Dispose()` in finally |

**Score:** 11/11 truths verified

---

### Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `WPF_Example/Utility/ImageFolderManager.cs` | Static utility class for inspection folder path generation | VERIFIED | File exists, 66 lines. Exports BeginInspection, GetSavePath, GetAnnotatedSavePath. `_lock` field present. Namespace `FinalVisionProject.Utility`. |
| `WPF_Example/Custom/Sequence/Inspection/Sequence_Inspection.cs` | InspectionSequenceContext with CurrentFolderPath property | VERIFIED | Line 20: `public string CurrentFolderPath { get; set; } = ""`. Clear() wires to BeginInspection(). |
| `WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs` | SaveResultImage with folder-based path and annotated image support | VERIFIED | `_FolderPath` field (line 180), OnBegin override (lines 194–202), rewritten SaveResultImage (lines 469–512). `ImageFolderManager.GetSavePath` and `GetAnnotatedSavePath` both present. |
| `WPF_Example/Setting/SystemSetting.cs` | ImageSavePath default changed to D:\Log | VERIFIED | Line 60: `= @"D:\Log"`. Old `AppDomain.CurrentDomain.BaseDirectory + @"Image"` fully removed. |
| `WPF_Example/FinalVision.csproj` | ImageFolderManager.cs in Compile includes | VERIFIED | csproj line 295: `<Compile Include="Utility\ImageFolderManager.cs" />` |

---

### Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| InspectionSequenceContext.Clear() | ImageFolderManager.BeginInspection() | method call in Clear() override | WIRED | Sequence_Inspection.cs line 29 |
| ImageFolderManager.BeginInspection() | SystemSetting.Handle.ImageSavePath | base path reference | WIRED | ImageFolderManager.cs line 21 |
| Action_Inspection.SaveResultImage() | ImageFolderManager.GetSavePath() | method call for original image path | WIRED | Action_Inspection.cs line 483 |
| Action_Inspection.SaveResultImage() | ImageFolderManager.GetAnnotatedSavePath() | method call for annotated image path | WIRED | Action_Inspection.cs line 497 |
| Action_Inspection.OnBegin() | InspectionSequenceContext.CurrentFolderPath | cast SequenceContext to read folder path | WIRED | Action_Inspection.cs lines 197–200: `prevResult as InspectionSequenceContext` then `_FolderPath = inspContext.CurrentFolderPath` |

---

### Data-Flow Trace (Level 4)

| Artifact | Data Variable | Source | Produces Real Data | Status |
|----------|---------------|--------|--------------------|--------|
| Action_Inspection.SaveResultImage | `_FolderPath` | `ImageFolderManager.BeginInspection()` via `OnBegin` → `InspectionSequenceContext.CurrentFolderPath` | Yes — `Directory.CreateDirectory` confirms real filesystem write | FLOWING |
| ImageFolderManager.BeginInspection | `basePath` | `SystemSetting.Handle.ImageSavePath` (INI-loaded setting, default `D:\Log`) | Yes — reads live setting value | FLOWING |

---

### Behavioral Spot-Checks

Step 7b: SKIPPED — application is a WPF GUI and has no runnable CLI entry point or standalone API to invoke without starting the full Windows process. Behavioral verification routed to human verification.

---

### Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
|-------------|------------|-------------|--------|----------|
| IMG-01 | 11-01-PLAN.md, 11-02-PLAN.md | 검사 이미지를 날짜>시간 하위폴더 구조로 저장 (`D:\Log\{yyyyMMdd}\{HHmmss}\{ShotName}_{OK|NG}.jpg`) | SATISFIED | ImageFolderManager produces `{basePath}\{yyyyMMdd}\{HHmmss_fff}\` hierarchy; GetSavePath returns `{ShotName}_{OK|NG}.jpg`; wired through InspectionSequenceContext and SaveResultImage |
| IMG-02 | 11-02-PLAN.md | OK 이미지 기본 미저장, NG 이미지만 기본 저장 (설정에서 변경 가능) | SATISFIED | SaveOkImage/SaveNgImage guards preserved in SaveResultImage (lines 475–476); setting defaults remain unchanged from prior phase |

**Orphaned requirements check:** REQUIREMENTS.md Traceability maps IMG-01 and IMG-02 to Phase 11, and both are claimed in plan frontmatter. No orphaned requirements.

---

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| — | — | — | — | No anti-patterns found |

No TODO/FIXME/placeholder comments, no empty return stubs, no hardcoded D:\Log references, no DateTime.Now inside SaveResultImage.

---

### Human Verification Required

#### 1. Live Inspection Folder Creation

**Test:** Start the application, load a recipe, trigger one inspection cycle (NG result expected).
**Expected:** A folder `D:\Log\{yyyyMMdd}\{HHmmss_fff}\` is created; it contains `{ShotName}_NG.jpg` and `{ShotName}_NG_annotated.jpg` for each shot that ran.
**Why human:** Requires WPF application startup and a physical or simulated camera trigger — cannot be exercised from the command line.

#### 2. OK Image Suppression by Default

**Test:** With `SaveOkImage = false` (default), run an inspection that produces an OK result. Verify no OK files appear in the time-folder.
**Expected:** Zero `*_OK.jpg` files in the output folder.
**Why human:** Depends on runtime SystemSetting value and actual inspection result — requires UI interaction.

---

### Gaps Summary

No gaps. All 11 observable truths are verified against the actual source files. All five artifacts exist, are substantive, and are wired into the data flow. Both IMG-01 and IMG-02 requirements are satisfied. Commits f240b0c, e8b7f37, bae0e46, 4baebb1 are confirmed in git history.

---

_Verified: 2026-04-03T07:00:00Z_
_Verifier: Claude (gsd-verifier)_
