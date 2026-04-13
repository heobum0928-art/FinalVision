---
phase: 15-v8-alive-dryrun-time-trace
plan: 02
subsystem: TCP heartbeat (ALIVE)
tags: [tcp, protocol, alive, heartbeat, v8]
requires: [VisionRequestPacket, VisionResponsePacket, ResourceMap, SystemHandler, TcpServer]
provides:
  - ALIVE request parsing + echo response
  - V->Client 1s heartbeat thread
  - 3s timeout + 3-retry ping + client disconnect on no response
affects:
  - WPF_Example/TcpServer/VisionRequestPacket.cs
  - WPF_Example/TcpServer/VisionResponsePacket.cs
  - WPF_Example/Custom/TcpServer/ResourceMap.cs
  - WPF_Example/Custom/SystemHandler.cs
  - WPF_Example/SystemHandler.cs
tech-stack:
  added: []
  patterns: [existing packet subclass pattern, Thread+Stopwatch heartbeat, volatile bool sync flag]
key-files:
  created: []
  modified:
    - WPF_Example/TcpServer/VisionRequestPacket.cs
    - WPF_Example/TcpServer/VisionResponsePacket.cs
    - WPF_Example/Custom/TcpServer/ResourceMap.cs
    - WPF_Example/Custom/SystemHandler.cs
    - WPF_Example/SystemHandler.cs
decisions:
  - D-02 honored — 3s timeout, 3-retry ping then disconnect
  - D-03 honored — ALIVE_SEND_INTERVAL_MS / ALIVE_TIMEOUT_MS / ALIVE_RETRY_COUNT constants
  - D-04 honored — Thread + Stopwatch (no System.Threading.Timer), volatile bool flag, Release.Join(1000)
  - PerformAliveTimeout uses GetClient(0).Disconnect() — TcpServer.OnAlarmProcess handles cleanup + OnDisconnected alarm
metrics:
  duration: ~20min
  completed: 2026-04-13
  tasks: 2
requirements: [TERM-01, TERM-02]
---

# Phase 15 Plan 02: ALIVE Bidirectional Heartbeat Summary

Added bidirectional `$ALIVE:1@` heartbeat: Client->V echo response + V->Client 1s self-emitted heartbeat with 3s timeout, up to 3-retry ping, then forced client disconnect + error logging. Thread created in `SystemHandler.Initialize()` and joined in `Release()`.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Alive packet type + MainRun echo | 067d65c | VisionRequestPacket.cs, VisionResponsePacket.cs, ResourceMap.cs, Custom/SystemHandler.cs |
| 2 | ALIVE heartbeat thread (1s send + 3s timeout) | e1a52a4 | SystemHandler.cs, Custom/SystemHandler.cs |

## What Was Built

**VisionRequestPacket.cs**
- `VisionRequestType` enum: added `Alive`
- Constant: `CMD_RECV_ALIVE = "ALIVE"`
- `Convert(string)` switch: parse case with `dataList.Length < 1` guard + `Int32.TryParse` site guard
- `AsAlive()` cast helper
- `AlivePacket : VisionRequestPacket` subclass

**VisionResponsePacket.cs**
- `EVisionResponseType` enum: added `Alive`
- Constant: `CMD_SEND_ALIVE = "ALIVE"`
- `Convert(packet)` switch: serialization → `$ALIVE:Site,OK@`
- `AsAliveResult()` cast helper
- `AliveResultPacket : VisionResponsePacket` subclass

**ResourceMap.cs**
- `SetIdentifier()`: added `Alive` fall-through case (no resource mapping — Vision-internal)

**Custom/SystemHandler.cs**
- Fields: `volatile bool _aliveResponseReceived`, `ALIVE_SEND_INTERVAL_MS=1000`, `ALIVE_TIMEOUT_MS=3000`, `ALIVE_RETRY_COUNT=3`
- `MainRun()` switch: `Alive` case sets `_aliveResponseReceived=true` + calls `ProcessAlive()`
- `ProcessAlive()`: builds `AliveResultPacket` (Target, Site) → MainRun sends via `Server.SendPacket`
- `AliveProcess()` background loop:
  - Skips when `!Server.IsConnected()` (sleeps 500ms)
  - Resets `_aliveResponseReceived=false` before each send
  - Calls `SendAlivePacket()` → busy-wait up to `ALIVE_TIMEOUT_MS` (50ms polling) for flag
  - Retries up to `ALIVE_RETRY_COUNT` attempts
  - On all-retry failure → `PerformAliveTimeout()`
  - Sleeps remaining time until `ALIVE_SEND_INTERVAL_MS` from first send
- `SendAlivePacket()`: `Server.SendMessage(0, "$ALIVE:1@")` + TcpConnection log
- `PerformAliveTimeout()`: error log + `Server.GetClient(0).Disconnect()` wrapped in try/catch (T-15-09 mitigation)

**SystemHandler.cs (base partial)**
- Field: `private Thread mAliveThread`
- `Initialize()` step 5: creates `mAliveThread` (BelowNormal priority, IsBackground=true, name="AliveProcess") and starts
- `Release()`: `mAliveThread.Join(1000)` after `mSystemThread.Join(1000)` (null guarded)

## Deviations from Plan

None — plan executed exactly as written.

## Success Criteria

- [x] Client->V `$ALIVE:1@` → `$ALIVE:1,OK@` echo + `_aliveResponseReceived=true` flag
- [x] V->Client 1s `$ALIVE:1@` heartbeat from dedicated thread
- [x] 3s timeout → retry up to `ALIVE_RETRY_COUNT=3` → log + disconnect on failure
- [x] `Server.IsConnected()` guard skips send when no client
- [x] `Release()` joins `mAliveThread` (1000ms timeout)
- [ ] Build verification: not run in worktree (parallel execution; orchestrator handles build)

## Threat Model Mitigations Applied

- **T-15-06 (Tampering, ALIVE parse)**: `dataList.Length < 1` guard + `Int32.TryParse` site validation
- **T-15-07 (DoS, AliveProcess)**: accepted — 1s interval limit + `IsConnected()` guard + bounded busy-wait polling
- **T-15-08 (Spoofing, _aliveResponseReceived)**: accepted — single-client environment (MAX_CONNECTION_COUNT=1)
- **T-15-09 (DoS, PerformAliveTimeout)**: try/catch around `Disconnect()` + error logging prevents thread death

## Self-Check: PASSED

- FOUND: WPF_Example/TcpServer/VisionRequestPacket.cs (`CMD_RECV_ALIVE` x2, `AlivePacket` subclass, parse case)
- FOUND: WPF_Example/TcpServer/VisionResponsePacket.cs (`CMD_SEND_ALIVE` x2, `AliveResultPacket` subclass, serialize case)
- FOUND: WPF_Example/Custom/TcpServer/ResourceMap.cs (`VisionRequestType.Alive` fall-through)
- FOUND: WPF_Example/Custom/SystemHandler.cs (`_aliveResponseReceived` x2, `AliveProcess`, `SendAlivePacket`, `PerformAliveTimeout`, `ProcessAlive`, `ALIVE_TIMEOUT_MS`)
- FOUND: WPF_Example/SystemHandler.cs (`mAliveThread` declaration, creation block, `Join(1000)` in Release)
- FOUND commit 067d65c (Task 1)
- FOUND commit e1a52a4 (Task 2)
