ClanTerritory - Physical Treasury for Visual Studio 2026
===============================================================

This package does not replace TerritoryModule.cs.

FILES
-----

1. Replace:

Source\ClanTerritory\Features\Territory\TerritoryZdoKeys.cs

2. Replace or create:

Source\ClanTerritory\Features\Territory\Services\PhysicalTreasuryService.cs

3. Include PhysicalTreasuryService.cs in the project:

Visual Studio 2026:
- Open Solution Explorer.
- Right-click project "ClanTerritory".
- Add -> Existing Item.
- Select:
  Source\ClanTerritory\Features\Territory\Services\PhysicalTreasuryService.cs
- Select the file and verify:
  Build Action = Compile

For the old non-SDK csproj, Visual Studio should add this line:

<Compile Include="Features\Territory\Services\PhysicalTreasuryService.cs" />

Place it near:

<Compile Include="Features\Territory\Services\TerritoryRuleService.cs" />

Do not add the line twice.

BUILD
-----

Run Rebuild All.

Expected:
1 succeeded
0 failed

RUNTIME LOG
-----------

Expected:

[TerritoryTreasury] Physical Treasury runtime started.
[TerritoryTreasury] Physical blackmetal treasury created behind ward:

Must not appear when opening Treasury:

[TerritoryContainers] Virtual container opened without world chest

BEHAVIOR
--------

- Real piece_chest_blackmetal.
- Position: 1.75 m strictly behind ward.
- Same horizontal rotation as ward.
- 8 x 4 inventory.
- Treasury stacks use existing 9999 hooks.
- Private access for ward creator.
- Cannot be damaged or removed with hammer.
- Existing virtual Treasury data migrates once.
- Resource absorption uses the physical chest inventory.
- Ground item ownership uses ZNetView.ClaimOwnership.
- Chest contents drop and chest is destroyed when ward is unregistered/destroyed.

Do not commit before successful runtime test.
