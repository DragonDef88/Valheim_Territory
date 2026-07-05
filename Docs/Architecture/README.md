# Clan Territory Architecture

This directory contains architectural documents for Clan Territory.

Unlike the Research documents, these files describe the architecture of the project itself.

## Document Types

### Research

Research documents answer:

> How does Valheim work?

Research is based on:

- dnSpy
- Valheim assemblies
- Runtime observations
- Verified facts

---

### Architecture

Architecture documents answer:

> How should Clan Territory be built?

Architecture documents are based on:

- Research
- Engineering principles
- Clean Architecture
- Project goals

---

### RFC

RFC documents answer:

> Why are we changing the architecture?

RFCs exist only when an architectural change is required.

Research

↓

Architecture Audit

↓

RFC

↓

Implementation

---

## Audit Status

| Audit | Status |
|--------|--------|
| 001 Core & Domain | PASS |

---

## Engineering Rule

Never redesign working systems without an architectural reason.

Architecture changes must always be supported by:

1. Research
2. Audit
3. RFC
4. Implementation