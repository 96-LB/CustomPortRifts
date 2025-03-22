# Changelog

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
