# Investigation 020: Radius visual pulse and Territory tab layout

## Problems

Testing after `1ce3895` confirmed that ward radius changes are saved and synchronized to runtime territory state, but the short visual pulse was not visible enough in-game. The same test also showed that the `Close` button overlapped lower Territory tab controls.

## Evidence

Runtime logs show radius changes applying to the ward, syncing into runtime territory state, saving to JSON, and refreshing the open ward menu. This proves the broken part is visual feedback, not radius persistence or runtime synchronization.

Decompiled `PrivateArea.ShowAreaMarker()` only activates the guard-stone `CircleProjector` for 0.5 seconds. It is a hover-style vanilla marker, not a persistent or strong pulse. `PrivateArea.IsInside(...)` uses `m_radius`, so the functional ward area follows the updated radius once `m_radius` is changed.

## Decision

- Keep updating `PrivateArea.m_radius`.
- Keep updating `m_areaMarker.m_radius`.
- Still call vanilla `ShowAreaMarker()`.
- Add an explicit temporary LineRenderer radius pulse around the ward for 3 seconds.
- Move the `Close` button lower.
- Compact the lower Territory tab controls to remove overlap.

## Limits

The new pulse is visual feedback only. The actual ward/territory rules continue to use `PrivateArea.m_radius` and the stored territory radius.
