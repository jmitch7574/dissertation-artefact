import geopandas as gpd

# Load your GeoJSON
gdf = gpd.read_file('../OSM Files/lincoln/buildings-latlng.geojson')

# Get the total bounds of the entire FeatureCollection
# Returns: array([minx, miny, maxx, maxy])
bbox = gdf.total_bounds

print(bbox)