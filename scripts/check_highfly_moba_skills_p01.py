#!/usr/bin/env python3
"""Static safety smoke tests for isolated HIGHFLY P01. NO Unity build implied."""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parent.parent
SKILLS = ROOT / "Assets/Highfly/SkillsLab"
paths = {
    "combat": SKILLS / "HighflyMobaSkillsLab.cs",
    "fx": SKILLS / "HighflyMobaSkillFx.cs",
    "aim": SKILLS / "HighflyMobaAimButton.cs",
    "hud": SKILLS / "HighflyMobaSkillHud.cs",
    "installer": SKILLS / "Editor/HighflyMobaLabInstaller.cs",
}
src = {name: file.read_text(encoding="utf-8") for name, file in paths.items()}
checks = {
    "3 distinct original archetypes":
        "AerialMage, RiftFighter, GravityGuardian" in src["combat"],
    "9 skill casts (3 slots x 3 styles)":
        all("slot == " + str(n) in src["combat"] for n in (1, 2, 3))
        and all("Cast" + s in src["combat"] for s in ("Aerial", "Rift", "Gravity")),
    "independent cooldown per style": "float[3, 3]" in src["combat"],
    "11 real ultimate hits": "const int hits = 11" in src["combat"],
    "mana and stamina cost routing": "TrySpendMana" in src["combat"]
        and "TrySpendStamina" in src["combat"],
    "FURY-only player dash": "fury.TryDash" in src["combat"]
        and "GetComponent<CharacterController>().Move" not in src["combat"]
        and "transform.position +=" not in src["combat"],
    "enemy collision controlled impulses": "cc.Move" in src["combat"]
        and "ApplyAirborne" in src["combat"],
    "NonAlloc projectile and proximity": "SphereCastNonAlloc" in src["combat"]
        and "OverlapSphereNonAlloc" in src["combat"],
    "no duplicate hit per enemy per AoE": "_unique.Add(h)" in src["combat"],
    "FX never decide damage": "ApplyDamage(" not in src["fx"]
        and "CharacterController.Move" not in src["fx"],
    "pooled FX": "AcquireParticles" in src["fx"] and "AcquireLine" in src["fx"],
    "touch cancel / drag / release": all(key in src["aim"] for key in
        ("OnPointerDown", "OnDrag", "OnPointerUp", "CancelActive")),
    "cooldown and recast HUD": "lab.Cooldown" in src["hud"]
        and "RecastAvailable" in src["hud"],
    "original scene never overwritten": "AssetDatabase.CopyAsset" in src["installer"]
        and "Refusing to overwrite" in src["installer"]
        and "original.isDirty" in src["installer"],
    "legacy ATK remains active": "previous.enabled = false" not in src["installer"],
    "no external proprietary VFX imports": not any(x in "".join(src.values())
        for x in ("CraftPix", "Sketchfab", "MobileLegends/Assets", "mlbb_asset")),
    "braces balanced in authored sources": all(
        s.count("{") == s.count("}") for s in src.values()),
}
for label, ok in checks.items():
    print(("PASS" if ok else "FAIL") + " | " + label)
failures = [label for label, ok in checks.items() if not ok]
print(f"\n{len(checks) - len(failures)}/{len(checks)} static safety checks passed.")
if failures:
    raise SystemExit(1)
