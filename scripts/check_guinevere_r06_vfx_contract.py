#!/usr/bin/env python3
"""Static R06 VFX isolation gate; does NOT replace compiling Unity or S23 QA."""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
FX = ROOT / "Assets/Highfly/Combat/HighflyGuinevereVfx.cs"
PREMIUM = ROOT / "Assets/Highfly/Combat/HighflyGuineverePremiumVfx.cs"
COMBAT = ROOT / "Assets/Highfly/Combat/HighflyGuinevereCombat.cs"
MOTOR = ROOT / "Assets/Highfly/Core/HighflyThirdPersonMotor.cs"

base = FX.read_text(encoding="utf-8")
vfx = PREMIUM.read_text(encoding="utf-8")
combat = COMBAT.read_text(encoding="utf-8")
motor = MOTOR.read_text(encoding="utf-8")

events = {
    "PrismaSpawn": 1, "Launch": 1, "Impact": 1,
    "Mark": 1, "Lift": 1, "Basic": 1, "Blink": 1,
    "Echo": 1, "Tempestad": 1,
}
for name, required in events.items():
    trigger = "_premium." + name + "("
    assert base.count(trigger) == required, f"R06_BRIDGE_{name}_DRIFT"
    assert re.search(r"public\s+void\s+" + name + r"\s*\(", vfx), (
        f"R06_METHOD_{name}_MISSING"
    )

assert "Queue<ParticleSystem>" in vfx, "R06_PARTICLE_POOL_MISSING"
assert "Queue<LineRenderer>" in vfx, "R06_RING_POOL_MISSING"
assert "_activeGhosts.Clear()" in vfx, "R06_GHOST_CLEANUP_MISSING"
assert "if (_premium != null) _premium.SetQuality((int)quality);" in base
assert "Shader.Find" in vfx
assert "tempestHits = 11" in combat, "R06_ULTIMATE_HITS_DRIFT"
assert "private IEnumerator TempestSequence()" in combat
assert "private void Update()" in motor
assert "HighflyQuaternius" not in vfx or "Quaternius" in vfx
for forbidden in (
    "CharacterController.Move(", "transform.Translate(", "resources.TrySpendMana(",
    "Damage(", "DealDamage(", "motor.Dash(", "Animator.SetTrigger(",
    "Resources.Load(", "UnityWebRequest", "HttpClient",
):
    assert forbidden not in vfx, f"R06_VFX_MUST_NOT_OWN_GAMEPLAY: {forbidden}"
assert "HighflyGuineverePremiumVfx" in base
assert "new GameObject(\"R06_Quaternius_Ghost\")" in vfx
print("HIGHFLY_GUINEVERE_R06_STATIC_CONTRACT_PASS: 9 visual hooks, "
      "pooling, Quaternius ghost cleanup, zero gameplay authority")
print("WARNING: UNITY COMPILATION, MOBILE SHADERS AND REAL S23 FPS NOT VERIFIED")
