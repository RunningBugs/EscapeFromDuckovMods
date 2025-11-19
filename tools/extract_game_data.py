#!/usr/bin/env python3
import argparse
import csv
import os
import re
from collections import defaultdict
from typing import Dict, List, Set, Optional, Any

# --- configuration ---
# Limit the size of fields content to avoid massive CSV cells for binary data or arrays
MAX_FIELD_LENGTH = 1000 

def load_script_map(export_root: str) -> Dict[str, str]:
    """
    Scans the export_root for .cs.meta and .dll.meta files to map GUIDs to Script Names.
    Returns a dict: { guid: script_name }
    """
    print("Building Script GUID map...")
    guid_map = {}
    # specific folders to speed up? just walk assets.
    assets_root = os.path.join(export_root, "Assets")
    
    for root, dirs, files in os.walk(assets_root):
        for f in files:
            if f.endswith(".cs.meta") or f.endswith(".dll.meta"):
                full_path = os.path.join(root, f)
                try:
                    with open(full_path, "r", encoding="utf-8", errors="ignore") as fh:
                        guid = None
                        for line in fh:
                            if line.strip().startswith("guid:"):
                                guid = line.split(":", 1)[1].strip()
                                break
                        if guid:
                            # getting the script name from the filename (Asset.cs.meta -> Asset)
                            script_name = f.rsplit(".", 2)[0] 
                            guid_map[guid] = script_name
                except Exception:
                    pass
    print(f"Found {len(guid_map)} scripts.")
    return guid_map

def load_asset_path_map(export_root: str) -> Dict[str, str]:
    """
    Maps GUIDs to Asset Paths (relative to export_root).
    """
    print("Building Asset Path map...")
    guid_map = {}
    assets_root = os.path.join(export_root, "Assets")
    
    for root, dirs, files in os.walk(assets_root):
        for f in files:
            if f.endswith(".meta"):
                full_path = os.path.join(root, f)
                try:
                    with open(full_path, "r", encoding="utf-8", errors="ignore") as fh:
                        # checking first few lines for guid
                        for i, line in enumerate(fh):
                            if i > 20: break
                            if line.strip().startswith("guid:"):
                                guid = line.split(":", 1)[1].strip()
                                asset_path = os.path.relpath(full_path[:-5], export_root) # strip .meta
                                guid_map[guid] = asset_path
                                break
                except Exception:
                    pass
    print(f"Found {len(guid_map)} assets.")
    return guid_map

def parse_yaml_block(lines: List[str]) -> Dict[str, Any]:
    """
    Parses a single YAML block (list of lines) into a flat dictionary.
    Handles basic key: value and simple nested structures by flattening keys.
    """
    data = {}
    stack = [] # stack of (indent_level, key_prefix) 
    
    # Regex for "key: value"
    kv_pattern = re.compile(r"^(\s*)([^:]+):\s*(.*)$")
    
    for line in lines:
        if not line.strip(): continue
        if line.strip().startswith("---"): continue 
        
        match = kv_pattern.match(line)
        if match:
            indent_str, key, val = match.groups()
            indent = len(indent_str)
            key = key.strip()
            val = val.strip()
            
            # manage stack for nesting
            while stack and stack[-1][0] >= indent:
                stack.pop()
            
            prefix = ""
            if stack:
                prefix = stack[-1][1] + "."
            
            full_key = prefix + key
            
            # If val is empty, it might be a parent object start
            if not val:
                stack.append((indent, full_key))
            else:
                # Clean value (remove {} if empty object, handle guids and fileIDs)
                if val.startswith("{"):
                    if "guid:" in val:
                        # extract guid
                        g_match = re.search(r"guid:\s*([a-f0-9]+)", val)
                        if g_match:
                            data[full_key + "_guid"] = g_match.group(1)
                    if "fileID:" in val:
                        # extract fileID
                        f_match = re.search(r"fileID:\s*(-?\d+)", val)
                        if f_match:
                            data[full_key + "_fileID"] = f_match.group(1)
                
                data[full_key] = val
        else:
            # Handle list items or other formats lightly?
            # For now, we skip complex list parsing to keep it simple, 
            # or append to previous key if it looks like a continuation
            pass
            
    return data

