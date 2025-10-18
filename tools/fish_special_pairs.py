#!/usr/bin/env python3
import argparse
import csv
import os
import re
from typing import Dict, List, Tuple, Set


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
        d = {}
        path = os.path.join(loc_dir, fname)
        if os.path.isfile(path):
            try:
                with open(path, 'r', encoding='utf-8', errors='ignore') as fh:
                    reader = csv.reader(fh)
                    for row in reader:
                        if not row or len(row) < 2:
                            continue
                        key = row[0].strip(); text = row[1].strip()
                        if key:
                            d[key] = text
            except Exception:
                pass
        maps[lang] = d
    return maps


def build_guid_to_asset_path(export_root: str) -> Dict[str, str]:
    mapping: Dict[str, str] = {}
    assets_root = os.path.join(export_root, "Assets")
    for dp, dn, fn in os.walk(assets_root):
        for f in fn:
            if not f.endswith('.meta'):
                continue
            meta_path = os.path.join(dp, f)
            try:
                with open(meta_path, 'r', encoding='utf-8', errors='ignore') as fh:
                    for i, line in enumerate(fh):
                        if line.startswith('guid: '):
                            guid = line.split(':', 1)[1].strip()
                            asset_path = meta_path[:-5]
                            mapping[guid] = os.path.relpath(asset_path, export_root)
                            break
                        if i > 8:
                            break
            except Exception:
                pass
    return mapping


def build_item_index(export_root: str) -> Dict[int, Dict]:
    """Return typeID -> {prefabName, displayKey, tags:[guid], prefabPath}"""
    items: Dict[int, Dict] = {}
    prefabs = walk_files(os.path.join(export_root, "Assets"), (".prefab",))
    tid_pat = re.compile(r"^\s*typeID\s*:\s*(\d+)\s*$")
    name_pat = re.compile(r"^\s*m_Name\s*:\s*(.*)$")
    disp_pat = re.compile(r"^\s*displayName\s*:\s*(.*)$")
    guid_pat = re.compile(r"guid:\s*([0-9a-f]+)")
    for pf in prefabs:
        try:
            with open(pf, 'r', encoding='utf-8', errors='ignore') as fh:
                lines = fh.readlines()
        except Exception:
            continue
        content = ''.join(lines)
        if 'typeID:' not in content:
            continue
        go_name = None
        display_key = None
        for line in lines[:200]:
            m = name_pat.match(line)
            if m and go_name is None:
                go_name = m.group(1).strip()
            m2 = disp_pat.match(line)
            if m2 and display_key is None:
                display_key = m2.group(1).strip()
        if display_key is None:
            for line in lines:
                m2 = disp_pat.match(line)
                if m2:
                    display_key = m2.group(1).strip()
                    break
        # collect tag guids in file (approximation; good enough for Only* checks)
        tag_guids = guid_pat.findall(content)
        for line in lines:
            m = tid_pat.match(line)
            if m:
                tid = int(m.group(1))
                if tid not in items:
                    items[tid] = {
                        'prefabName': go_name or '',
                        'displayKey': display_key or '',
                        'tags': tag_guids,
                        'prefabPath': os.path.relpath(pf, export_root),
                    }
    return items


