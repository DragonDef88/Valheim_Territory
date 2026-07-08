Territory Radius Lifecycle Research

Date:
2026-07-08

## Purpose

Research how ClanTerritory should implement territory radius control so that:

- the UI slider changes the real ward private area;
- vanilla ward protection uses the same radius;
- the visible area marker matches the same radius;
- the value is stored in ClanTerritory ZDO keys;
- future STU_Ward-like features can use the same radius.

---

# Sources

Valheim decompiled sources:

- `Docs/Research/Valheim/Decompiled/assembly_valheim/PrivateArea.cs`
- `Docs/Research/Valheim/Decompiled/assembly_valheim/CircleProjector.cs`

Existing ClanTerritory research:

- `Docs/Research/Mods/003_STU_Ward_Managed_Settings.md`

---

# Valheim PrivateArea radius

`PrivateArea` has a public runtime field:

```csharp
public float m_radius = 10f;

This field is the actual private-area radius used by Valheim runtime checks.

PrivateArea.Awake() copies the field to the area marker:

if (m_areaMarker)
{
    m_areaMarker.m_radius = m_radius;
}

This means vanilla Valheim initializes the visual marker from the private area radius at startup.

Access checks

The real protection logic uses PrivateArea.m_radius.

PrivateArea.CheckAccess(...) iterates all private areas and checks:

allArea.IsEnabled() && allArea.IsInside(point, radius)

IsInside(...) uses:

Utils.DistanceXZ(transform.position, point) < m_radius + radius

Therefore, changing only UI text or JSON does not change protection.

To change the real protection area, ClanTerritory must update:

privateArea.m_radius
Area marker

CircleProjector owns the visible radius marker field:

public float m_radius = 5f;

During Update(), every marker segment position is calculated from:

Mathf.Sin(angle) * m_radius
Mathf.Cos(angle) * m_radius

Therefore, changing only PrivateArea.m_radius changes protection but not necessarily the marker.

To keep the marker aligned with protection, ClanTerritory must also update:

privateArea.m_areaMarker.m_radius
STU_Ward reference

Existing STU_Ward research found that STU_Ward stores extended ward settings in ZDO, including:

stuw_radius
stuw_auto_close_doors
stuw_auto_close_delay

It also applies settings back to runtime objects:

updates PrivateArea.m_radius;
updates CircleProjector.m_radius;
updates marker-related runtime values;
invalidates spatial/runtime caches when radius changes.

ClanTerritory decision from existing research:

Do not use STU_Ward keys directly.

Use ClanTerritory keys:

ct_territory_radius
ct_auto_close_doors
ct_auto_close_delay
ClanTerritory radius decision

The territory radius must be stored on the ward ZDO because the ward anchors the territory.

Suggested key:

ct_territory_radius

Default value should be the current vanilla ward radius:

privateArea.m_radius

Radius changes should use the same owner-authoritative lifecycle as territory naming:

UI slider
    ↓
WardMenuController
    ↓
WardMenuTerritoryActions
    ↓
TerritoryRadiusService.RequestSetRadius(...)
    ↓
ZNetView.InvokeRPC("CT_SetTerritoryRadius", ...)
    ↓
owner RPC handler
    ↓
ZDO.Set("ct_territory_radius", radius)
    ↓
Apply runtime radius
    ↓
Publish TerritoryRadiusChangedEvent
    ↓
WardMenuService.Refresh()
Runtime apply rules

When a radius is loaded or changed, ClanTerritory must apply it to:

privateArea.m_radius

and, when present:

privateArea.m_areaMarker.m_radius

This ensures:

vanilla CheckAccess() uses the same radius;
IsInside() uses the same radius;
area marker matches the same radius;
damage protection and ward access checks stay consistent.
Range limits

Do not decide final balance in research.

Suggested first implementation constants:

Min radius: 10
Max radius: 100
Step: 5

These can later become config values.

Auto-close doors

Auto-close doors should not be implemented in the radius commit.

It needs separate research because doors are separate Valheim objects and may use:

Door
Interactable
ZNetView
ZDO state
animations
owner RPCs

Required future research:

Research Valheim door lifecycle

Only after that should ClanTerritory implement:

ct_auto_close_doors
ct_auto_close_delay
Implementation direction

First code commit after this research:

Introduce TerritoryRadius feature

Expected files:

Source/ClanTerritory/Features/TerritoryRadius/
├── Events/
│   └── TerritoryRadiusChangedEvent.cs
├── Services/
│   ├── ITerritoryRadiusService.cs
│   └── TerritoryRadiusService.cs
├── TerritoryRadiusModule.cs
└── TerritoryRadiusZdoKeys.cs

Required integration:

register radius RPC from PrivateAreaHooks.AwakePostfix;
apply stored radius when ward awakens;
update WardMenuModelBuilder to read radius from service/ZDO;
publish event after successful owner write;
refresh open ward menu on radius change.
Do not implement yet

Do not implement these in the radius commit:

auto-close doors;
restriction bitmask;
portal restrictions;
pickup restrictions;
item stand restrictions;
Guilds access;
Groups access.

Those need separate research and separate commits.