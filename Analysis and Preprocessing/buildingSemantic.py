import ijson
from collections import Counter
import pandas as pd

file_path = '../OSM Files/london.geojson'

semantic_tags = [
    'height', 'building:levels', 'roof:shape', 'roof:material', 
    'building:material', 'building:color', 'amenity', 'shop'
]

# Residential types to exclude (Corrected spelling to 'residential')
res_types = ['house', 'residential', 'semidetached_house', 'terrace', 'garage', 'garages', 'detached', 'shed', 'apartment']

# Counters and Totals
withres_tag_counts = Counter()
withres_total_buildings = 0

nores_tag_counts = Counter()
nores_total_buildings = 0 # Fixed: was Counter(), should be int

onlyres_tag_counts = Counter()
onlyres_total_buildings = 0 # Fixed: was Counter(), should be int

for tag in semantic_tags:
    withres_tag_counts[tag] = 0
    nores_tag_counts[tag] = 0
    onlyres_tag_counts[tag] = 0

print("Processing file... this may take a moment.")

try:
    with open(file_path, 'r', encoding='utf-8') as f:
        parser = ijson.items(f, 'features.item.properties')
        
        for prop in parser:
            b_type = prop.get('building')
            if b_type:
                # 1. Global Stats (All Buildings)
                withres_total_buildings += 1
                for tag in semantic_tags:
                    if tag in prop:
                        withres_tag_counts[tag] += 1

                if b_type not in res_types:
                    nores_total_buildings += 1
                    for tag in semantic_tags:
                        if tag in prop:
                            nores_tag_counts[tag] += 1
                else:
                    onlyres_total_buildings += 1
                    for tag in semantic_tags:
                        if tag in prop:
                            onlyres_tag_counts[tag] += 1

    # --- Process Global Results ---
    res_all = []
    for tag in semantic_tags:
        count = withres_tag_counts[tag]
        perc = (count / withres_total_buildings * 100) if withres_total_buildings > 0 else 0
        res_all.append({'Semantic Tag': tag, 'Count': count, 'Percentage': f"{perc:.2f}%"})

    # --- Process Non-Residential Results ---
    res_nores = []
    for tag in semantic_tags:
        count = nores_tag_counts[tag]
        perc = (count / nores_total_buildings * 100) if nores_total_buildings > 0 else 0
        res_nores.append({'Semantic Tag': tag, 'Count': count, 'Percentage': f"{perc:.2f}%"})
        
    # --- Process Non-Residential Results ---
    res_onlyres = []
    for tag in semantic_tags:
        count = onlyres_tag_counts[tag]
        perc = (count / onlyres_total_buildings * 100) if onlyres_total_buildings > 0 else 0
        res_onlyres.append({'Semantic Tag': tag, 'Count': count, 'Percentage': f"{perc:.2f}%"})

    # Output
    print("\n--- ALL BUILDINGS ---")
    print(f"Total: {withres_total_buildings}")
    print(pd.DataFrame(res_all).to_string(index=False))

    print("\n--- NON-RESIDENTIAL ONLY ---")
    print(f"Total: {nores_total_buildings}")
    print(pd.DataFrame(res_nores).to_string(index=False))
    
    print("\n--- RESIDENTIAL ONLY ---")
    print(f"Total: {onlyres_total_buildings}")
    print(pd.DataFrame(res_onlyres).to_string(index=False))

except FileNotFoundError:
    print(f"Error: File {file_path} not found.")
except Exception as e:
    print(f"An error occurred: {e}")