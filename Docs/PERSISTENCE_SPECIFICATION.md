# Clan Territory — Persistence Specification

## Purpose

Persistence stores the long-term state of Clan Territory.

Persistence must not store Unity objects, Valheim objects, GameObjects, Components, or runtime-only references.

Persistence stores only persistence records.

## Core Rule

Ward is the root of saved territory data.

A saved territory is stored as a Ward-centered record.

## Storage Pipeline

```text
Domain Entity
↓
Mapper
↓
Persistence Record
↓
Save File
↓
JSON
↓
Save File
↓
Persistence Record
↓
Mapper
↓
Domain Entity

Save Location
BepInEx/config/ClanTerritory/worlds/<worldName>.json
Backup Location
BepInEx/config/ClanTerritory/backups/<worldName>_<timestamp>.json
Save File Structure
{
  "metadata": {
    "version": 1,
    "worldName": "sfsfs",
    "savedAtUtc": "2026-07-02T00:00:00Z",
    "pluginVersion": "0.3.0-alpha",
    "build": "Alpha",
    "recordCount": 1
  },
  "wards": [
    {
      "wardId": "3095305870:14738",
      "territory": {
        "territoryId": "territory_3095305870:14738",
        "ownerPlayerId": 0,
        "ownerName": "Test",
        "x": 100.0,
        "y": 20.0,
        "z": -50.0,
        "radius": 100.0
      },
      "permissions": null,
      "terrain": null,
      "portals": null,
      "extensions": {}
    }
  ]
}
Rules
Domain objects are never serialized directly.
Unity and Valheim objects are never serialized.
Every save file must contain metadata.
Every save file must contain a version.
WardRecord is the root record for territory data.
Save must create a backup before overwriting an existing file.
Load must be safe if the file does not exist.
Corrupted files must not crash the mod.
Future data must be added without breaking old saves.
Migration must be supported before changing save format.
Versioning

Current save format version:

1

Save format version is separate from plugin version.

Error Handling

If save file is missing:

Start with empty state.

If save file is corrupted:

Log error.
Ignore corrupted file.
Start with empty state.

If backup fails:

Log warning.
Continue save attempt.
Future Sections

Reserved future fields:

permissions
terrain
portals
extensions

These fields must remain optional.