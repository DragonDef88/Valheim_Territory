# Clan Territory — Domain Model

## Core Idea

Ward is the heart of the territory system.

The original Valheim Ward becomes the center of territory ownership, permissions, security, terrain control, portal control, and future automation.

## Root Concept

```text
Ward
↓
Territory
↓
Owner
↓
Permissions
↓
Extensions
Domain Rules
Domain must not depend on Unity.
Domain must not depend on Valheim.
Domain must not depend on BepInEx.
Domain contains rules and value objects.
Features adapt Valheim objects into Domain objects.
Current Domain Objects
Identifiers
WardId
TerritoryId
PlayerId
Value Objects
WorldPosition
TerritoryRadius
OwnerInfo
Entities
Territory
Territory

A Territory:

has a TerritoryId;
belongs to a WardId;
has an OwnerInfo;
has a WorldPosition;
has a TerritoryRadius;
can check whether a point is inside;
can check whether it overlaps another territory.
Boundary

Allowed in Domain:

string
int
long
float
Domain objects

Not allowed in Domain:

UnityEngine.Vector3
GameObject
PrivateArea
Player
ZDO
ZNetView
BepInEx classes
Harmony classes