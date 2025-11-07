#!/usr/bin/env python3
"""
Fix manifest entries that include '.unity' in basenames by adding variants
with the '.unity' segment stripped.

This helps the client try sensible filenames like:
  - Base_Scene.unity.png  -> Base_Scene.png
  - also add MiniMap_Base_Scene.png and Map_Base_Scene.png as additional fallbacks

Usage:
  python3 tools/DynamicMap/site/fix_manifest_unity_names.py \
      --site-root tools/DynamicMap/site \
      --manifest data/maps-manifest.json \
      --backup

The script updates the manifest file in-place (after creating a backup when requested).
"""

from __future__ import annotations

import argparse
import json
import os
import re
import shutil
import sys
from typing import Dict, List, Optional, Set

DEFAULT_MANIFEST_REL = "data/maps-manifest.json"


def parse_args():
    p = argparse.ArgumentParser(
        description="Fix maps manifest entries with .unity in basenames"
    )
    p.add_argument(
        "--site-root", "-s", default="tools/DynamicMap/site", help="Site root directory"
    )
    p.add_argument(
        "--manifest",
        "-m",
        default=DEFAULT_MANIFEST_REL,
        help="Manifest path relative to site-root",
    )
    p.add_argument(
        "--backup",
        action="store_true",
        help="Create a backup of the manifest before modifying",
    )
    p.add_argument("--verbose", "-v", action="store_true", help="Verbose output")
    return p.parse_args()


def load_manifest(manifest_path: str) -> Optional[Dict]:
    if not os.path.isfile(manifest_path):
        return None
    with open(manifest_path, "r", encoding="utf-8") as fh:
        return json.load(fh)


def write_manifest(manifest_path: str, data: Dict):
    tmp = manifest_path + ".tmp"
    with open(tmp, "w", encoding="utf-8") as fh:
        json.dump(data, fh, indent=2, ensure_ascii=False)
    os.replace(tmp, manifest_path)


def ensure_backup(manifest_path: str):
    bak = manifest_path + ".bak"
    if not os.path.exists(bak):
        shutil.copy2(manifest_path, bak)
        return bak
    # if backup exists, create timestamped backup
    import datetime

    ts = datetime.datetime.utcnow().strftime("%Y%m%dT%H%M%SZ")
    bak_ts = f"{manifest_path}.bak.{ts}"
    shutil.copy2(manifest_path, bak_ts)
    return bak_ts


def strip_unity_segment(basename: str) -> Optional[str]:
    """
    If basename contains '.unity' before the final extension, strip that segment.
    Examples:
      'Base_Scene.unity.png' -> 'Base_Scene.png'
      'Level_Farm_01.unity.png' -> 'Level_Farm_01.png'
    If no match, return None.
    """
    m = re.match(
        r"^(?P<prefix>.+?)\.unity(?P<ext>\.[^./\\]+)$", basename, re.IGNORECASE
    )
    if not m:
        return None
    return f"{m.group('prefix')}{m.group('ext')}"


