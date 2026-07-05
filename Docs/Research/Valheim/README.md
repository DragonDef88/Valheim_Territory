# Valheim Research Notes

This directory contains engineering research notes based on direct inspection of Valheim assemblies through dnSpy.

These files are not user-facing documentation.

They exist to support architectural decisions in Clan Territory.

## Rules

- Never guess Valheim internals.
- If behavior affects architecture, inspect it in dnSpy.
- Record the inspected classes and methods.
- Separate confirmed facts from assumptions.
- Do not design systems on unverified behavior.
- If a class is missing from the current dnSpy export, mark the research as partial.

## Source Priority

1. Valheim assemblies inspected through dnSpy
2. Clan Territory GitHub main
3. BepInEx / Jötunn documentation
4. In-game tests
5. Architectural conclusions

## Research Index

- `001_PrivateArea_Lifecycle.md`
- `002_World_Load_Pipeline.md`
- `003_ZDO_Lifecycle.md`
- `004_Zone_Loading.md`
- `005_Player_RemovePiece.md`
- `006_WearNTear_EntryPoints.md`