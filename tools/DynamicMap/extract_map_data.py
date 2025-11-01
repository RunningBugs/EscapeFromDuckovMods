#!/usr/bin/env python3
"""
Extract minimap metadata and simple points of interest from a Duckov AssetRipper export.

Outputs a JSON payload plus the referenced minimap textures so the static viewer in
`tools/DynamicMap/site` can run on GitHub Pages (or any static file host).
"""

from __future__ import annotations

import argparse
import csv
import datetime as dt
import json
import math
import os
import re
import shutil
import struct
import sys
from dataclasses import dataclass
from typing import Dict, Iterable, List, Optional, Tuple


MINIMAP_SETTINGS_GUID = "d551df320acceeb317a9e97502ade12f"
MINIMAP_SETTINGS_FILE_ID = -1857372209
SIMPLE_POI_FILE_ID = 1147714721

_REF_RE = re.compile(
    r"\{fileID:\s*(-?\d+)(?:,\s*guid:\s*([0-9a-f]{32}))?(?:,\s*type:\s*(\d+))?\}",
    re.IGNORECASE,
)
_VECTOR3_RE = re.compile(
    r"\{x:\s*([-\d.eE]+),\s*y:\s*([-\d.eE]+),\s*z:\s*([-\d.eE]+)\}"
)
_VECTOR4_RE = re.compile(
    r"\{x:\s*([-\d.eE]+),\s*y:\s*([-\d.eE]+),\s*z:\s*([-\d.eE]+),\s*w:\s*([-\d.eE]+)\}"
)
_COLOR_RE = re.compile(
    r"\{r:\s*([-\d.eE]+),\s*g:\s*([-\d.eE]+),\s*b:\s*([-\d.eE]+),\s*a:\s*([-\d.eE]+)\}"
)


@dataclass
class TransformData:
    transform_id: int
    game_object_id: Optional[int]
    parent_transform_id: Optional[int]
    local_position: Tuple[float, float, float]
    local_rotation: Tuple[float, float, float, float]
    local_scale: Tuple[float, float, float]


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Generate static map data from a Duckov AssetRipper export."
    )
    parser.add_argument(
        "export_root",
        help="Path to the AssetRipper ExportedProject root (must contain an Assets/ folder).",
    )
    parser.add_argument(
        "--out",
        default="tools/DynamicMap/site",
        help="Destination directory for the static site (default: %(default)s).",
    )
    parser.add_argument(
        "--lang",
        default="en",
        help="Preferred localization language code (default: %(default)s).",
    )
    parser.add_argument(
        "--rotation-cw",
        type=float,
        default=45.0,
        help="Clockwise rotation (in degrees) to align minimap textures with in-game minimap orientation (default: %(default)s).",
    )
    return parser.parse_args()


def parse_localization(
    export_root: str, languages: Iterable[str]
) -> Dict[str, Dict[str, str]]:
    loc_dir = os.path.join(export_root, "Assets", "StreamingAssets", "Localization")
    result: Dict[str, Dict[str, str]] = {}
    for lang in languages:
        fname = {
            "en": "English.csv",
            "zh": "ChineseSimplified.csv",
        }.get(lang, f"{lang}.csv")
        path = os.path.join(loc_dir, fname)
        mapping: Dict[str, str] = {}
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
                            mapping[key] = text
            except Exception:
                pass
        result[lang] = mapping
    return result


def build_guid_map(export_root: str) -> Dict[str, str]:
    mapping: Dict[str, str] = {}
    assets_root = os.path.join(export_root, "Assets")
    for dirpath, _, filenames in os.walk(assets_root):
        for fname in filenames:
            if not fname.endswith(".meta"):
                continue
            meta_path = os.path.join(dirpath, fname)
            try:
                with open(meta_path, "r", encoding="utf-8", errors="ignore") as fh:
                    for i, line in enumerate(fh):
                        if line.startswith("guid: "):
                            guid = line.split(":", 1)[1].strip()
                            mapping[guid] = meta_path[:-5]  # strip .meta
                            break
                        if i > 8:
                            break
            except Exception:
                continue
    return mapping


