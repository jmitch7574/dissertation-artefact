import json
import sys
from pathlib import Path
from statistics import median

FILE_PATH  = "../OSM Files/lincoln/buildings.geojson"

with open(FILE_PATH, encoding="utf-8") as f:
    data = json.load(f)

height_keys = {
    'height': 1,
    'building:height': 2,
    'overture:height': 3
}

features = data.get("features", [])

building_count = 0
building_no_height_count_lidar = 0
building_no_height_total = 0
total_overture_difference = 0
total_overture_difference_abs = 0
total_overture_differences = []
total_overture_differences_abs = []
total_overture_difference_count = 0

for feature in features:
    props = feature.get("properties") or {}

    if "building" not in props.keys():
        continue

    building_count += 1

    if props["lidar:m_height"] < 2:
        building_no_height_count_lidar += 1

        if not ('height' in props or 'building:height' in props or 'overture:height' in props):
            building_no_height_total += 1

    else:
        if "overture:height" in props:
            total_overture_difference += props["lidar:m_height"] - props["overture:height"]
            total_overture_differences.append(props["lidar:m_height"] - props["overture:height"])
            total_overture_difference_abs += abs(props["lidar:m_height"] - props["overture:height"])
            total_overture_differences_abs.append(abs(props["lidar:m_height"] - props["overture:height"]))
            total_overture_difference_count += 1

print(f"Buildings found   : {building_count:,}")
print(f"LiDAR Height < 1 m      : {building_no_height_count_lidar:,}")
print(f"No Height at all      : {building_no_height_total:,}")
print(f"Mean Overture Difference      : {(total_overture_difference / total_overture_difference_count):,}")
print(f"Median Overture Difference      : {median(total_overture_differences):,}")
print(f"Mean Overture Difference (abs)     : {(total_overture_difference_abs / total_overture_difference_count):,}")
print(f"Median Overture Difference (abs)     : {median(total_overture_differences_abs):,}")