def find_special_pairs(export_root: str) -> List[Dict]:
    # Scan files once, then aggregate identical entries (same source, bait, fish, chance)
    block_pat = re.compile(r"specialPairs:\s*(?:\n\s*-\s*baitID:\s*\d+\s*\n\s*fishID:\s*\d+\s*\n\s*chance:\s*[0-9.]+)+", re.M)
    entry_pat = re.compile(r"-\s*baitID:\s*(\d+)\s*\n\s*fishID:\s*(\d+)\s*\n\s*chance:\s*([0-9.]+)")
    file_set: Set[str] = set()
    for root in (os.path.join(export_root, 'Assets'),):  # include Scenes via Assets to avoid double scanning
        for path in walk_files(root, ('.unity', '.prefab', '.asset')):
            file_set.add(path)
    counts: Dict[Tuple[str,int,int,float], int] = {}
    for path in file_set:
        try:
            with open(path, 'r', encoding='utf-8', errors='ignore') as fh:
                content = fh.read()
        except Exception:
            continue
        for blk in block_pat.finditer(content):
            for em in entry_pat.finditer(blk.group(0)):
                key = (
                    os.path.relpath(path, export_root),
                    int(em.group(1)),
                    int(em.group(2)),
                    float(em.group(3)),
                )
                counts[key] = counts.get(key, 0) + 1
    pairs: List[Dict] = []
    for (source, bait, fish, chance), cnt in counts.items():
        pairs.append({'source': source, 'baitID': bait, 'fishID': fish, 'chance': chance, 'count': cnt})
    return pairs


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('export_root', help='Path to AssetRipper ExportedProject root (folder that contains Assets/)')
    ap.add_argument('--out_csv', default='fish_special_pairs.csv', help='Output CSV path')
    args = ap.parse_args()

    loc = parse_localization(args.export_root)
    guid_map = build_guid_to_asset_path(args.export_root)
    items = build_item_index(args.export_root)
    pairs = find_special_pairs(args.export_root)

    # detect fishes by displayKey or prefabName prefix
    def is_fish(item):
        return (item.get('displayKey', '').startswith('Item_Fish_') or item.get('prefabName', '').startswith('Fish_'))

    # Compile Only* tag name set for quick checks
    only_names = {'Fish_OnlySunDay', 'Fish_OnlyDay', 'Fish_OnlyNight', 'Fish_OnlyRainDay', 'Fish_OnlyStorm'}
    def flags_for_fish(item):
        tags = item.get('tags', []) or []
        names = set()
        for g in tags:
            apath = guid_map.get(g, '')
            base = os.path.splitext(os.path.basename(apath))[0] if apath else ''
            if base in only_names:
                names.add(base)
        return {
            'SunnyOnly': 'Fish_OnlySunDay' in names,
            'DayOnly': 'Fish_OnlyDay' in names,
            'NightOnly': 'Fish_OnlyNight' in names,
            'RainOnly': 'Fish_OnlyRainDay' in names,
            'StormOnly': 'Fish_OnlyStorm' in names,
        }

    # Prepare rows: one per pair, and also include fishes with no pairs
    rows = [[
        'fishID','fishPrefab','fishKey','fishEN','fishZH',
        'SunnyOnly','DayOnly','NightOnly','RainOnly','StormOnly',
        'baitID','baitPrefab','baitKey','baitEN','baitZH','chance','occurrences',
        'sceneID','sceneEN','sceneZH','sourceAsset'
    ]]

    # Index pairs by fishID
    pairs_by_fish: Dict[int, List[Dict]] = {}
    for p in pairs:
        pairs_by_fish.setdefault(p['fishID'], []).append(p)

    # All fishes from items
    fish_type_ids = [tid for tid,it in items.items() if is_fish(it)]
    # Fallback: scan Fish_*.prefab under Assets/GameObject for any missed fish
    if not fish_type_ids:
        go_prefabs = walk_files(os.path.join(args.export_root, 'Assets'), ('.prefab',))
        tid_pat = re.compile(r"^\s*typeID\s*:\s*(\d+)\s*$")
        for pf in go_prefabs:
            base = os.path.basename(pf)
            if not base.startswith('Fish_'):
                continue
            try:
                with open(pf, 'r', encoding='utf-8', errors='ignore') as fh:
                    lines = fh.readlines()
            except Exception:
                continue
            tid = None
            for line in lines:
                m = tid_pat.match(line)
                if m:
                    tid = int(m.group(1)); break
            if tid is None:
                continue
            fish_type_ids.append(tid)
            # if this fish isn't in items, add a minimal record
            items.setdefault(tid, {
                'prefabName': os.path.splitext(base)[0],
                'displayKey': '',
                'tags': [],
                'prefabPath': os.path.relpath(pf, args.export_root),
            })
    for fish_id in sorted(fish_type_ids):
        fit = items.get(fish_id, {})
        fkey = fit.get('displayKey','')
        f_en = loc.get('en',{}).get(fkey,'')
        f_zh = loc.get('zh',{}).get(fkey,'')
        fflags = flags_for_fish(fit)
        fish_pairs = pairs_by_fish.get(fish_id, [])
        if not fish_pairs:
            rows.append([
                fish_id, fit.get('prefabName',''), fkey, f_en, f_zh,
                fflags['SunnyOnly'], fflags['DayOnly'], fflags['NightOnly'], fflags['RainOnly'], fflags['StormOnly'],
                '', '', '', '', '', '',
                '', '', '',
            ])
        else:
            for p in fish_pairs:
                bit = items.get(p['baitID'], {})
                bkey = bit.get('displayKey','')
                b_en = loc.get('en',{}).get(bkey,'')
                b_zh = loc.get('zh',{}).get(bkey,'')
                # derive scene id and localization
                src = p['source']
                mscene = re.search(r'(Level_[A-Za-z0-9_]+)', src)
                scene_id = mscene.group(1) if mscene else ''
                stripped = scene_id.replace('Level_', '') if scene_id.startswith('Level_') else scene_id
                # Candidate localization keys (in order): exact Level_*, bare key, Location_*, MapLocation_*
                def localize_scene(lang: str) -> str:
                    lm = loc.get(lang, {})
                    for k in (scene_id, stripped, f'Location_{stripped}', f'MapLocation_{stripped}'):
                        if k and k in lm and lm[k]:
                            return lm[k]
                    return ''
                scene_en = localize_scene('en')
                scene_zh = localize_scene('zh')
                rows.append([
                    fish_id, fit.get('prefabName',''), fkey, f_en, f_zh,
                    fflags['SunnyOnly'], fflags['DayOnly'], fflags['NightOnly'], fflags['RainOnly'], fflags['StormOnly'],
                    p['baitID'], bit.get('prefabName',''), bkey, b_en, b_zh, p['chance'], p.get('count',1),
                    scene_id, scene_en, scene_zh, src
                ])

    with open(args.out_csv, 'w', encoding='utf-8') as f:
        for r in rows:
            f.write(','.join(map(lambda x: str(x).replace(',', ';'), r))+'\n')
    print(f"Wrote {args.out_csv} with {len(rows)-1} rows")


if __name__ == '__main__':
    main()