def parse_reference(value: str) -> Dict[str, Optional[str]]:
    match = _REF_RE.search(value)
    if not match:
        return {"fileID": None, "guid": None, "type": None}
    file_id, guid, type_part = match.groups()
    return {
        "fileID": int(file_id),
        "guid": guid,
        "type": int(type_part) if type_part is not None else None,
    }


def parse_vector3(value: str) -> Tuple[float, float, float]:
    match = _VECTOR3_RE.search(value)
    if not match:
        return (0.0, 0.0, 0.0)
    return tuple(float(part) for part in match.groups())  # type: ignore[return-value]


def parse_quaternion(value: str) -> Tuple[float, float, float, float]:
    match = _VECTOR4_RE.search(value)
    if not match:
        return (0.0, 0.0, 0.0, 1.0)
    x, y, z, w = (float(part) for part in match.groups())
    return normalize_quaternion((x, y, z, w))


def parse_color(value: str) -> Tuple[float, float, float, float]:
    match = _COLOR_RE.search(value)
    if not match:
        return (1.0, 1.0, 1.0, 1.0)
    return tuple(float(part) for part in match.groups())  # type: ignore[return-value]


def normalize_quaternion(
    q: Tuple[float, float, float, float],
) -> Tuple[float, float, float, float]:
    x, y, z, w = q
    mag_sq = x * x + y * y + z * z + w * w
    if mag_sq <= 1e-12:
        return (0.0, 0.0, 0.0, 1.0)
    inv_mag = 1.0 / math.sqrt(mag_sq)
    return (x * inv_mag, y * inv_mag, z * inv_mag, w * inv_mag)


def quaternion_multiply(
    q1: Tuple[float, float, float, float],
    q2: Tuple[float, float, float, float],
) -> Tuple[float, float, float, float]:
    x1, y1, z1, w1 = q1
    x2, y2, z2, w2 = q2
    x = w1 * x2 + x1 * w2 + y1 * z2 - z1 * y2
    y = w1 * y2 - x1 * z2 + y1 * w2 + z1 * x2
    z = w1 * z2 + x1 * y2 - y1 * x2 + z1 * w2
    w = w1 * w2 - x1 * x2 - y1 * y2 - z1 * z2
    return normalize_quaternion((x, y, z, w))


def cross(
    a: Tuple[float, float, float], b: Tuple[float, float, float]
) -> Tuple[float, float, float]:
    ax, ay, az = a
    bx, by, bz = b
    return (
        ay * bz - az * by,
        az * bx - ax * bz,
        ax * by - ay * bx,
    )


def rotate_vector(
    q: Tuple[float, float, float, float], v: Tuple[float, float, float]
) -> Tuple[float, float, float]:
    q = normalize_quaternion(q)
    x, y, z, w = q
    u = (x, y, z)
    uv = cross(u, v)
    uuv = cross(u, uv)
    uv = tuple(component * (2.0 * w) for component in uv)
    uuv = tuple(component * 2.0 for component in uuv)
    return (
        v[0] + uv[0] + uuv[0],
        v[1] + uv[1] + uuv[1],
        v[2] + uv[2] + uuv[2],
    )


def parse_file_id(value: str) -> Optional[int]:
    match = _REF_RE.search(value)
    if not match:
        return None
    return int(match.group(1))


def get_block(lines: List[str], start: int) -> Tuple[List[str], int]:
    block: List[str] = []
    idx = start
    block.append(lines[idx])
    idx += 1
    while idx < len(lines) and not lines[idx].startswith("--- !u!"):
        block.append(lines[idx])
        idx += 1
    return block, idx


def parse_block_header(line: str) -> Tuple[int, int]:
    line = line.strip()
    parts = line.split()
    if len(parts) < 3:
        return (0, 0)
    class_part = parts[1]
    id_part = parts[2]
    class_id = int(class_part[3:])  # skip "!u!"
    file_id = int(id_part[1:])  # skip '&'
    return class_id, file_id