def process_file(file_path: str, export_root: str, script_map: Dict[str, str]) -> List[Dict]:
    """
    Reads a file, extracts MonoBehaviours, and returns a list of data dicts.
    """
    results = []
    try:
        with open(file_path, "r", encoding="utf-8", errors="ignore") as fh:
            lines = fh.readlines()
    except Exception:
        return []

    # Identify blocks
    blocks = []
    current_block = []
    current_type = None
    
    # Simple state machine to find --- !u!114 (MonoBehaviour)
    for line in lines:
        if line.startswith("--- !u!"):
            if current_block:
                blocks.append((current_type, current_block))
            current_block = []
            # extract type ID
            type_match = re.search(r"!u!(\d+)", line)
            if type_match:
                current_type = type_match.group(1)
            else:
                current_type = "unknown"
        current_block.append(line)
    if current_block:
        blocks.append((current_type, current_block))
        
    # We need to map FileID -> GameObject Name for context
    # But in a single file, we can usually find the GameObject matching the MonoBehaviour
    # For simplicity, we extract the m_Name from the MonoBehaviour itself if present,
    # or from the GameObject block if we can link them. 
    # In prefabs, MBs are linked to GOs.
    
    # Pass 1: Find GameObjects and their names (FileID -> Name)
    # &12345 is the anchor (FileID)
    file_id_to_name = {}
    
    for b_type, b_lines in blocks:
        if b_type == "1": # GameObject
            # extract anchor
            anchor = None
            name = "Unknown"
            for l in b_lines:
                if l.startswith("--- !u!1"):
                    am = re.search(r"&(\d+)", l)
                    if am: anchor = am.group(1)
                if "m_Name:" in l:
                    name = l.split(":", 1)[1].strip()
            if anchor:
                file_id_to_name[anchor] = name

    # Pass 2: Parse MonoBehaviours
    rel_path = os.path.relpath(file_path, export_root)
    
    for b_type, b_lines in blocks:
        if b_type == "114": # MonoBehaviour
            data = parse_yaml_block(b_lines)
            
            # Helper to find key ending with suffix
            def get_val(suffix):
                for k, v in data.items():
                    if k == suffix or k.endswith("." + suffix):
                        return v
                return None

            script_guid = get_val("m_Script_guid")
            script_file_id = get_val("m_Script_fileID")
            
            if not script_guid or not script_file_id:
                # try parsing raw m_Script line if regex failed or missing
                for l in b_lines:
                    if "m_Script:" in l:
                        if "guid:" in l and not script_guid:
                            gm = re.search(r"guid:\s*([a-f0-9]+)", l)
                            if gm:
                                script_guid = gm.group(1)
                        if "fileID:" in l and not script_file_id:
                            fm = re.search(r"fileID:\s*(-?\d+)", l)
                            if fm:
                                script_file_id = fm.group(1)
                        if script_guid and script_file_id: break

            if not script_guid:
                continue
                
            script_name = script_map.get(script_guid, script_guid) # fallback to guid
            
            # Use pure script name for grouping
            script_identifier = script_name

            # Attempt to find name
            # 1. m_Name in block
            obj_name = data.get("m_Name")
            
            # 2. Link to GameObject
            if not obj_name or obj_name == "":
                go_ref = data.get("m_GameObject.fileID")
                if go_ref and go_ref in file_id_to_name:
                    obj_name = file_id_to_name[go_ref]
            
            # 3. Fallback to filename
            if not obj_name:
                obj_name = os.path.splitext(os.path.basename(file_path))[0]
            
            entry = {
                "_SourceFile": rel_path,
                "_ObjectName": obj_name,
                "_Script": script_identifier,
                "_ScriptGUID": script_guid,
                "_ScriptFileID": script_file_id
            }
            entry.update(data)
            results.append(entry)
            
    return results

