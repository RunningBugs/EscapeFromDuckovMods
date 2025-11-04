#!/usr/bin/env python3
"""
tools/add_unity_packages.py

Safely patch a Unity project's Packages/manifest.json to include a set of
recommended packages (for Unity 2022.3.x projects). This script:

- Accepts a project path as the first argument (optional). If omitted, uses cwd.
- Backs up the existing manifest.json to manifest.json.bak.TIMESTAMP.
- Adds missing package entries without modifying existing versions.
- Supports a --dry-run flag to preview changes without writing.

Usage:
    python tools/add_unity_packages.py /path/to/UnityProject
    python tools/add_unity_packages.py --dry-run /path/to/UnityProject

This script is non-destructive: it never deletes existing manifest.json and
always creates a backup before writing.
"""

import sys
import os
import json
import shutil
import time
from pathlib import Path

# Recommended packages for Unity 2022.3.x (typical compatible versions).
# If you have a different target editor version, adjust versions accordingly.
RECOMMENDED_PACKAGES = {
    "com.unity.ugui": "1.0.0",
    "com.unity.render-pipelines.universal": "14.0.12",
    "com.unity.mathematics": "1.2.6",
    "com.unity.splines": "2.6.2",
    "com.unity.burst": "1.8.9",
    "com.unity.textmeshpro": "3.0.6",
}


def load_manifest(manifest_path: Path) -> dict:
    if not manifest_path.exists():
        # Return a minimal manifest structure
        return {"dependencies": {}}
    with manifest_path.open("r", encoding="utf-8") as f:
        try:
            return json.load(f)
        except json.JSONDecodeError as e:
            print(f"ERROR: Failed to parse manifest.json: {manifest_path}\n{e}")
            sys.exit(2)


def backup_manifest(manifest_path: Path, backup_root: Path) -> Path:
    ts = int(time.time())
    backup_path = backup_root / f"manifest.json.bak.{ts}"
    backup_root.mkdir(parents=True, exist_ok=True)
    if manifest_path.exists():
        shutil.copy2(manifest_path, backup_path)
        print(f"Backed up existing manifest.json to: {backup_path}")
    else:
        # touch an empty backup file to indicate we created from scratch
        backup_path.write_text(
            "// manifest.json did not exist; created by script\n", encoding="utf-8"
        )
        print(
            f"No existing manifest.json found. Created placeholder backup at: {backup_path}"
        )
    return backup_path


def write_manifest(manifest_path: Path, data: dict):
    manifest_path.parent.mkdir(parents=True, exist_ok=True)
    with manifest_path.open("w", encoding="utf-8") as f:
        json.dump(data, f, indent=2, ensure_ascii=False)
    print(f"Wrote manifest.json to: {manifest_path}")


def add_packages(manifest: dict, packages: dict) -> (dict, dict):
    """
    Add packages to the manifest dependencies dict if missing.
    Returns (new_manifest, added_packages)
    """
    deps = manifest.get("dependencies", {})
    added = {}
    for pkg, ver in packages.items():
        if pkg in deps:
            # Respect existing version; do not overwrite
            continue
        deps[pkg] = ver
        added[pkg] = ver
    manifest["dependencies"] = deps
    return manifest, added


def parse_args(argv):
    dry_run = False
    args = [a for a in argv[1:] if a != "--dry-run" and a != "-n"]
    if "--dry-run" in argv or "-n" in argv:
        dry_run = True
    project_path = (
        Path(args[0]).expanduser().resolve() if args else Path(os.getcwd()).resolve()
    )
    return project_path, dry_run


def main():
    project_path, dry_run = parse_args(sys.argv)
    print("Project path:", project_path)
    print("Dry run:", dry_run)
    manifest_path = project_path / "Packages" / "manifest.json"
    backup_root = project_path / "ToolsManifestBackups"
    # Load
    manifest = load_manifest(manifest_path)
    # Backup
    backup_manifest(manifest_path, backup_root)
    # Add packages
    new_manifest, added = add_packages(manifest, RECOMMENDED_PACKAGES)
    if not added:
        print(
            "No packages needed to be added. manifest.json already contains recommended entries."
        )
        if dry_run:
            print("Dry-run complete.")
        else:
            print("No changes made.")
        return 0
    # Report
    print("Packages to be added:")
    for k, v in added.items():
        print(f"  {k}: {v}")
    if dry_run:
        print("Dry-run enabled; manifest.json not written.")
        return 0
    # Write out new manifest
    write_manifest(manifest_path, new_manifest)
    print(
        "\nDone. Open the project in Unity and allow Package Manager to restore packages."
    )
    print(
        "If Unity reports version suggestions, prefer the verified versions offered by the Editor."
    )
    return 0


if __name__ == "__main__":
    try:
        exit_code = main()
    except KeyboardInterrupt:
        print("\nInterrupted by user.")
        exit_code = 130
    sys.exit(exit_code)