def collect_transforms_and_components(
    lines: List[str],
) -> Tuple[Dict[int, TransformData], Dict[int, int], Dict[int, int]]:
    transforms: Dict[int, TransformData] = {}
    gameobject_to_transform: Dict[int, int] = {}
    component_to_gameobject: Dict[int, int] = {}

    idx = 0
    while idx < len(lines):
        if not lines[idx].startswith("--- !u!"):
            idx += 1
            continue
        block, idx = get_block(lines, idx)
        class_id, file_id = parse_block_header(block[0])

        if class_id == 4:  # Transform
            game_object_id: Optional[int] = None
            parent_id: Optional[int] = None
            local_position = (0.0, 0.0, 0.0)
            local_rotation = (0.0, 0.0, 0.0, 1.0)
            local_scale = (1.0, 1.0, 1.0)
            for line in block:
                stripped = line.strip()
                if stripped.startswith("m_GameObject:"):
                    game_object_id = parse_file_id(stripped)
                elif stripped.startswith("m_Father:"):
                    parent_id = parse_file_id(stripped)
                elif stripped.startswith("m_LocalPosition:"):
                    local_position = parse_vector3(stripped.split(":", 1)[1])
                elif stripped.startswith("m_LocalRotation:"):
                    local_rotation = parse_quaternion(stripped.split(":", 1)[1])
                elif stripped.startswith("m_LocalScale:"):
                    local_scale = parse_vector3(stripped.split(":", 1)[1])
            transforms[file_id] = TransformData(
                transform_id=file_id,
                game_object_id=game_object_id,
                parent_transform_id=parent_id if parent_id and parent_id != 0 else None,
                local_position=local_position,
                local_rotation=local_rotation,
                local_scale=local_scale,
            )
            if game_object_id is not None:
                gameobject_to_transform[game_object_id] = file_id
        elif class_id == 212:  # SpriteRenderer component
            game_object_id = None
            for line in block:
                stripped = line.strip()
                if stripped.startswith("m_GameObject:"):
                    game_object_id = parse_file_id(stripped)
                    break
            if game_object_id is not None:
                component_to_gameobject[file_id] = game_object_id
    return transforms, gameobject_to_transform, component_to_gameobject


def compute_world_transform(
    transform_id: int,
    transforms: Dict[int, TransformData],
    cache: Dict[
        int,
        Tuple[
            Tuple[float, float, float],
            Tuple[float, float, float, float],
            Tuple[float, float, float],
        ],
    ],
) -> Tuple[
    Tuple[float, float, float],
    Tuple[float, float, float, float],
    Tuple[float, float, float],
]:
    if transform_id in cache:
        return cache[transform_id]

    data = transforms.get(transform_id)
    if data is None:
        result = ((0.0, 0.0, 0.0), (0.0, 0.0, 0.0, 1.0), (1.0, 1.0, 1.0))
        cache[transform_id] = result
        return result

    if data.parent_transform_id is None:
        result = (
            data.local_position,
            data.local_rotation,
            data.local_scale,
        )
        cache[transform_id] = result
        return result

    parent_pos, parent_rot, parent_scale = compute_world_transform(
        data.parent_transform_id, transforms, cache
    )

    scaled_local = (
        parent_scale[0] * data.local_position[0],
        parent_scale[1] * data.local_position[1],
        parent_scale[2] * data.local_position[2],
    )
    rotated = rotate_vector(parent_rot, scaled_local)
    world_pos = (
        parent_pos[0] + rotated[0],
        parent_pos[1] + rotated[1],
        parent_pos[2] + rotated[2],
    )
    world_rot = quaternion_multiply(parent_rot, data.local_rotation)
    world_scale = (
        parent_scale[0] * data.local_scale[0],
        parent_scale[1] * data.local_scale[1],
        parent_scale[2] * data.local_scale[2],
    )
    result = (world_pos, world_rot, world_scale)
    cache[transform_id] = result
    return result


def compute_world_position(
    transform_id: int,
    transforms: Dict[int, TransformData],
    cache: Dict[
        int,
        Tuple[
            Tuple[float, float, float],
            Tuple[float, float, float, float],
            Tuple[float, float, float],
        ],
    ],
) -> Optional[Tuple[float, float, float]]:
    if transform_id not in transforms:
        return None
    world_pos, _, _ = compute_world_transform(transform_id, transforms, cache)
    return world_pos


