#!/usr/bin/env python3
import argparse
import csv
import json
import os
import re
from typing import Dict, List, Tuple, Optional


TARGET_SCRIPT_GUID = "d551df320acceeb317a9e97502ade12f"


def walk_files(root: str, suffixes: Tuple[str, ...]) -> List[str]:
    paths: List[str] = []
    for dirpath, _, filenames in os.walk(root):
        for fname in filenames:
            if fname.endswith(suffixes):
                paths.append(os.path.join(dirpath, fname))
    return paths


def parse_localization(export_root: str) -> Dict[str, Dict[str, str]]:
    loc_dir = os.path.join(export_root, "Assets", "StreamingAssets", "Localization")
    result: Dict[str, Dict[str, str]] = {}
    for lang, fname in [("en", "English.csv"), ("zh", "ChineseSimplified.csv")]:
        table: Dict[str, str] = {}
        path = os.path.join(loc_dir, fname)
        if os.path.isfile(path):
            try:
                with open(path, "r", encoding="utf-8", errors="ignore") as fh:
                    reader = csv.reader(fh)
                    for row in reader:
                        if not row or len(row) < 2:
                            continue
                        key = row[0].strip()
                        value = row[1].strip()
                        if key:
                            table[key] = value
            except Exception:
                pass
        result[lang] = table
    return result


def build_guid_to_asset_path(export_root: str) -> Dict[str, str]:
    mapping: Dict[str, str] = {}
    assets_root = os.path.join(export_root, "Assets")
    for dirpath, _, filenames in os.walk(assets_root):
        for fname in filenames:
            if not fname.endswith(".meta"):
                continue
            meta_path = os.path.join(dirpath, fname)
            try:
                with open(meta_path, "r", encoding="utf-8", errors="ignore") as fh:
                    for idx, line in enumerate(fh):
                        if line.startswith("guid: "):
                            guid = line.split(":", 1)[1].strip()
                            asset_path = meta_path[:-5]
                            mapping[guid] = os.path.relpath(asset_path, export_root)
                            break
                        if idx > 8:
                            break
            except Exception:
                pass
    return mapping


def parse_number(value: str) -> Optional[float]:
    if value == "" or value is None:
        return None
    try:
        if any(ch in value for ch in (".", "e", "E")):
            return float(value)
        return float(int(value))
    except ValueError:
        return None


GUID_RE = re.compile(r"guid:\s*([0-9a-f]+)", re.IGNORECASE)


def parse_character_asset(path: str) -> Optional[Dict]:
    try:
        with open(path, "r", encoding="utf-8", errors="ignore") as fh:
            lines = fh.readlines()
    except Exception:
        return None

    script_line = None
    for line in lines[:20]:
        stripped = line.strip()
        if stripped.startswith("m_Script:") and "fileID: 70297966" in stripped and TARGET_SCRIPT_GUID in stripped:
            script_line = stripped
            break
    if not script_line:
        return None

    data: Dict[str, object] = {
        "asset_path": path,
    }
    integers = {
        "team",
        "characterIconType",
        "health",
        "hasSoul",
        "showHealthBar",
        "showName",
        "exp",
        "hasSkill",
    }
    floats = {
        "moveSpeedFactor",
        "hasCashChance",
        "itemSkillChance",
        "damageMultiplier",
        "gunCritRateGain",
        "nightVisionAbility",
        "gunScatterMultiplier",
        "gunDistanceMultiplier",
        "bulletSpeedMultiplier",
    }
    guid_fields = {
        "characterModel",
        "lootBoxPrefab",
        "facePreset",
        "aiController",
        "skillPfb",
    }

    for line in lines:
        stripped = line.strip()
        if not stripped or ":" not in stripped:
            continue
        key, raw_val = stripped.split(":", 1)
        key = key.strip()
        value = raw_val.strip()

        if key == "m_Name":
            data["asset_name"] = value
            continue
        if key == "nameKey":
            data["name_key"] = value
            continue

        if key in integers:
            num = parse_number(value)
            if num is not None:
                data[key] = int(num)
            continue

        if key in floats:
            num = parse_number(value)
            if num is not None:
                data[key] = float(num)
            continue

        if key in guid_fields:
            match = GUID_RE.search(line)
            if match:
                data[f"{key}Guid"] = match.group(1)
            continue

    return data


