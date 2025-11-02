#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
fix_markers_override.py

Small helper to override mapping for specific loot items in markers CSV.

By default this forces Item_BP_GPU_Potato to be associated with the
sceneId "Level_GroundZero_1" and sets sourcePath to the canonical scene file.

Usage:
    python3 tools/DynamicMap/fix_markers_override.py \
        --csv tools/DynamicMap/site/data/markers.csv \
        --id Item_BP_GPU_Potato \
        --scene-id Level_GroundZero_1 \
        --scene-path "Assets/Scenes/Level_GroundZero/Level_GroundZero_1.unity"

The script makes a backup of the original CSV (same path + ".bak").
"""

from __future__ import annotations

import argparse
import csv
import os
import shutil
import sys
from typing import List


def parse_args() -> argparse.Namespace:
    p = argparse.ArgumentParser(
        description="Override loot item mapping in markers CSV."
    )
    p.add_argument(
        "--csv",
        "-c",
        default="tools/DynamicMap/site/data/markers.csv",
        help="Path to markers CSV (default: %(default)s)",
    )
    p.add_argument(
        "--id",
        "-i",
        default="Item_BP_GPU_Potato",
        help="Item id to override (default: %(default)s). Matches 'id' column or goName/itemKeyOrName when present.",
    )
    p.add_argument(
        "--scene-id",
        "-s",
        default="Level_GroundZero_1",
        help="SceneId to force for matching rows (default: %(default)s).",
    )
    p.add_argument(
        "--scene-path",
        "-p",
        default="Assets/Scenes/Level_GroundZero/Level_GroundZero_1.unity",
        help="sourcePath / scenePath to write into the CSV for matching rows (default: %(default)s).",
    )
    p.add_argument(
        "--dry-run",
        action="store_true",
        help="Do not modify files; just report what would be changed.",
    )
    return p.parse_args()


def load_csv(path: str) -> List[dict]:
    with open(path, "r", encoding="utf-8", newline="") as fh:
        reader = csv.DictReader(fh)
        rows = list(reader)
    return rows


def write_csv(path: str, rows: List[dict], fieldnames: List[str]) -> None:
    tmp = path + ".tmp"
    with open(tmp, "w", encoding="utf-8", newline="") as fh:
        writer = csv.DictWriter(fh, fieldnames=fieldnames)
        writer.writeheader()
        for r in rows:
            writer.writerow(r)
    os.replace(tmp, path)


def match_row_for_id(row: dict, target_id: str) -> bool:
    # Check common columns that may contain the item identifier
    candidates = []
    for key in ("id", "itemKeyOrName", "goName", "name"):
        if key in row and row.get(key):
            candidates.append(str(row.get(key)))
    # Case-insensitive compare and also allow prefixed forms
    for val in candidates:
        if not val:
            continue
        if val.lower() == target_id.lower():
            return True
        # Some CSVs store 'Item_BP_GPU_Potato' while others store 'LootBox_Formula_BP_GPU_Potato'
        if val.lower().endswith(target_id.lower()):
            return True
    return False


def main() -> None:
    args = parse_args()
    csv_path = args.csv
    target_id = args.id
    scene_id = args.scene_id
    scene_path = args.scene_path
    dry = args.dry_run

    if not os.path.isfile(csv_path):
        print(f"[ERR] CSV not found: {csv_path}", file=sys.stderr)
        sys.exit(1)

    print(f"[INFO] Loading CSV: {csv_path}")
    rows = load_csv(csv_path)
    if not rows:
        print("[WARN] CSV is empty", file=sys.stderr)
        sys.exit(0)

    fieldnames = list(rows[0].keys())
    changed = 0
    matched_indices: List[int] = []

    for idx, row in enumerate(rows):
        if match_row_for_id(row, target_id):
            # Only change if sceneId differs or sourcePath differs
            old_scene = (row.get("sceneId") or "").strip()
            old_path = (row.get("sourcePath") or "").strip()
            need_change = False
            if old_scene != scene_id:
                need_change = True
            if old_path != scene_path:
                need_change = True
            if need_change:
                matched_indices.append(idx)
                changed += 1
                print(
                    f"[INFO] Will override row {idx}: id={row.get('id')!r} sceneId {old_scene!r} -> {scene_id!r}"
                )
                # apply changes in memory
                row["sceneId"] = scene_id
                # also update sourcePath if column exists
                if "sourcePath" in row:
                    row["sourcePath"] = scene_path

    if changed == 0:
        print("[INFO] No matching rows found; nothing to change.")
        return

    # Backup original
    bak = csv_path + ".bak"
    print(f"[INFO] Backing up original CSV to: {bak}")
    shutil.copy2(csv_path, bak)

    if dry:
        print("[DRY-RUN] No changes written. Exiting.")
        return

    # Write back
    print(f"[INFO] Writing updated CSV with {changed} modified rows.")
    write_csv(csv_path, rows, fieldnames)
    print("[OK] Done.")


if __name__ == "__main__":
    main()