def read_sprite_metadata(
    sprite_guid: str,
    guid_map: Dict[str, str],
) -> Optional[Dict[str, Optional[str]]]:
    asset_path = guid_map.get(sprite_guid)
    if not asset_path or not os.path.isfile(asset_path):
        return None

    sprite_name: Optional[str] = None
    texture_ref: Optional[Dict[str, Optional[str]]] = None

    try:
        with open(asset_path, "r", encoding="utf-8", errors="ignore") as fh:
            for line in fh:
                stripped = line.strip()
                if stripped.startswith("m_Name:"):
                    sprite_name = stripped.split(":", 1)[1].strip()
                elif stripped.startswith("texture:"):
                    texture_ref = parse_reference(stripped.split(":", 1)[1])
                    break
    except Exception:
        pass

    return {
        "path": asset_path,
        "name": sprite_name,
        "texture": texture_ref,
    }


def read_png_dimensions(path: str) -> Optional[Tuple[int, int]]:
    try:
        with open(path, "rb") as fh:
            header = fh.read(24)
            if len(header) < 24:
                return None
            if header[:8] != b"\x89PNG\r\n\x1a\n":
                return None
            width = struct.unpack(">I", header[16:20])[0]
            height = struct.unpack(">I", header[20:24])[0]
            return width, height
    except Exception:
        return None
    return None


def normalize_scene_path(export_root: str, scene_path: str) -> str:
    return os.path.relpath(scene_path, export_root).replace(os.sep, "/")


def parse_minimap_settings_block(
    block: List[str],
) -> Dict[str, object]:
    data: Dict[str, object] = {
        "maps": [],
        "combinedCenter": None,
        "combinedSize": None,
        "combinedSprite": None,
    }
    current: Optional[Dict[str, object]] = None

    for line in block:
        stripped = line.strip()

        if stripped.startswith("- imageWorldSize:"):
            value = stripped.split(":", 1)[1].strip()
            try:
                current = {"imageWorldSize": float(value)}
            except ValueError:
                current = {"imageWorldSize": None}
            casted = data["maps"]
            if isinstance(casted, list):
                casted.append(current)
            else:
                data["maps"] = [current]
        elif current is not None:
            if stripped.startswith("sceneID:"):
                current["sceneID"] = stripped.split(":", 1)[1].strip()
            elif stripped.startswith("sprite:"):
                current["sprite"] = parse_reference(stripped.split(":", 1)[1])
            elif stripped.startswith("offsetReference:"):
                current["offsetReference"] = parse_reference(stripped.split(":", 1)[1])
            elif stripped.startswith("mapWorldCenter:"):
                current["mapWorldCenter"] = parse_vector3(stripped.split(":", 1)[1])
            elif stripped.startswith("hide:"):
                current["hide"] = stripped.split(":", 1)[1].strip() in {
                    "1",
                    "true",
                    "True",
                }
            elif stripped.startswith("noSignal:"):
                current["noSignal"] = stripped.split(":", 1)[1].strip() in {
                    "1",
                    "true",
                    "True",
                }
        else:
            if stripped.startswith("combinedCenter:"):
                data["combinedCenter"] = parse_vector3(stripped.split(":", 1)[1])
            elif stripped.startswith("combinedSize:"):
                try:
                    data["combinedSize"] = float(stripped.split(":", 1)[1].strip())
                except ValueError:
                    data["combinedSize"] = None
            elif stripped.startswith("combinedSprite:"):
                data["combinedSprite"] = parse_reference(stripped.split(":", 1)[1])

    return data


