"use strict";

const state = {
  data: null,
  maps: [],
  mapIndex: new Map(),
  currentMap: null,
  currentMarkers: [],
  markerElements: [],
  mapRotation: 0,
};

const mapSelect = document.querySelector("#map-select");
const mapImage = document.querySelector("#map-image");
const markerLayer = document.querySelector("#marker-layer");
const markerListEl = document.querySelector("#marker-list");
const mapMetaEl = document.querySelector("#map-meta");
const generatedMetaEl = document.querySelector("#generated-meta");
const mapRotator = document.querySelector("#map-rotator");

async function bootstrap() {
  try {
    const response = await fetch("data/maps.json", { cache: "no-store" });
    if (!response.ok) {
      throw new Error(`Failed to load maps.json (${response.status})`);
    }
    const payload = await response.json();
    state.data = payload;
    generatedMetaEl.textContent = `Generated at ${payload.generatedAt || "unknown time"}`;
    setupMaps(payload.maps || []);
    setupMarkers(payload.markers || []);
    if (state.maps.length > 0) {
      mapSelect.value = state.maps[0].id;
      selectMap(state.maps[0].id);
    } else {
      mapMetaEl.textContent = "No minimap data available.";
    }
  } catch (err) {
    console.error(err);
    mapMetaEl.textContent =
      "Unable to load map data. Check the console for details.";
  }
}

function setupMaps(maps) {
  state.maps = [];
  state.mapIndex.clear();
  mapSelect.innerHTML = "";

  const usableMaps = maps
    .filter(
      (map) =>
        map &&
        map.texture &&
        (map.texture.relativePath || map.texture.sourcePath) &&
        Array.isArray(map.mapWorldCenter),
    )
    .sort((a, b) => {
      const left = (a.sceneId || a.sprite?.name || "").toLowerCase();
      const right = (b.sceneId || b.sprite?.name || "").toLowerCase();
      return left.localeCompare(right);
    });

  usableMaps.forEach((map) => {
    const id = map.sceneId || map.texture.guid || map.texture.relativePath;
    if (!id) {
      return;
    }
    const displayName =
      map.sceneId ||
      map.sprite?.name ||
      friendlyFromPath(map.texture.relativePath) ||
      "Unnamed map";
    const packed = { ...map, id, displayName };
    state.maps.push(packed);
    state.mapIndex.set(id, packed);

    const option = document.createElement("option");
    option.value = id;
    option.textContent = displayName;
    mapSelect.appendChild(option);
  });
}

function setupMarkers(markers) {
  state.markers = Array.isArray(markers) ? markers : [];
}

function selectMap(mapId) {
  const map = state.mapIndex.get(mapId);
  if (!map) {
    return;
  }
  state.currentMap = map;
  state.currentMarkers = markersForMap(map);
  state.mapRotation = typeof map.rotationCW === "number" ? map.rotationCW : 0;
  if (mapRotator) {
    if (state.mapRotation) {
      mapRotator.style.transform = `rotate(${state.mapRotation}deg)`;
    } else {
      mapRotator.style.transform = "rotate(0deg)";
    }
  }
  updateMapMeta(map);
  renderMarkerList(state.currentMarkers);

  mapImage.dataset.mapId = map.id;
  const texturePath =
    map.texture?.relativePath || map.texture?.sourcePath || "";
  if (texturePath) {
    mapImage.src = texturePath;
    if (mapImage.complete && mapImage.naturalWidth) {
      renderMarkers(map, state.currentMarkers);
    } else {
      clearMarkers();
    }
  } else {
    mapImage.removeAttribute("src");
    clearMarkers();
  }
}

function markersForMap(map) {
  const mapSceneId = map.sceneId;
  const markers = state.markers || [];
  const sourceDir = (map.sourceSceneDir || "").toLowerCase();
  const halfSpan =
    typeof map.imageWorldSize === "number" && map.imageWorldSize > 0
      ? map.imageWorldSize / 2
      : null;
  return markers
    .filter((marker) => {
      if (!Array.isArray(marker.sceneIds) || marker.sceneIds.length === 0) {
        return !mapSceneId;
      }
      return mapSceneId ? marker.sceneIds.includes(mapSceneId) : true;
    })
    .filter((marker) => {
      if (sourceDir && typeof marker.sourceScene === "string") {
        const markerSourceLower = marker.sourceScene.toLowerCase();
        if (!markerSourceLower.startsWith(sourceDir)) {
          return false;
        }
      }
      if (marker.hideIcon) {
        return false;
      }
      if (
        Array.isArray(marker.color) &&
        marker.color.length >= 4 &&
        marker.color[3] <= 0
      ) {
        return false;
      }
      if (!Array.isArray(marker.worldPosition)) {
        return false;
      }
      if (halfSpan == null) {
        return true;
      }
      const [wx, , wz] = marker.worldPosition;
      const [cx, , cz] = map.mapWorldCenter || [];
      if (
        !Number.isFinite(wx) ||
        !Number.isFinite(wz) ||
        !Number.isFinite(cx) ||
        !Number.isFinite(cz)
      ) {
        return true;
      }
      const dx = wx - cx;
      const dz = wz - cz;
      const EPS = 1e-3;
      return Math.abs(dx) <= halfSpan + EPS && Math.abs(dz) <= halfSpan + EPS;
    });
}