def main():
    parser = argparse.ArgumentParser(description="Extract Game Object Data to CSVs")
    parser.add_argument("export_root", help="Path to AssetRipper ExportedProject root")
    parser.add_argument("output_dir", help="Directory to save CSV files")
    args = parser.parse_args()

    if not os.path.exists(args.output_dir):
        os.makedirs(args.output_dir)

    script_map = load_script_map(args.export_root)
    # asset_map = load_asset_path_map(args.export_root) # Optional: Resolving every guid is expensive, maybe do it on demand?
    # Let's do it, it's useful.
    asset_map = load_asset_path_map(args.export_root)

    all_data = defaultdict(list) # script_name -> list of dicts
    
    # Directories to scan
    scan_dirs = [
        os.path.join(args.export_root, "Assets", "MonoBehaviour"),
        os.path.join(args.export_root, "Assets", "GameObject"),
        os.path.join(args.export_root, "Assets", "Prefabs"), # Just in case
    ]
    
    files_processed = 0
    for scan_dir in scan_dirs:
        if not os.path.exists(scan_dir): continue
        for root, _, files in os.walk(scan_dir):
            for f in files:
                if f.endswith((".asset", ".prefab")):
                    full_path = os.path.join(root, f)
                    entries = process_file(full_path, args.export_root, script_map)
                    for e in entries:
                        # Post-process: Resolve GUIDs in values
                        updates = {}
                        for k, v in e.items():
                            if k.endswith("_guid") and v in asset_map:
                                # add a resolved path field
                                base_key = k[:-5] # remove _guid
                                updates[base_key + "_path"] = asset_map[v]
                        e.update(updates)
                        
                        script_key = e["_Script"]
                        all_data[script_key].append(e)
                    files_processed += 1
                    if files_processed % 100 == 0:
                        print(f"Processed {files_processed} files...", end='\r')
    
    print(f"\nProcessed {files_processed} files. Generating CSVs...")

    # Write CSVs
    
    # 1. The Big CSV
    print("Generating All_Game_Objects.csv...")
    all_rows = []
    all_keys = set()
    for rows in all_data.values():
        all_rows.extend(rows)
        for r in rows:
            all_keys.update(r.keys())
            
    sorted_all_keys = sorted(list(all_keys))
    context_keys = ["_ObjectName", "_SourceFile", "_Script", "_ScriptGUID", "_ScriptFileID"]
    for k in reversed(context_keys):
        if k in sorted_all_keys:
            sorted_all_keys.remove(k)
            sorted_all_keys.insert(0, k)
            
    big_csv_path = os.path.join(args.output_dir, "All_Game_Objects.csv")
    try:
        with open(big_csv_path, "w", encoding="utf-8", newline="") as fh:
            writer = csv.DictWriter(fh, fieldnames=sorted_all_keys)
            writer.writeheader()
            writer.writerows(all_rows)
    except Exception as e:
        print(f"Error writing All_Game_Objects.csv: {e}")

    # 2. Partitioned CSVs (by Script Name only)
    print("Generating partitioned CSVs...")
    
    # Regroup by Script Name (stripping any fileID suffix logic if it existed, 
    # though we removed it in process_file, let's ensure we group purely by map name)
    
    for script_key, rows in all_data.items():
        if not rows: continue
        
        # filename safety
        safe_name = "".join([c if c.isalnum() else "_" for c in script_key])
        out_path = os.path.join(args.output_dir, f"{safe_name}.csv")
        
        # Collect all keys for this group
        keys = set()
        for r in rows:
            keys.update(r.keys())
        
        # Sort keys
        sorted_keys = sorted(list(keys))
        for k in reversed(context_keys):
            if k in sorted_keys:
                sorted_keys.remove(k)
                sorted_keys.insert(0, k)
        
        try:
            with open(out_path, "w", encoding="utf-8", newline="") as fh:
                writer = csv.DictWriter(fh, fieldnames=sorted_keys)
                writer.writeheader()
                writer.writerows(rows)
        except Exception as e:
            print(f"Error writing {out_path}: {e}")
            
    print(f"Done! Output saved to {args.output_dir}")

if __name__ == "__main__":
    main()