def parse_simple_poi_block(block: List[str]) -> Dict[str, object]:
    data: Dict[str, object] = {
        "gameObjectId": None,
        "icon": None,
        "color": None,
        "shadowColor": None,
        "shadowDistance": None,
        "displayName": "",
        "followActiveScene": False,
        "overrideSceneID": "",
        "isArea": False,
        "areaRadius": 0.0,
        "scaleFactor": 1.0,
        "hideIcon": False,
    }
    for line in block:
        stripped = line.strip()
        if stripped.startswith("m_GameObject:"):
            data["gameObjectId"] = parse_file_id(stripped)
        elif stripped.startswith("icon:"):
            data["icon"] = parse_reference(stripped.split(":", 1)[1])
        elif stripped.startswith("color:"):
            data["color"] = parse_color(stripped.split(":", 1)[1])
        elif stripped.startswith("shadowColor:"):
            data["shadowColor"] = parse_color(stripped.split(":", 1)[1])
        elif stripped.startswith("shadowDistance:"):
            try:
                data["shadowDistance"] = float(stripped.split(":", 1)[1].strip())
            except ValueError:
                data["shadowDistance"] = None
        elif stripped.startswith("displayName:"):
            data["displayName"] = stripped.split(":", 1)[1].strip()
        elif stripped.startswith("followActiveScene:"):
            data["followActiveScene"] = stripped.split(":", 1)[1].strip() in {
                "1",
                "true",
                "True",
            }
        elif stripped.startswith("overrideSceneID:"):
            data["overrideSceneID"] = stripped.split(":", 1)[1].strip()
        elif stripped.startswith("isArea:"):
            data["isArea"] = stripped.split(":", 1)[1].strip() in {"1", "true", "True"}
        elif stripped.startswith("areaRadius:"):
            try:
                data["areaRadius"] = float(stripped.split(":", 1)[1].strip())
            except ValueError:
                data["areaRadius"] = 0.0
        elif stripped.startswith("scaleFactor:"):
            try:
                data["scaleFactor"] = float(stripped.split(":", 1)[1].strip())
            except ValueError:
                data["scaleFactor"] = 1.0
        elif stripped.startswith("hideIcon:"):
            data["hideIcon"] = stripped.split(":", 1)[1].strip() in {
                "1",
                "true",
                "True",
            }
    return data


def collect_scene_data(
    scene_path: str,
    lines: List[str],
    transforms: Dict[int, TransformData],
    go_to_transform: Dict[int, int],
    component_to_gameobject: Dict[int, int],
) -> Tuple[List[Dict[str, object]], List[Dict[str, object]]]:
    map_settings_blocks: List[Dict[str, object]] = []
    poi_entries: List[Dict[str, object]] = []
    transform_cache: Dict[
        int,
        Tuple[
            Tuple[float, float, float],
            Tuple[float, float, float, float],
            Tuple[float, float, float],
        ],
    ] = {}

    idx = 0
    while idx < len(lines):
        if not lines[idx].startswith("--- !u!"):
            idx += 1
            continue
        block, idx = get_block(lines, idx)
        class_id, _ = parse_block_header(block[0])
        if class_id != 114:
            continue

        script_guid = None
        script_file_id = None
        for line in block:
            stripped = line.strip()
            if stripped.startswith("m_Script:"):
                ref = parse_reference(stripped.split(":", 1)[1])
                script_guid = ref.get("guid")
                script_file_id = ref.get("fileID")
                break

        if script_guid != MINIMAP_SETTINGS_GUID or script_file_id is None:
            continue

        if script_file_id == MINIMAP_SETTINGS_FILE_ID:
            map_settings_blocks.append(parse_minimap_settings_block(block))
        elif script_file_id == SIMPLE_POI_FILE_ID:
            entry = parse_simple_poi_block(block)
            go_id = entry.get("gameObjectId")
            if isinstance(go_id, int):
                transform_id = go_to_transform.get(go_id)
                if transform_id is not None:
                    world = compute_world_position(
                        transform_id, transforms, transform_cache
                    )
                    entry["worldPosition"] = world
            poi_entries.append(entry)

    # enrich POIs with fallback scene IDs from map settings
    scene_ids: List[str] = []
    for settings in map_settings_blocks:
        for entry in settings.get("maps", []):
            if isinstance(entry, dict):
                scene_id = entry.get("sceneID")
                if isinstance(scene_id, str) and scene_id:
                    scene_ids.append(scene_id)
    scene_ids = sorted(set(scene_ids))

    for entry in poi_entries:
        override = entry.get("overrideSceneID")
        scene_targets: List[str] = []
        if isinstance(override, str) and override:
            scene_targets.append(override)
        if not scene_targets:
            scene_targets.extend(scene_ids)
        if not scene_targets:
            scene_name = os.path.splitext(os.path.basename(scene_path))[0]
            scene_targets.append(scene_name)
        entry["sceneIds"] = sorted(set(scene_targets))

    # resolve offset references for maps
    for settings in map_settings_blocks:
        for entry in settings.get("maps", []):
            if not isinstance(entry, dict):
                continue
            offset_ref = entry.get("offsetReference")
            if not isinstance(offset_ref, dict):
                continue
            offset_component_id = offset_ref.get("fileID")
            if not isinstance(offset_component_id, int):
                continue
            go_id = component_to_gameobject.get(offset_component_id)
            if go_id is None:
                continue
            transform_id = go_to_transform.get(go_id)
            if transform_id is None:
                continue
            transform_data = transforms.get(transform_id)
            if transform_data:
                entry["offset"] = (
                    transform_data.local_position[0],
                    transform_data.local_position[1],
                )

    return map_settings_blocks, poi_entries


