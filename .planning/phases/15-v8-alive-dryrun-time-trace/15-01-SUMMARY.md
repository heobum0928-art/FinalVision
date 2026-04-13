---
phase: 15-v8-alive-dryrun-time-trace
plan: 01
subsystem: TCP terminal protocol
tags: [tcp, protocol, dryrun, time, trace, v8]
requires: [VisionRequestPacket, VisionResponsePacket, ResourceMap, SystemHandler]
provides:
  - DryRun/Time/Trace request parsing
  - DryRun/Time/Trace response serialization
  - DryRun ProcessTest intercept
affects:
  - WPF_Example/TcpServer/VisionRequestPacket.cs
  - WPF_Example/TcpServer/VisionResponsePacket.cs
  - WPF_Example/Custom/TcpServer/ResourceMap.cs
  - WPF_Example/Custom/SystemHandler.cs
tech-stack:
  added: []
  patterns: [existing packet subclass pattern, volatile bool flag]
key-files:
  created: []
  modified:
    - WPF_Example/TcpServer/VisionRequestPacket.cs
    - WPF_Example/TcpServer/VisionResponsePacket.cs
    - WPF_Example/Custom/TcpServer/ResourceMap.cs
    - WPF_Example/Custom/SystemHandler.cs
decisions:
  - D-01 honored: $LIGHT response format untouched
  - DryRun uses volatile bool for thread safety with ALIVE thread (future 15-02)
  - $TIME stored only in _syncedTime field — Windows clock unchanged
  - $TRACE values retained until next $TRACE (no per-test clear)
metrics:
  duration: ~15min
  completed: 2026-04-13
  tasks: 2
---

# Phase 15 Plan 01: DryRun/Time/Trace Protocol Extension Summary

Added DRYRUN/TIME/TRACE TCP command parsing, serialization, and SystemHandler handlers with DryRun-mode ProcessTest intercept that bypasses Sequences.Start and enqueues immediate OK result via SequenceBase.ResponseQueue.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Add request+response packet types | b17fd13 | VisionRequestPacket.cs, VisionResponsePacket.cs, ResourceMap.cs |
| 2 | SystemHandler handlers + DryRun intercept | 9e26774 | Custom/SystemHandler.cs |

## What Was Built

**VisionRequestPacket.cs**
- `VisionRequestType` enum: added `DryRun`, `Time`, `Trace`
- Constants: `CMD_RECV_DRYRUN="DRYRUN"`, `CMD_RECV_TIME="TIME"`, `CMD_RECV_TRACE="TRACE"`
- `Convert(string)` switch: 3 parse cases with length/TryParse guards
- `$TIME` parser: wraps `new DateTime(...)` in try-catch for `ArgumentOutOfRangeException` (invalid month/day/hour values return null)
- Cast helpers: `AsDryRun()`, `AsTime()`, `AsTrace()`
- Subclasses: `DryRunPacket(bool Enable)`, `TimePacket(DateTime SyncedTime)`, `TracePacket(string PalletId, string MaterialId)`

**VisionResponsePacket.cs**
- `EVisionResponseType` enum: added `DryRun`, `Time`, `Trace`
- Constants: `CMD_SEND_DRYRUN`, `CMD_SEND_TIME`, `CMD_SEND_TRACE`
- `Convert(packet)` switch: 3 serialization cases → `$CMD:Site,OK@` simple format
- Cast helpers: `AsDryRunResult()`, `AsTimeResult()`, `AsTraceResult()`
- Subclasses: `DryRunResultPacket`, `TimeResultPacket`, `TraceResultPacket`
- **$LIGHT case untouched** (CONTEXT D-01 — current format is confirmed spec)

**ResourceMap.cs**
- `SetIdentifier()`: added fall-through cases for DryRun/Time/Trace (no resource mapping needed)

**Custom/SystemHandler.cs**
- Fields: `volatile bool _dryRunMode`, `DateTime _syncedTime`, `string _palletId`, `string _materialId`
- `MainRun()` switch: 3 new case branches → ProcessDryRun/ProcessTime/ProcessTrace
- `ProcessTest()`: DryRun intercept branch — builds TestResultPacket(OK), `Sequences[packet.Identifier]?.ResponseQueue.Enqueue(dryResult)` with null guard, skips `Sequences.Start()`
- New methods: `ProcessDryRun` / `ProcessTime` / `ProcessTrace` — all log via `Logging.PrintLog(ELogType.Trace, ...)` and return result packet for MainRun's `Server.SendPacket()` path

## Deviations from Plan

None — plan executed exactly as written.

## Success Criteria

- [x] DRYRUN/TIME/TRACE Request parsing + Response serialization + SystemHandler handlers all implemented
- [x] DryRun ON → `ProcessTest()` skips `Sequences.Start()` and enqueues OK directly
- [x] `$TIME` invalid date values return null (ArgumentOutOfRangeException guard)
- [x] `$LIGHT` code unchanged (D-01)
- [ ] Build verification: not run in worktree (parallel execution; orchestrator / next wave handles build)

## Threat Model Mitigations Applied

- T-15-01 (Tampering, $TIME): `dataList.Length < 7` + `Int32.TryParse` per-field + `try/catch ArgumentOutOfRangeException` → return null
- T-15-02 (Tampering, $DRYRUN): `dataList.Length < 2` + `Int32.TryParse` guards
- T-15-03 (Tampering, $TRACE): `dataList.Length < 3` guard (comma-in-id is client responsibility — accepted)

## Self-Check: PASSED

- FOUND: WPF_Example/TcpServer/VisionRequestPacket.cs (CMD_RECV_DRYRUN/TIME/TRACE + DryRunPacket/TimePacket/TracePacket — grep count 24)
- FOUND: WPF_Example/TcpServer/VisionResponsePacket.cs (CMD_SEND_DRYRUN/TIME/TRACE + result subclasses)
- FOUND: WPF_Example/Custom/TcpServer/ResourceMap.cs (DryRun/Time/Trace cases)
- FOUND: WPF_Example/Custom/SystemHandler.cs (_dryRunMode + ProcessDryRun/Time/Trace — grep count 10)
- FOUND commit b17fd13 (Task 1)
- FOUND commit 9e26774 (Task 2)
