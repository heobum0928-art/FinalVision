---
date: "2026-04-14 17:50"
promoted: false
---

refactor ALIVE heartbeat to 5s packet-arrival timeout (a900d1b) — removed
  retry/response-wait, V→PLC 1s unconditional send, PLC→V 5s idle = down