def localize_text(
    key: str,
    localization: Dict[str, Dict[str, str]],
    preferred_lang: str,
) -> str:
    if not key:
        return ""
    langs = [preferred_lang]
    # Always fall back to English if available.
    if preferred_lang != "en":
        langs.append("en")
    for lang in langs:
        table = localization.get(lang)
        if table and key in table:
            return table[key]
    return key


def ensure_directory(path: str) -> None:
    os.makedirs(path, exist_ok=True)


def remove_directory(path: str) -> None:
    if os.path.isdir(path):
        shutil.rmtree(path)


def sanitize_filename(name: str) -> str:
    safe = re.sub(r"[^A-Za-z0-9_.-]", "_", name)
    return safe or "unnamed"


def main() -> None:
    args = parse_args()
    export_root = os.path.abspath(args.export_root)
    if not os.path.isdir(os.path.join(export_root, "Assets")):
        print(
            f"[ERR] {export_root} does not look like an AssetRipper ExportedProject.",
            file=sys.stderr,
        )
        sys.exit(1)

    out_root = os.path.abspath(args.out)
    data_dir = os.path.join(out_root, "data")
    maps_asset_dir = os.path.join(out_root, "assets", "maps")

    ensure_directory(out_root)
    ensure_directory(data_dir)
    remove_directory(maps_asset_dir)
    ensure_directory(maps_asset_dir)

    localization = parse_localization(export_root, languages=[args.lang, "en"])
    guid_map = build_guid_map(export_root)

    maps_output: List[Dict[str, object]] = []
    markers_output: List[Dict[str, object]] = []
    texture_destinations: Dict[str, str] = {}

    scenes_root = os.path.join(export_root, "Assets", "Scenes")
    scene_file_paths: List[str] = []
    for dirpath, _, filenames in os.walk(scenes_root):
        for fname in filenames:
            if fname.endswith(".unity"):
                scene_file_paths.append(os.path.join(dirpath, fname))
    scene_file_paths.sort()

    for scene_path in scene_file_paths:
        with open(scene_path, "r", encoding="utf-8", errors="ignore") as fh:
            lines = fh.readlines()
        transforms, go_to_transform, component_to_go = (
            collect_transforms_and_components(lines)
        )
        map_blocks, poi_entries = collect_scene_data(
            scene_path, lines, transforms, go_to_transform, component_to_go
        )

        scene_rel_path = normalize_scene_path(export_root, scene_path)

        for settings in map_blocks:
            for entry in settings.get("maps", []):
                if not isinstance(entry, dict):
                    continue
                sprite_ref = entry.get("sprite")
                if not isinstance(sprite_ref, dict):
                    continue
                sprite_guid = sprite_ref.get("guid")
                if not sprite_guid:
                    continue
                sprite_meta = read_sprite_metadata(sprite_guid, guid_map)
                if not sprite_meta:
                    continue
                texture_ref = sprite_meta.get("texture")
                texture_guid = None
                if isinstance(texture_ref, dict):
                    texture_guid = texture_ref.get("guid")
                texture_path = guid_map.get(texture_guid) if texture_guid else None
                texture_width = None
                texture_height = None
                dest_rel_path = None
                if texture_path and os.path.isfile(texture_path):
                    dims = read_png_dimensions(texture_path)
                    if dims:
                        texture_width, texture_height = dims
                    dest_filename = sanitize_filename(
                        entry.get("sceneID") or os.path.basename(texture_path)
                    )
                    if not dest_filename.lower().endswith(".png"):
                        dest_filename = f"{dest_filename}.png"
                    dest_rel_path = os.path.join("assets", "maps", dest_filename)
                    if texture_path not in texture_destinations:
                        texture_destinations[texture_path] = dest_rel_path
                pixel_size = None
                image_world_size = entry.get("imageWorldSize")
                if isinstance(image_world_size, (int, float)) and texture_width:
                    pixel_size = float(image_world_size) / float(texture_width)
                maps_output.append(
                    {
                        "sceneId": entry.get("sceneID"),
                        "sourceScene": scene_rel_path,
                        "sourceSceneDir": os.path.dirname(scene_rel_path),
                        "imageWorldSize": image_world_size,
                        "mapWorldCenter": entry.get("mapWorldCenter"),
                        "hide": entry.get("hide", False),
                        "noSignal": entry.get("noSignal", False),
                        "rotationCW": args.rotation_cw,
                        "pixelSize": pixel_size,
                        "sprite": {
                            "guid": sprite_guid,
                            "name": sprite_meta.get("name"),
                            "assetPath": normalize_scene_path(
                                export_root, sprite_meta["path"]
                            )
                            if sprite_meta.get("path")
                            else None,
                        },
                        "texture": {
                            "guid": texture_guid,
                            "width": texture_width,
                            "height": texture_height,
                            "sourcePath": normalize_scene_path(
                                export_root, texture_path
                            )
                            if texture_path
                            else None,
                            "relativePath": dest_rel_path,
                        },
                        "offset": entry.get("offset"),
                    }
                )

        for entry in poi_entries:
            name_key = entry.get("displayName", "")
            localized = localize_text(name_key, localization, args.lang)
            marker_world = entry.get("worldPosition")
            markers_output.append(
                {
                    "name": name_key,
                    "nameLocalized": localized,
                    "sceneIds": entry.get("sceneIds"),
                    "sourceScene": scene_rel_path,
                    "followActiveScene": entry.get("followActiveScene"),
                    "overrideSceneID": entry.get("overrideSceneID"),
                    "isArea": entry.get("isArea"),
                    "areaRadius": entry.get("areaRadius"),
                    "scaleFactor": entry.get("scaleFactor"),
                    "hideIcon": entry.get("hideIcon"),
                    "color": entry.get("color"),
                    "shadowColor": entry.get("shadowColor"),
                    "shadowDistance": entry.get("shadowDistance"),
                    "worldPosition": marker_world,
                    "icon": entry.get("icon"),
                }
            )

    # Copy required textures.
    for src_path, dest_rel in texture_destinations.items():
        dest_path = os.path.join(out_root, dest_rel)
        ensure_directory(os.path.dirname(dest_path))
        shutil.copy2(src_path, dest_path)

    payload = {
        "generatedAt": dt.datetime.utcnow().isoformat(timespec="seconds") + "Z",
        "exportRoot": export_root,
        "maps": maps_output,
        "markers": markers_output,
    }
    json_path = os.path.join(data_dir, "maps.json")
    with open(json_path, "w", encoding="utf-8") as fh:
        json.dump(payload, fh, indent=2)
    print(f"[OK] Wrote {json_path}")
    print(
        f"[OK] Copied {len(texture_destinations)} minimap textures into {maps_asset_dir}"
    )


if __name__ == "__main__":
    main()
