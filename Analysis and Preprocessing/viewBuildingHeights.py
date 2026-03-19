import rasterio
import geopandas as gpd
import pandas as pd
import numpy as np
from rasterstats import zonal_stats
import json

# 1. Define File Paths
dsm_path = "../GeoTiff/FZ_DSM_SK9570_P_10742_20181216_20181216.tif"
dtm_path = "../GeoTiff\DTM_SK9570_P_10742_20181216_20181216.tif"  # You need the corresponding DTM
geojson_path = "..\OSM Files\lincoln_bng.geojson"
output_path = "..\OSM Files\lincoln_bng_height.geojson"

# 2. Calculate the nDSM (Normalized Digital Surface Model)
# This represents the actual height of objects above ground
with rasterio.open(dsm_path) as dsm_src:
    dsm_data = dsm_src.read(1)
    affine = dsm_src.transform
    nodata = dsm_src.nodata
    
    with rasterio.open(dtm_path) as dtm_src:
        dtm_data = dtm_src.read(1)
        
    # Subtract DTM from DSM to get height above ground
    # We use np.where to ensure we don't calculate heights on NoData pixels
    ndsm = np.where((dsm_data != nodata) & (dtm_data != nodata), dsm_data - dtm_data, np.nan)

# 3. Load OSM GeoJSON
buildings = gpd.read_file(geojson_path)

# Ensure the CRS (Coordinate Reference System) matches the Raster
with rasterio.open(dsm_path) as src:
    raster_crs = src.crs.to_string()
buildings = buildings.to_crs(raster_crs)

# 4. Extract average height per building polygon
# 'stats="mean"' handles the averaging within the building footprint
stats = zonal_stats(buildings, ndsm, affine=affine, stats="mean", nodata=np.nan)

# 5. Assign to the 'lidar:m_height' field
buildings['lidar:m_height'] = [round(s['mean'], 2) if s['mean'] is not None else 0 for s in stats]

for col in buildings.select_dtypes(include=['datetime64', 'datetime64[ns]', 'datetimetz']).columns:
    buildings[col] = buildings[col].astype(str)

# 1. Convert the GeoDataFrame to a Python Dictionary (GeoJSON format)
data = json.loads(buildings.to_json())

# 2. Scrub all 'null' values from the properties of each feature
for feature in data['features']:
    # This dictionary comprehension keeps the key/value ONLY if the value isn't None/null
    feature['properties'] = {k: v for k, v in feature['properties'].items() if v is not None}

# 3. Write the cleaned dictionary to a file
with open(output_path, 'w', encoding='utf-8') as f:
    json.dump(data, f, ensure_ascii=False)

print(f"Cleaned file saved to {output_path} without null properties.")

print(f"Processing complete. Saved to {output_path}")