import ijson
from collections import Counter
import pandas as pd

file_path = '../OSM Files/london.geojson'

# Counters are great for large-scale tallies without building a huge list
building_counts = Counter()
amenity_counts = Counter()
indoor_counts = Counter()
    
building_count_total = 0
indoor_count_total = 0

print("Processing file...")

try:
    # 'encoding="utf-8"' is the fix for your UnicodeDecodeError
    with open(file_path, 'r', encoding='utf-8') as f:
        # ijson.items streams only the 'properties' block of each feature
        # This prevents loading the 1.3GB into RAM all at once
        parser = ijson.items(f, 'features.item.properties')
        
        for prop in parser:
            # Safely grab values and update tallies
            b = prop.get('building')
            a = prop.get('amenity')
            c = prop.get('indoor')
            
            if b: 
                building_counts[b] += 1
                building_count_total += 1
            if a and b: amenity_counts[a] += 1
            if c: 
                indoor_counts[c] += 1
                indoor_count_total += 1

    # Convert results to a nice DataFrame for display
    df_b = pd.DataFrame(building_counts.most_common(15), columns=['Building Type', 'Count'])
    df_a = pd.DataFrame(amenity_counts.most_common(15), columns=['Amenity Type', 'Count'])
    df_c = pd.DataFrame(indoor_counts.most_common(15), columns=['Indoor Type', 'Count'])

    print("\n--- TOP BUILDINGS ---")
    print(df_b)
    print("\n--- TOP AMENITIES ---")
    print(df_a)
    print("\n--- INDOOR COUNT ---")
    print(f"{indoor_count_total} / {building_count_total} {(indoor_count_total * 100) / building_count_total}%")

except UnicodeDecodeError as e:
    print(f"Still hitting an encoding error: {e}")
except MemoryError:
    print("The file is too large for standard processing; ijson is required.")