def enrich_character(entry: Dict, guid_map: Dict[str, str], loc: Dict[str, Dict[str, str]], export_root: str) -> Dict:
    asset_path = entry.get("asset_path", "")
    if asset_path:
        entry["asset_path"] = os.path.relpath(asset_path, export_root)

    asset_name = entry.get("asset_name", "")
    if "_" in asset_name:
        first, rest = asset_name.split("_", 1)
        entry["preset_type"] = first
        entry["preset_name"] = rest
        if "_" in rest:
            entry["preset_group"] = rest.split("_", 1)[0]
        else:
            entry["preset_group"] = rest
    else:
        entry["preset_type"] = asset_name
        entry["preset_name"] = ""
        entry["preset_group"] = ""

    name_key = entry.get("name_key", "")
    entry["name_en"] = loc.get("en", {}).get(name_key, "")
    entry["name_zh"] = loc.get("zh", {}).get(name_key, "")

    for field in ("characterModel", "lootBoxPrefab", "facePreset", "aiController", "skillPfb"):
        guid = entry.get(f"{field}Guid")
        if guid:
            entry[f"{field}Path"] = guid_map.get(guid, "")

    is_boss = False
    for token in (asset_name, name_key, entry.get("name_en", ""), entry.get("name_zh", "")):
        if token and "boss" in token.lower():
            is_boss = True
            break
    entry["is_bossish"] = is_boss

    bool_fields = ("hasSoul", "showHealthBar", "showName", "hasSkill")
    for bf in bool_fields:
        if bf in entry:
            entry[bf] = bool(entry[bf])

    return entry


def load_characters(export_root: str) -> List[Dict]:
    mono_dir = os.path.join(export_root, "Assets", "MonoBehaviour")
    assets = walk_files(mono_dir, (".asset",))
    loc = parse_localization(export_root)
    guid_map = build_guid_to_asset_path(export_root)
    entries: List[Dict] = []
    for asset in assets:
        parsed = parse_character_asset(asset)
        if parsed:
            enriched = enrich_character(parsed, guid_map, loc, export_root)
            entries.append(enriched)
    entries.sort(key=lambda e: (e.get("preset_type", ""), e.get("preset_group", ""), e.get("name_en", ""), e.get("asset_name", "")))
    return entries


def write_csv(path: str, entries: List[Dict]) -> None:
    field_order = [
        "asset_path",
        "asset_name",
        "preset_type",
        "preset_group",
        "preset_name",
        "name_key",
        "name_en",
        "name_zh",
        "team",
        "characterIconType",
        "is_bossish",
        "health",
        "hasSoul",
        "showHealthBar",
        "showName",
        "exp",
        "moveSpeedFactor",
        "hasSkill",
        "hasCashChance",
        "itemSkillChance",
        "characterModelPath",
        "lootBoxPrefabPath",
        "facePresetPath",
        "aiControllerPath",
        "skillPfbPath",
    ]
    with open(path, "w", encoding="utf-8", newline="") as fh:
        writer = csv.DictWriter(fh, field_order, extrasaction="ignore")
        writer.writeheader()
        for entry in entries:
            row = entry.copy()
            for key in ("hasSoul", "showHealthBar", "showName", "hasSkill", "is_bossish"):
                if key in row:
                    row[key] = "TRUE" if row[key] else "FALSE"
            writer.writerow(row)


def main() -> None:
    parser = argparse.ArgumentParser(description="List Duckov characters defined in AssetRipper export.")
    parser.add_argument("export_root", help="Path to AssetRipper ExportedProject root (folder that contains Assets/)")
    parser.add_argument("--out_csv", default="characters.csv", help="Path for CSV output")
    parser.add_argument("--out_json", default="characters.json", help="Path for JSON output")
    args = parser.parse_args()

    entries = load_characters(args.export_root)

    with open(args.out_json, "w", encoding="utf-8") as fh:
        json.dump(entries, fh, indent=2, ensure_ascii=False)

    write_csv(args.out_csv, entries)


if __name__ == "__main__":
    main()
