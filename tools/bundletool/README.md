# Local bundletool pin

- Version: `1.18.3`
- Official release: https://github.com/google/bundletool/releases/tag/1.18.3
- Download asset: `bundletool-all-1.18.3.jar`
- SHA-256: `A099CFA1543F55593BC2ED16A70A7C67FE54B1747BB7301F37FDFD6D91028E29`
- License: Apache License 2.0 in the official `google/bundletool` repository
- Intended use: local Android App Bundle validation only; never included in the player

The jar is intentionally ignored by Git. `scripts/inspect-aab.ps1` also executes `bundletool version` and requires exact output `1.18.3` before inspecting an AAB.
