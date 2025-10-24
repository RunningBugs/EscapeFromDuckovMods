#!/usr/bin/env python3
import argparse
import csv
import json
import os
import re
from typing import Dict, List, Tuple


def walk_files(root: str, suffixes: Tuple[str, ...]) -> List[str]:
    out = []
    for dp, dn, fn in os.walk(root):
        for f in fn:
            if f.endswith(suffixes):
                out.append(os.path.join(dp, f))
    return out


def parse_localization(export_root: str) -> Dict[str, Dict[str, str]]:
    loc_dir = os.path.join(export_root, "Assets", "StreamingAssets", "Localization")
    maps: Dict[str, Dict[str, str]] = {}
    for lang, fname in [("en", "English.csv"), ("zh", "ChineseSimplified.csv")]:
        path = os.path.join(loc_dir, fname)
        d = {}
        if os.path.isfile(path):
            try:
                with open(path, "r", encoding="utf-8", errors="ignore") as fh:
                    reader = csv.reader(fh)
                    for row in reader:
                        if not row or len(row) < 2:
                            continue
                        key = row[0].strip()
                        text = row[1].strip()
                        if key:
                            d[key] = text
            except Exception:
                pass
        maps[lang] = d
    return maps


def extract_mono_objects(lines: List[str]) -> List[Dict]:
    objs = []
    start_indices = []
    for i, line in enumerate(lines):
        if line.startswith("--- !u!114 "):
            start_indices.append(i)
    start_indices.append(len(lines))
    for i in range(len(start_indices) - 1):
        s = start_indices[i]
        e = start_indices[i + 1]
        block = lines[s:e]
        # ensure it's MonoBehaviour
        if not block or not block[0].startswith("--- !u!114 "):
            continue
        objs.append({"start": s, "end": e, "lines": block})
    return objs


def build_guid_to_asset_path(export_root: str) -> Dict[str, str]:
    """Build a GUID -> asset relative path map by scanning .meta files under Assets/.
    Useful to resolve tag GUIDs to Tag asset filenames (Tag_<Name>)."""
    mapping: Dict[str, str] = {}
    assets_root = os.path.join(export_root, "Assets")
    for dp, dn, fn in os.walk(assets_root):
        for f in fn:
            if not f.endswith('.meta'):
                continue
            meta_path = os.path.join(dp, f)
            try:
                with open(meta_path, 'r', encoding='utf-8', errors='ignore') as fh:
                    # GUID is at top of Unity meta files; stop after a few lines for speed
                    for i, line in enumerate(fh):
                        if line.startswith('guid: '):
                            guid = line.split(':', 1)[1].strip()
                            asset_path = meta_path[:-5]  # strip .meta
                            mapping[guid] = os.path.relpath(asset_path, export_root)
                            break
                        if i > 8:
                            break
            except Exception:
                pass
    return mapping


def parse_item_from_block(block_lines: List[str]) -> Dict:
    # Detect an Item-like block by presence of key fields
    # Fields we try to capture: typeID, displayName, maxStackCount, value, quality, displayQuality, weight, order, soundKey, iconGUID
    item = {}
    pat_map = {
        "typeID": re.compile(r"^\s*typeID\s*:\s*(\d+)\s*$"),
        "displayName": re.compile(r"^\s*displayName\s*:\s*(.*)$"),
        "maxStackCount": re.compile(r"^\s*maxStackCount\s*:\s*(\d+)\s*$"),
        "value": re.compile(r"^\s*value\s*:\s*(\d+)\s*$"),
        "quality": re.compile(r"^\s*quality\s*:\s*(\d+)\s*$"),
        "displayQuality": re.compile(r"^\s*displayQuality\s*:\s*(\d+)\s*$"),
        "weight": re.compile(r"^\s*weight\s*:\s*([0-9.]+)\s*$"),
        "order": re.compile(r"^\s*order\s*:\s*(\d+)\s*$"),
        "soundKey": re.compile(r"^\s*soundKey\s*:\s*(.*)$"),
    }
    icon_guid = None
    for line in block_lines:
        for k, pat in pat_map.items():
            m = pat.match(line)
            if m:
                item[k] = m.group(1).strip()
        if icon_guid is None and "icon:" in line and "guid:" in line:
            mg = re.search(r"guid:\s*([0-9a-f]+)", line)
            if mg:
                icon_guid = mg.group(1)
    if icon_guid:
        item["iconGUID"] = icon_guid
    # Required fields
    if "typeID" in item and "displayName" in item:
        return item
    return {}


