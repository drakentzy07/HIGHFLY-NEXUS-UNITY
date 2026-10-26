#!/usr/bin/env python3
"""Stage only free CC0 Quaternius FBX donors for the Unity 6 FURY prototype.

The paid Source/Pro distributions are never required. No third-party assets
are committed to the repository; CI records each downloaded SHA-256.
"""
from __future__ import annotations

import argparse
import hashlib
import re
from pathlib import Path
from zipfile import ZipFile

IMAGE_EXT = (".png", ".jpg", ".jpeg", ".tga")


def clean(text: str) -> str:
    return re.sub(r"[^a-z0-9]+", "_", text.lower()).strip("_")


def score(path: str, preferred: str) -> int:
    s = clean(path)
    stem = clean(Path(path).stem)
    result = (50 if "fbx" in s else 0) + (20 if "unity" in s else 0)
    if preferred in s:
        result += 100
    if "fullbody" in stem or "full_body" in stem:
        result += 12
    if "preview" in s or "collision" in s:
        result -= 60
    return result


def safe_copy(archive: ZipFile, member: str, dst: Path) -> None:
    # Avoid zip slip, malformed names and absolute paths.
    parts = Path(member).parts
    if not parts or any(x in ("..", "") for x in parts) or Path(member).is_absolute():
        raise ValueError(f"Unsafe asset name: {member}")
    dst.parent.mkdir(parents=True, exist_ok=True)
    with archive.open(member) as inp, dst.open("wb") as out:
        while chunk := inp.read(1024 * 1024):
            out.write(chunk)


def stage(archive_path: Path, root: Path, kind: str) -> list[str]:
    dest = root / kind
    selected = []
    with ZipFile(archive_path) as z:
        members = [m for m in z.namelist() if not m.endswith("/")]
        fbxs = [m for m in members if m.lower().endswith(".fbx")]
        if kind == "Base":
            for preference in ("regular_female", "regular_male"):
                options = sorted(fbxs, key=lambda f: score(f, preference), reverse=True)
                if preference == "regular_male":
                    options = [f for f in options if "female" not in clean(f)]
                if not options:
                    continue
                best = options[0]
                if score(best, preference) < 100:
                    # e.g. pack names Superhero_Male_FullBody
                    gender = "female" if "female" in preference else "male"
                    alternatives = [f for f in options if gender in clean(f)]
                    if not alternatives:
                        continue
                    best = alternatives[0]
                if best not in selected:
                    selected.append(best)
        elif kind == "Outfits":
            for preference in ("female_knight", "male_knight", "female_wizard", "male_warrior"):
                options = sorted(fbxs, key=lambda f: score(f, preference), reverse=True)
                if not options: continue
                best = options[0]
                if score(best, preference) < 100:
                    tokens = preference.split("_")
                    candidates = [f for f in options if all(token in clean(f) for token in tokens)]
                    if not candidates: continue
                    best = candidates[0]
                if best not in selected:
                    selected.append(best)
            if not selected and fbxs:
                # Donor names differ from gender/class naming; favor Unity FBX
                # outfit rigs first and retain CC0 modular pieces for later assembly.
                selected = sorted(fbxs, key=lambda p: (
                    "unity" not in clean(p),
                    not any(h in clean(p) for h in ("outfit", "knight", "warrior", "robe", "armor", "armour")),
                    "gltf" in clean(p),
                    len(p)
                ))[:12]
        elif kind == "Animations2":
            # The audited CC0 Standard ZIP contains one in-place Unity FBX and
            # one _RM alternative. FURY owns motion, so import only in-place.
            selected = [f for f in fbxs if f.lower().endswith("/unity/ual2_standard.fbx")]
            if len(selected) != 1:
                raise SystemExit("Expected exactly one UAL2 Standard Unity in-place FBX")
        elif kind == "Animations":
            hints = ("idle", "running", "run", "jog", "walking",
                     "sword", "slash", "punch", "attack", "magic",
                     "cast", "spell", "dodge", "roll")
            chosen = [f for f in fbxs if any(h in clean(f) for h in hints)]
            selected = chosen[:80] if chosen else fbxs[:60]
        if kind in ("Base", "Animations", "Animations2") and not selected:
            raise SystemExit(f"No usable CC0 FBX in {kind}: {archive_path}")
        for f in selected:
            # Directory structure preserved so adjacent textures remain discoverable.
            safe_copy(z, f, dest / f)

        # Textures stay at their original relative paths. CI avoids importing
        # large preview shots/background images into the Android/WebGL build.
        texture_candidates = [m for m in members if m.lower().endswith(IMAGE_EXT)
                              and not any(k in clean(m) for k in ("preview", "thumbnail", "screenshot"))]
        for f in texture_candidates[:180]:
            safe_copy(z, f, dest / f)

    digest = hashlib.sha256(archive_path.read_bytes()).hexdigest()
    return [f"{kind}: {len(selected)} FBX, textures={min(len(texture_candidates),180)}, sha256={digest}",
            *selected]


def main() -> None:
    p = argparse.ArgumentParser()
    p.add_argument("--base", required=True, type=Path)
    p.add_argument("--outfits", required=True, type=Path)
    p.add_argument("--animations", required=True, type=Path)
    p.add_argument("--animations-2", required=False, type=Path)
    p.add_argument("--project", required=True, type=Path)
    args = p.parse_args()
    root = args.project / "Assets/External/Quaternius/Hunters"
    log = []
    inputs = [("Base", args.base), ("Outfits", args.outfits),
              ("Animations", args.animations)]
    if args.animations_2 is not None:
        inputs.append(("Animations2", args.animations_2))
    for kind, source in inputs:
        if not source.is_file():
            raise SystemExit(f"Missing donor ZIP {kind}: {source}")
        log.extend(stage(source, root, kind))
    out = args.project / "build/diagnostics/quaternius-hunter-provenance.txt"
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text("\n".join(log) + "\n", encoding="utf-8")
    print(out.read_text())


if __name__ == "__main__":
    main()