function renderMarkers(map, markers) {
  clearMarkers();
  const width = map.texture?.width;
  const height = map.texture?.height;
  if (
    !width ||
    !height ||
    !map.pixelSize ||
    !Array.isArray(map.mapWorldCenter)
  ) {
    return;
  }
  markers.forEach((marker) => {
    const position = projectWorldToMap(marker.worldPosition, map);
    if (!position) {
      return;
    }
    if (
      position.x < 0 ||
      position.y < 0 ||
      position.x > width ||
      position.y > height
    ) {
      return;
    }
    const el = document.createElement("div");
    el.className = "marker";
    el.dataset.x = position.x.toFixed(2);
    el.dataset.y = position.y.toFixed(2);
    const label = marker.nameLocalized || marker.name || "Unknown";
    el.dataset.label = label;
    if (Array.isArray(marker.color)) {
      const colorCss = colorToCss(marker.color);
      el.style.setProperty("--marker-color", colorCss);
      el.style.background = colorCss;
      el.dataset.color = "1";
    }
    el.title = label;
    markerLayer.appendChild(el);
    state.markerElements.push(el);
  });
  updateMarkerPositions();
}

function renderMarkerList(markers) {
  markerListEl.innerHTML = "";
  const sorted = [...markers].sort((a, b) => {
    const left = (a.nameLocalized || a.name || "").toLowerCase();
    const right = (b.nameLocalized || b.name || "").toLowerCase();
    return left.localeCompare(right);
  });

  sorted.forEach((marker) => {
    const li = document.createElement("li");
    const title = document.createElement("strong");
    title.textContent = marker.nameLocalized || marker.name || "Unknown marker";

    const coords = document.createElement("span");
    if (Array.isArray(marker.worldPosition)) {
      const [x, , z] = marker.worldPosition;
      coords.textContent = `${x.toFixed(1)}, ${z.toFixed(1)}`;
    } else {
      coords.textContent = "n/a";
    }

    li.appendChild(title);
    li.appendChild(coords);
    li.title = Array.isArray(marker.sceneIds)
      ? `Scenes: ${marker.sceneIds.join(", ")}`
      : "";
    markerListEl.appendChild(li);
  });
}

function updateMarkerPositions() {
  if (!state.markerElements.length) {
    return;
  }
  const naturalWidth = mapImage.naturalWidth;
  const naturalHeight = mapImage.naturalHeight;
  if (!naturalWidth || !naturalHeight) {
    return;
  }
  const scaleX = mapImage.clientWidth / naturalWidth;
  const scaleY = mapImage.clientHeight / naturalHeight;
  state.markerElements.forEach((el) => {
    const x = parseFloat(el.dataset.x);
    const y = parseFloat(el.dataset.y);
    if (!Number.isFinite(x) || !Number.isFinite(y)) {
      return;
    }
    el.style.left = `${x * scaleX}px`;
    el.style.top = `${y * scaleY}px`;
  });
}

function clearMarkers() {
  markerLayer.innerHTML = "";
  state.markerElements = [];
}

function updateMapMeta(map) {
  const rows = [];
  if (map.displayName) {
    rows.push(`<div><strong>${escapeHtml(map.displayName)}</strong></div>`);
  }
  if (map.texture?.width && map.texture?.height) {
    rows.push(
      `<div>Resolution: ${map.texture.width} × ${map.texture.height} px</div>`,
    );
  }
  if (typeof map.imageWorldSize === "number") {
    rows.push(`<div>World width: ${map.imageWorldSize.toFixed(1)}</div>`);
  }
  if (typeof map.pixelSize === "number") {
    rows.push(`<div>World units / pixel: ${map.pixelSize.toFixed(3)}</div>`);
  }
  if (typeof map.rotationCW === "number" && map.rotationCW !== 0) {
    rows.push(`<div>Rotation: ${map.rotationCW.toFixed(1)}° CW</div>`);
  }
  mapMetaEl.innerHTML = rows.join("") || "<div>No metadata.</div>";
}

function projectWorldToMap(worldPosition, map) {
  if (
    !Array.isArray(worldPosition) ||
    !Array.isArray(map.mapWorldCenter) ||
    typeof map.pixelSize !== "number" ||
    !map.texture
  ) {
    return null;
  }
  const [wx, , wz] = worldPosition;
  const [cx, , cz] = map.mapWorldCenter;
  const dx = wx - cx;
  const dz = wz - cz;
  const width = map.texture.width;
  const height = map.texture.height || width;
  if (!width || !height) {
    return null;
  }
  const pixel = map.pixelSize;
  return {
    x: width / 2 + dx / pixel,
    y: height / 2 - dz / pixel,
  };
}

function friendlyFromPath(path) {
  if (!path) {
    return "";
  }
  const parts = path.split("/");
  return parts[parts.length - 1] || "";
}

function colorToCss(color) {
  const [r, g, b, a = 1] = color;
  const fr = clamp01(r);
  const fg = clamp01(g);
  const fb = clamp01(b);
  const fa = clamp01(a);
  return `rgba(${Math.round(fr * 255)}, ${Math.round(fg * 255)}, ${Math.round(fb * 255)}, ${fa.toFixed(2)})`;
}

function clamp01(value) {
  if (typeof value !== "number" || Number.isNaN(value)) {
    return 0;
  }
  return Math.min(1, Math.max(0, value));
}

function escapeHtml(str) {
  return str
    ? str
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;")
    : "";
}

mapSelect.addEventListener("change", (event) => {
  selectMap(event.target.value);
});

mapImage.addEventListener("load", () => {
  if (!state.currentMap) {
    return;
  }
  if (mapImage.dataset.mapId !== state.currentMap.id) {
    return;
  }
  renderMarkers(state.currentMap, state.currentMarkers);
});

window.addEventListener("resize", updateMarkerPositions);
document.addEventListener("DOMContentLoaded", bootstrap);
