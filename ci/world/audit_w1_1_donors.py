#!/usr/bin/env python3
from __future__ import annotations

import csv
import sys
from collections import defaultdict
from pathlib import Path

if len(sys.argv) != 3:
    raise SystemExit("usage: audit_w1_1_donors.py <assets_root> <diagnostics_dir>")

assets_root = Path(sys.argv[1])
diagnostics_dir = Path(sys.argv[2])
diagnostics_dir.mkdir(parents=True, exist_ok=True)

categories = {
    "FORGE": [
        "smith", "forge", "anvil", "furnace", "oven", "hammer",
        "grinder", "weapon", "sword", "axe", "shield", "tool",
        "barrel", "crate", "workbench",
    ],
    "INN": [
        "inn", "tavern", "bed", "table", "chair", "bench", "stool",
        "barrel", "bottle", "mug", "cup", "fireplace", "candle",
        "shelf", "chest", "kitchen", "food",
    ],
    "GUILD": [
        "board", "notice", "quest", "table", "chair", "bench", "book",
        "map", "banner", "weapon", "shield", "armor", "chest", "shelf",
        "desk", "counter",
    ],
    "MARKET": [
        "market", "stall", "shop", "cart", "crate", "basket", "barrel",
        "food", "fruit", "fish", "meat", "vegetable", "table", "counter",
    ],
    "STRUCTURE": [
        "wall", "floor", "door", "window", "roof", "stair", "beam",
        "column", "building", "house", "interior",
    ],
}

allowed_suffixes = {
    ".prefab", ".fbx", ".obj", ".mat", ".png", ".tga", ".jpg", ".jpeg"
}

rows: list[tuple[str, str, str, str]] = []
summary = defaultdict(int)
all_files = 0

for path in assets_root.rglob("*"):
    if not path.is_file():
        continue
    if path.suffix.lower() not in allowed_suffixes:
        continue

    all_files += 1
    rel = path.relative_to(assets_root).as_posix()
    haystack = rel.lower()

    matched = []
    for category, keywords in categories.items():
        hits = [k for k in keywords if k in haystack]
        if hits:
            matched.append((category, ",".join(sorted(set(hits)))))

    for category, keywords in matched:
        rows.append((category, path.stem, rel, keywords))
        summary[category] += 1

out_tsv = diagnostics_dir / "w1-1-rgpoly-donor-inventory.tsv"
with out_tsv.open("w", newline="", encoding="utf-8") as f:
    writer = csv.writer(f, delimiter="	")
    writer.writerow(["category", "asset_name", "relative_path", "keyword_hits"])
    writer.writerows(sorted(rows))

out_summary = diagnostics_dir / "w1-1-rgpoly-donor-summary.txt"
with out_summary.open("w", encoding="utf-8") as f:
    f.write("HIGHFLY W1.1 RG POLY DONOR AUDIT\n")
    f.write(f"asset files scanned: {all_files}\n")
    for category in categories:
        f.write(f"{category}: {summary[category]} matching entries\n")
    f.write("\nPolicy:\n")
    f.write("- RG Poly remains exterior host.\n")
    f.write("- Reuse matching interior props before importing external donors.\n")
    f.write("- External donor only fills a verified functional gap.\n")
    f.write("- No donor may add PlayerController, CameraController, Combat or damage authority.\n")

print(out_summary.read_text(encoding="utf-8"))