def add_variant_entries_for_scene(
    entries: List[Dict], site_maps_dir: str, verbose: bool = False
) -> int:
    """
    Given a list of manifest-entry dicts for a scene, add stripped variants for any
    basenames that include '.unity'. Returns number of added entries.
    Each manifest entry is expected to have keys:
      - basename (str)
      - source (optional)
      - existsInSite (optional)
    """
    existing_basenames: Set[str] = {
        e.get("basename") for e in entries if e.get("basename")
    }
    to_add: List[Dict] = []

    for e in list(entries):
        bn = e.get("basename") or ""
        if not bn:
            continue
        stripped = strip_unity_segment(bn)
        if not stripped:
            continue
        if stripped in existing_basenames:
            if verbose:
                print(
                    f"[DBG] stripped basename {stripped} already present for scene, skipping"
                )
            continue

        # Derive source if possible by replacing '.unity' in source as well
        src = e.get("source") or None
        derived_source = None
        if src:
            # if source path contains the basename, try to replace the basename
            try:
                src_bn = os.path.basename(src)
                if src_bn.lower() == bn.lower():
                    derived_source = src.replace(src_bn, stripped)
                else:
                    # try a simple replace of .unity segment in the filename
                    if ".unity" in src:
                        derived_source = src.replace(".unity", "")
            except Exception:
                derived_source = None

        # check whether stripped exists in the site maps folder (best-effort)
        exists_in_site = False
        if site_maps_dir:
            candidate_path = os.path.join(site_maps_dir, stripped)
            exists_in_site = os.path.isfile(candidate_path)

        # also create additional friendly candidates: MiniMap_<name> and Map_<name>
        base_no_ext = os.path.splitext(stripped)[0]
        extra1 = f"MiniMap_{base_no_ext}{os.path.splitext(stripped)[1]}"
        extra2 = f"Map_{base_no_ext}{os.path.splitext(stripped)[1]}"

        # Add the primary stripped entry
        new_entry = {
            "basename": stripped,
            "source": derived_source,
            "existsInSite": exists_in_site,
        }
        to_add.append(new_entry)
        existing_basenames.add(stripped)

        # add extras if not present
        if extra1 not in existing_basenames:
            exists1 = (
                os.path.isfile(os.path.join(site_maps_dir, extra1))
                if site_maps_dir
                else False
            )
            to_add.append({"basename": extra1, "source": None, "existsInSite": exists1})
            existing_basenames.add(extra1)
        if extra2 not in existing_basenames:
            exists2 = (
                os.path.isfile(os.path.join(site_maps_dir, extra2))
                if site_maps_dir
                else False
            )
            to_add.append({"basename": extra2, "source": None, "existsInSite": exists2})
            existing_basenames.add(extra2)

        if verbose:
            print(
                f"[INFO] will add stripped variants for {bn}: {stripped}, {extra1}, {extra2}"
            )

    # Append to entries
    for item in to_add:
        entries.append(item)
    return len(to_add)


def main():
    args = parse_args()
    site_root = os.path.abspath(args.site_root)
    manifest_rel = args.manifest
    manifest_path = os.path.join(site_root, manifest_rel)

    if not os.path.isdir(site_root):
        print(f"[ERR] site root not found: {site_root}", file=sys.stderr)
        sys.exit(2)
    if not os.path.isfile(manifest_path):
        print(f"[ERR] manifest not found: {manifest_path}", file=sys.stderr)
        sys.exit(3)

    if args.backup:
        bak = ensure_backup(manifest_path)
        if args.verbose:
            print(f"[INFO] backup created: {bak}")

    manifest = load_manifest(manifest_path)
    if manifest is None:
        print(f"[ERR] failed to read manifest JSON: {manifest_path}", file=sys.stderr)
        sys.exit(4)

    # site maps dir (where actual images are stored)
    site_maps_dir = os.path.join(site_root, "assets", "maps")
    if not os.path.isdir(site_maps_dir):
        # not fatal; we still update manifest entries but cannot check existsInSite
        if args.verbose:
            print(
                f"[WARN] site maps directory does not exist: {site_maps_dir}",
                file=sys.stderr,
            )

    scene_candidates = manifest.get("sceneMapCandidates") or {}
    total_added = 0
    for scene_key, entries in list(scene_candidates.items()):
        if not isinstance(entries, list):
            continue
        added = add_variant_entries_for_scene(
            entries,
            site_maps_dir if os.path.isdir(site_maps_dir) else None,
            verbose=args.verbose,
        )
        if added and args.verbose:
            print(f"[INFO] scene '{scene_key}': added {added} entries")
        total_added += added

    # write back manifest
    try:
        write_manifest(manifest_path, manifest)
    except Exception as e:
        print(f"[ERR] failed to write manifest: {e}", file=sys.stderr)
        sys.exit(5)

    print(
        f"[OK] updated manifest: {manifest_path} (added {total_added} variant entries)"
    )


if __name__ == "__main__":
    main()
