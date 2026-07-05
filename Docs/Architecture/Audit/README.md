# Architecture Audits

Architecture audits evaluate the current state of Clan Territory.

Audits do not change architecture directly.

They identify:

- what is healthy;
- what should be watched;
- what requires RFC.

## Status Values

| Status | Meaning |
|--------|---------|
| PASS | Architecture is healthy |
| WATCH | Acceptable now, but should be monitored |
| ACTION | Requires RFC before implementation |

## Audit Index

| Audit | Title | Status |
|-------|-------|--------|
| 001 | Core & Domain | PASS |
| 002 | Runtime & Persistence | ACTION |
| 003 | Territory, WardDetection & WorldDiscovery | ACTION |
| 004 | Integration Layer | ACTION |
| 005 | Runtime Implementation Review | ACTION |