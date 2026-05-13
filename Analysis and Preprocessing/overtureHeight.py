import geopandas as gpd
import json

with open("../OSM Files/lincoln/buildings.geojson", encoding="utf-8") as f:
    osm_data = json.load(f)

with open("../OSM Files/lincoln/lincoln_building_overture.geojson", encoding="utf-8") as f:
    overture_data = json.load(f)


prefix_map = {"w": "way", "r": "relation", "n": "node"}

# Build an overture lookup
overture_heights = {}
for feature in overture_data["features"]:
    height = feature["properties"].get("height")
    if height is None:
        continue
    sources = feature["properties"].get("sources", [])
    for source in sources:
        if source.get("dataset") == "OpenStreetMap":
            record_id = source.get("record_id", "")
            if record_id and record_id[0] in prefix_map:
                prefix = prefix_map[record_id[0]]
                osm_num = record_id[1:].split("@")[0]
                osm_id = f"{prefix}/{osm_num}"
                overture_heights[osm_id] = height
            break

# Apply heights to OSM features by OSM ID shared across both
for feature in osm_data["features"]:
    props = feature["properties"]
    if props.get("height") is not None:
        continue  # already has a height
    osm_id = props.get("@id")
    if osm_id and osm_id in overture_heights:
        props["overture:height"] = overture_heights[osm_id]

# Strip null properties which inflate file size
for feature in osm_data["features"]:
    feature["properties"] = {
        k: v for k, v in feature["properties"].items() if v is not None
    }

with open("../OSM Files/lincoln/buildings.geojson", "w", encoding="utf-8") as f:
    json.dump(osm_data, f, ensure_ascii=False)