# Changelog

## v1.0.3 - 30 October 2025
Updated game version. Compatible with Patch 1.10.0.

## v1.0.2 - 18 September 2025
Updated game version. Compatible with Patch 1.8.0.

## v1.0.1 - 14 August 2025
Updated game version. Compatible with Patch 1.7.0.

## v1.0.0 - 7 July 2025
⚠️ **This release contains breaking changes.**
- ⚠️ Removes the mod's old functionality entirely in light of custom portraits now being supported by the base game.
- Adds support for global overrides of portraits in base game levels, either on a per-character or per-track basis
- Adds reskin toggles for Cadence (Crypt costume) and the NecroDancer (Crypt and Burger costumes).
- Adds toggles to prevent Beastmaster, Coda, and Shopkeeper from overriding portraits in their respective extra modes.
- Adds a version check which causes the mod to be automatically disabled when the game updates.

Updated game version. Compatible with Patch 1.6.0.

## v0.2.2 - 22 March 2025
- Disables the underlying portrait animators, which was sometimes causing custom hero portraits to have a jittery offset.
- Removes stray white pixels appearing along the border of custom sprites.

## v0.2.1 - 21 March 2025
- Fixes a bug which caused portraits to sometimes not properly display on the first attempt of the level if they took a long time to load.

## v0.2.0 - 21 March 2025
⚠️ **This release contains breaking changes.**
- Adds support for custom hero portraits to replace Cadence on the left side. Hero portraits are loaded from `CustomPortRifts/Hero` and follow the same conventions as counterpart portraits.
- ⚠️ Counterpart portraits are now loaded from `CustomPortRifts/Counterpart` instead of `CustomPortRifts`.
- ⚠️ To maintain consistency, counterpart portraits are no longer shifted upwards 100 pixels.

## v0.1.0 - 17 March 2025
Initial release.