def list_items(export_root: str) -> List[Dict]:
    items: List[Dict] = []
    prefabs = walk_files(os.path.join(export_root, "Assets"), (".prefab",))
    for pf in prefabs:
        try:
            with open(pf, "r", encoding="utf-8", errors="ignore") as fh:
                lines = fh.readlines()
        except Exception:
            continue
        # Find GameObject name
        go_name = None
        for line in lines[:200]:
            if line.strip().startswith("m_Name:"):
                go_name = line.split(":", 1)[1].strip()
                break
        # Extract MonoBehaviour blocks
        blocks = extract_mono_objects(lines)
        # First pass: find primary item MB
        primary = None
        for b in blocks:
            meta = parse_item_from_block(b["lines"])
            if meta:
                primary = meta
                break
        if not primary:
            continue
        # Fill extra info
        type_id = int(primary.get("typeID", 0))
        max_stack = int(primary.get("maxStackCount", 1) or 1)
        # Attempt to find nested attributes in same file (list: entries with key/baseValue, tags, vars)
        stats: Dict[str, float] = {}
        key_pat = re.compile(r"^\s*key\s*:\s*(.*)$")
        base_pat = re.compile(r"^\s*baseValue\s*:\s*([0-9.]+)\s*$")
        cur_key = None
        tag_guids: List[str] = []
        variables_count = 0
        constants_count = 0
        agents_count = 0
        effects_count = 0
        reading_tags = False
        reading_which = None
        def is_new_prop(s: str) -> bool:
            # a same-level or higher-level property start like '  something:'
            return bool(re.match(r"^\s{2,}[a-zA-Z_][a-zA-Z0-9_]*\s*:\s*", s)) and not s.strip().startswith('-')
        for b in blocks:
            for line in b["lines"]:
                km = key_pat.match(line)
                if km:
                    cur_key = km.group(1).strip()
                bm = base_pat.match(line)
                if bm and cur_key:
                    try:
                        stats[cur_key] = float(bm.group(1))
                    except Exception:
                        pass
                    cur_key = None
                # tags
                if not reading_tags and line.strip().startswith("tags:"):
                    reading_tags = True
                    continue
                if reading_tags:
                    if "guid:" in line and "-" in line:
                        mg = re.search(r"guid:\s*([0-9a-f]+)", line)
                        if mg:
                            tag_guids.append(mg.group(1))
                    elif is_new_prop(line):
                        # ignore internal container props like 'list:' or 'entries:' inside tags
                        prop = line.strip().split(':', 1)[0]
                        if prop not in ("list", "entries"):
                            reading_tags = False
                # list counters
                if reading_which is None:
                    if line.strip().startswith("variables:"):
                        reading_which = 'variables'
                    elif line.strip().startswith("constants:"):
                        reading_which = 'constants'
                    elif line.strip().startswith("agents:"):
                        reading_which = 'agents'
                    elif line.strip().startswith("effects:"):
                        reading_which = 'effects'
                else:
                    if line.strip().startswith("- "):
                        if reading_which == 'variables': variables_count += 1
                        elif reading_which == 'constants': constants_count += 1
                        elif reading_which == 'agents': agents_count += 1
                        elif reading_which == 'effects': effects_count += 1
                    elif is_new_prop(line):
                        reading_which = None

        # presence flags across file
        presence = {
            "inventory": any(l.strip().startswith("inventory:") for l in lines),
            "usageUtilities": any(l.strip().startswith("usageUtilities:") for l in lines),
            "slots": any(l.strip().startswith("slots:") for l in lines),
            "itemGraphic": any(l.strip().startswith("itemGraphic:") for l in lines),
        }
        disp_key = primary.get("displayName", "")
        category = ""
        if disp_key.startswith("Item_"):
            parts = disp_key.split("_")
            if len(parts) > 1:
                category = parts[1]
        items.append({
            "prefab": os.path.relpath(pf, export_root),
            "prefabName": go_name or "",
            "typeID": type_id,
            "displayNameKey": disp_key,
            "category": category,
            "maxStackCount": max_stack,
            "stackable": max_stack > 1,
            "value": int(primary.get("value", 0) or 0),
            "quality": int(primary.get("quality", 0) or 0),
            "displayQuality": int(primary.get("displayQuality", 0) or 0),
            "weight": float(primary.get("weight", 0.0) or 0.0),
            "order": int(primary.get("order", 0) or 0),
            "soundKey": primary.get("soundKey", ""),
            "iconGUID": primary.get("iconGUID", ""),
            "tags": tag_guids,
            "variablesCount": variables_count,
            "constantsCount": constants_count,
            "agentsCount": agents_count,
            "effectsCount": effects_count,
            **presence,
            "stats": stats,
        })
    # Deduplicate by typeID (keep first)
    seen = set()
    uniq: List[Dict] = []
    for it in items:
        if it["typeID"] in seen:
            continue
        seen.add(it["typeID"])
        uniq.append(it)
    return sorted(uniq, key=lambda x: x["typeID"])


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("export_root", help="Path to AssetRipper ExportedProject root (folder that contains Assets/")
    ap.add_argument("--out_csv", default="items.csv", help="Output CSV path")
    ap.add_argument("--out_json", default="items.json", help="Output JSON path")
    args = ap.parse_args()

    items = list_items(args.export_root)
    loc = parse_localization(args.export_root)
    guid_map = build_guid_to_asset_path(args.export_root)

    # Enrich with localized names/descriptions where possible
    for it in items:
        key = it.get("displayNameKey", "")
        it["nameEN"] = loc.get("en", {}).get(key, "")
        it["nameZH"] = loc.get("zh", {}).get(key, "")
        desc_key = (key + "_Desc") if key else ""
        it["descEN"] = loc.get("en", {}).get(desc_key, "")
        it["descZH"] = loc.get("zh", {}).get(desc_key, "")

    # Map tag GUIDs to Icon asset paths
    for it in items:
        icon_guid = it.get("iconGUID", "")
        icon_path = guid_map.get(icon_guid, "")
        it["iconPath"] = icon_path.replace('Sprite', 'Texture2D').replace('.asset', '.png')

    # Map tag GUIDs to Tag_<Name> keys and localized names
    for it in items:
        tag_guids = it.get("tags", []) or []
        tag_keys = []
        tag_en = []
        tag_zh = []
        for g in tag_guids:
            asset_path = guid_map.get(g, "")
            base = os.path.splitext(os.path.basename(asset_path))[0] if asset_path else ""
            tkey = ("Tag_" + base) if base else ""
            if tkey:
                tag_keys.append(tkey)
                tag_en.append(loc.get("en", {}).get(tkey, base))
                tag_zh.append(loc.get("zh", {}).get(tkey, base))
        it["tagKeys"] = tag_keys
        it["tagsEN"] = tag_en
        it["tagsZH"] = tag_zh

    # Write CSV (core fields)
    cols = [
        "typeID",
        "prefabName",
        "displayNameKey",
        "category",
        "nameEN",
        "nameZH",
        "descEN",
        "descZH",
        "maxStackCount",
        "stackable",
        "value",
        "quality",
        "displayQuality",
        "weight",
        "order",
        "soundKey",
        "iconGUID",
        "tags",
        "tagKeys",
        "tagsEN",
        "tagsZH",
        "variablesCount",
        "constantsCount",
        "agentsCount",
        "effectsCount",
        "inventory",
        "usageUtilities",
        "slots",
        "itemGraphic",
        "prefab",
    ]
    with open(args.out_csv, "w", encoding="utf-8") as f:
        f.write(",".join(cols) + "\n")
        for it in items:
            row = []
            for c in cols:
                v = it.get(c, "")
                row.append(str(v).replace(",", ";"))
            f.write(",".join(row) + "\n")

    # Write JSON (full details including stats)
    with open(args.out_json, "w", encoding="utf-8") as f:
        json.dump(items, f, ensure_ascii=False, indent=2)

    print(f"Wrote {args.out_csv} with {len(items)} items and {args.out_json}")


if __name__ == "__main__":
    main()
