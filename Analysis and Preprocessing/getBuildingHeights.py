"""
extract_building_heights.py

Extracts LiDAR-derived building heights from nDSM rasters and attaches them
to OSM building footprints as a 'lidar:m_height' property.

Key features:
  - Erodes building polygons inward before sampling to avoid edge-pixel noise
  - Correctly handles courtyard buildings (polygons with holes):
      outer walls shrink inward, courtyard holes expand outward
  - Falls back to the original polygon for buildings too small to erode
  - Strips null properties from the output GeoJSON
"""

import json
import numpy as np
import rasterio
import geopandas as gpd
from rasterstats import zonal_stats
from shapely.geometry import Polygon, MultiPolygon
from shapely.ops import unary_union

DSM_PATH     = "../GeoTiff/FZ_DSM_SK9570_P_10742_20181216_20181216.tif"
DTM_PATH     = "../GeoTiff/DTM_SK9570_P_10742_20181216_20181216.tif"
GEOJSON_PATH = "../OSM Files/lincoln/buildings.geojson"
OUTPUT_PATH  = "../OSM Files/lincoln/buildings.geojson"


OFFSET_METRES = 2

def shrink_vertexes(geom, offset_metres):
    """
    Sample the vertexes of a building height such that the vertexes move towards the building's body
      - Outer boundary:           shrinks inward  (negative buffer)
      - Inner boundaries / holes: grow outward    (positive buffer on the hole)

    return the original polygon if shrinked edges collapse
    """
    if geom is None or geom.is_empty:
        return geom

    def single_polygon(poly):
        if not isinstance(poly, Polygon):
            return poly

        eroded_exterior = Polygon(poly.exterior).buffer(-offset_metres)
        if eroded_exterior.is_empty:
            return poly

        expanded_holes = []
        for interior in poly.interiors:
            expanded_hole = Polygon(interior).buffer(offset_metres)
            if not expanded_hole.is_empty:
                expanded_holes.append(expanded_hole)

        if expanded_holes:
            result = eroded_exterior.difference(unary_union(expanded_holes))
        else:
            result = eroded_exterior

        return result if (result is not None and not result.is_empty) else poly

    if isinstance(geom, Polygon):
        return single_polygon(geom)

    elif isinstance(geom, MultiPolygon):
        parts = [single_polygon(part) for part in geom.geoms]
        valid_parts = [p for p in parts if p is not None and not p.is_empty]
        if valid_parts:
            return unary_union(valid_parts)
        return geom  # All parts collapsed — return original

    else:
        # Points, LineStrings, etc. — leave unchanged
        return geom

print("Reading DSM and DTM rasters...")

with rasterio.open(DSM_PATH) as dsm_src:
    dsm_data = dsm_src.read(1)
    affine   = dsm_src.transform
    nodata   = dsm_src.nodata
    raster_crs = dsm_src.crs.to_string()

with rasterio.open(DTM_PATH) as dtm_src:
    dtm_data = dtm_src.read(1)

ndsm = np.where(
    (dsm_data != nodata) & (dtm_data != nodata),
    dsm_data - dtm_data,
    np.nan
)

print("nDSM calculated.")

print(f"Loading building footprints from {GEOJSON_PATH}...")

buildings = gpd.read_file(GEOJSON_PATH)
buildings = buildings.to_crs(raster_crs)

print(f"  {len(buildings)} buildings loaded.")

print(f"Eroding building footprints by {OFFSET_METRES} m (courtyard-aware)...")

sampling_geoms = gpd.GeoSeries(
    [shrink_vertexes(geom, OFFSET_METRES) for geom in buildings.geometry],
    crs=buildings.crs
)

# Report how many fell back to the original polygon
n_fallback = sum(
    s.equals(o)
    for s, o in zip(sampling_geoms, buildings.geometry)
)
print(f"  {n_fallback} buildings were too small to erode and use their original footprint.")


print("Sampling nDSM within eroded footprints...")

stats = zonal_stats(
    sampling_geoms,
    ndsm,
    affine=affine,
    stats="mean",
    nodata=np.nan
)

buildings["lidar:m_height"] = [
    round(s["mean"], 2) if s["mean"] is not None else 0
    for s in stats
]

data = json.loads(buildings.to_json())

# Remove the null entries added by geopandas
for feature in data["features"]:
    feature["properties"] = {
        k: v for k, v in feature["properties"].items() if v is not None
    }

with open(OUTPUT_PATH, "w", encoding="utf-8") as f:
    json.dump(data, f, ensure_ascii=False)

print(f"\nDone. Output saved to {OUTPUT_PATH}")
print(f"  Buildings processed : {len(buildings)}")
print(f"  Offset applied     : {OFFSET_METRES} m")
print(f"  Fallback (no offset): {n_fallback}")