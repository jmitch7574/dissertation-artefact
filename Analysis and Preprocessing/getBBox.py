import geopandas as gpd

# Load GeoJSON
gdf = gpd.read_file('../OSM Files/lincoln/buildings-latlng.geojson')

# Get the total bounds of the entire FeatureCollection
bbox = gdf.total_bounds

print(bbox)