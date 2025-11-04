#!/usr/bin/env python3
"""
tools/remove_splines.py

Safely remove the `com.unity.splines` dependency from a Unity project's
Packages/manifest.json.

Usage:
  python tools/remove_splines.py /path/to/UnityProject
  python tools/remove_splines.py --dry-run /path/to/UnityProject
  python tools/remove_splines.py --project /path/to/UnityProject --yes

Behavior:
- Creates a timestamped backup of manifest.json before modifying it.
- Does nothing if the dependency is not present.
- --dry-run shows what would be done without writing changes.
- --yes skips confirmation prompts.

This script is non-destructive: backups are always created.
"""

from __future__ import annotations
import argparse
import json
import shutil
import sys
from pathlib import Path
import time


def eprint(*args, **kwargs):
    print(*args, file=sys.stderr, **kwargs)


def parse_args() -> argparse.Namespace:
    p = argparse.ArgumentParser(
        description="Remove com.unity.splines from manifest.json"
    )
    p.add_argument(
        "project",
        nargs="?",
        default=".",
        help="Path to the Unity project root (default: current directory).",
    )
    p.add_argument(
        "--dry-run",
        action="store_true",
        help="Show what would be changed without writing the manifest.",
    )
    p.add_argument(
        "--yes",
        action="store_true",
        help="Do not prompt for confirmation; proceed automatically.",
    )
    return p.parse_args()


def backup_manifest(manifest_path: Path) -> Path:
    ts = int(time.time())
    backup_dir = manifest_path.parent / "ToolsManifestBackups"
    backup_dir.mkdir(parents=True, exist_ok=True)
    backup_path = backup_dir / f"manifest.json.bak.{ts}"
    shutil.copy2(manifest_path, backup_path)
    return backup_path


def load_manifest(manifest_path: Path) -> dict:
    with manifest_path.open("r", encoding="utf-8") as f:
        return json.load(f)


def write_manifest(manifest_path: Path, data: dict):
    # Write pretty JSON with 2-space indent to be consistent with Unity manifest style.
    with manifest_path.open("w", encoding="utf-8") as f:
        json.dump(data, f, indent=2, ensure_ascii=False)
        f.write("\n")


def main():
    args = parse_args()
    project_root = Path(args.project).expanduser().resolve()
    manifest_path = project_root / "Packages" / "manifest.json"

    if not manifest_path.exists():
        eprint(f"manifest.json not found at: {manifest_path}")
        sys.exit(1)

    try:
        manifest = load_manifest(manifest_path)
    except Exception as ex:
        eprint(f"Failed to parse manifest.json: {ex}")
        sys.exit(2)

    deps = manifest.get("dependencies", {})
    key = "com.unity.splines"

    if key not in deps:
        print(f"Dependency '{key}' not present in manifest.json. No change needed.")
        sys.exit(0)

    print(f"Found '{key}' in manifest.json with version: {deps.get(key)!r}")

    if args.dry_run:
        print("Dry-run mode: no changes will be written.")
        print("Would remove dependency:", key)
        sys.exit(0)

    if not args.yes:
        resp = input(f"Remove '{key}' from {manifest_path}? [y/N]: ").strip().lower()
        if resp not in ("y", "yes"):
            print("Aborted by user. No changes made.")
            sys.exit(0)

    try:
        backup_path = backup_manifest(manifest_path)
        print(f"Backed up manifest.json to: {backup_path}")
    except Exception as ex:
        eprint(f"Failed to backup manifest.json: {ex}")
        sys.exit(3)

    # Remove the dependency and write file
    try:
        del deps[key]
        manifest["dependencies"] = deps
        write_manifest(manifest_path, manifest)
        print(f"Removed '{key}' from manifest.json and wrote file: {manifest_path}")
        print(
            "Open the project in Unity and allow the Package Manager to restore packages."
        )
    except Exception as ex:
        eprint(f"Failed to write manifest.json: {ex}")
        # attempt to restore backup
        try:
            shutil.copy2(backup_path, manifest_path)
            eprint("Restored manifest.json from backup.")
        except Exception as ex2:
            eprint(f"Failed to restore backup: {ex2}")
        sys.exit(4)


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        eprint("\nInterrupted by user.")
        sys.exit(130)
