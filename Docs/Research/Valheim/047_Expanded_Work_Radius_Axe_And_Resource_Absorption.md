# Investigation 047: Expanded radius, axe slot, lower fuel drain, and ground resource absorption

## Problems observed

The test log showed that the ward menu kept refreshing with the ward radius value while the terraforming radius was saved separately. In the same test, the worker paused because fuel ran out. MineRock hits were visible, which means the worker successfully reached vanilla mining logic.

## Decisions

This package makes the next iteration more territory-scale and less wasteful:

- terraforming radius maximum is raised to 200;
- fuel is no longer consumed once per every small work operation;
- fuel now uses a work-progress meter and spends one fuel item after multiple terrain/mining/chopping operations;
- the spirit is brighter, larger, and emissive so it is visible in daytime;
- the preparation chest top row now exposes a third tool slot for axes;
- trees and logs can be hit by the worker through vanilla `TreeBase.Damage(HitData)` and `TreeLog.Damage(HitData)`;
- ground item stacks inside the ward territory can be absorbed into the virtual preparation chest or treasury if that chest already contains the same item stack.

## Ground item absorption rule

The worker does not create new resource categories automatically. It only pulls a dropped stack if an existing virtual chest stack already matches the item.

This keeps the feature safe: players decide what each virtual chest should collect by placing at least one matching stack there first.
