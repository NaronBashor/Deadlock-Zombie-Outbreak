using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapAccentPlacer : MonoBehaviour
{
    public Tilemap[] groundTilemaps;      // Array of Tilemaps to check for the ground tiles
    public Tilemap accentTilemap;         // Tilemap for placing the accent tiles

    // Define the sprite names you want to search for
    public string grassTileSpriteName = "grass_tiles_0000_Layer-55";
    public string sandTileSpriteName = "sand_0012_tile";
    public string asphaltTileSpriteName = "asphalt_tiles_0012_Layer-0";
    public string waterTile1SpriteName = "water_0012_tile";
    public string waterTile2SpriteName = "water_tiles_0005_Layer-24";

    // Define arrays of accent tiles for random selection
    public TileBase[] grassAccentTiles;    // Array of random grass accent tiles
    public TileBase[] sandAccentTiles;     // Array of random sand accent tiles
    public TileBase[] asphaltAccentTiles;  // Array of random asphalt accent tiles
    public TileBase[] waterAccentTiles;    // Array of random water accent tiles

    private void Start()
    {
        PlaceAccentsOnTargetTiles();
    }

    private void PlaceAccentsOnTargetTiles()
    {
        // Loop through all provided Tilemaps
        foreach (Tilemap groundTilemap in groundTilemaps) {
            // Get the bounds of the current Tilemap
            BoundsInt bounds = groundTilemap.cellBounds;
            Debug.Log($"Processing Tilemap with bounds: {bounds}");

            // Loop through the tiles in the current Tilemap within the bounds
            for (int x = bounds.xMin; x < bounds.xMax; x++) {
                for (int y = bounds.yMin; y < bounds.yMax; y++) {
                    Vector3Int tilePosition = new Vector3Int(x, y, 0);

                    // Attempt to get the tile at the current position
                    TileBase currentTile = groundTilemap.GetTile(tilePosition);

                    if (currentTile != null) {
                        Debug.Log($"Found tile at {tilePosition} of type {currentTile.GetType().Name}");

                        // Handle different types of TileBase (e.g., custom tiles or Rule Tiles)
                        // If it's a basic Tile, check its sprite
                        if (currentTile is Tile tileWithSprite && tileWithSprite.sprite != null) {
                            string spriteName = tileWithSprite.sprite.name;  // Get the sprite name
                            Debug.Log($"Checking tile at {tilePosition} with sprite name: {spriteName}");

                            // Compare the sprite name to the known tile names
                            if (spriteName == grassTileSpriteName) {
                                TileBase randomAccentTile = GetRandomAccentTile(grassAccentTiles);
                                if (randomAccentTile != null) {
                                    accentTilemap.SetTile(tilePosition, randomAccentTile);
                                    Debug.Log($"Placed grass accent tile at {tilePosition}");
                                }
                            } else if (spriteName == sandTileSpriteName) {
                                TileBase randomAccentTile = GetRandomAccentTile(sandAccentTiles);
                                if (randomAccentTile != null) {
                                    accentTilemap.SetTile(tilePosition, randomAccentTile);
                                    Debug.Log($"Placed sand accent tile at {tilePosition}");
                                }
                            } else if (spriteName == asphaltTileSpriteName) {
                                TileBase randomAccentTile = GetRandomAccentTile(asphaltAccentTiles);
                                if (randomAccentTile != null) {
                                    accentTilemap.SetTile(tilePosition, randomAccentTile);
                                    Debug.Log($"Placed asphalt accent tile at {tilePosition}");
                                }
                            } else if (spriteName == waterTile1SpriteName || spriteName == waterTile2SpriteName) {
                                TileBase randomAccentTile = GetRandomAccentTile(waterAccentTiles);
                                if (randomAccentTile != null) {
                                    accentTilemap.SetTile(tilePosition, randomAccentTile);
                                    Debug.Log($"Placed water accent tile at {tilePosition}");
                                }
                            }
                        } else {
                            Debug.Log($"Tile at {tilePosition} is not a basic 'Tile' or does not have a sprite.");
                        }
                    } else {
                        Debug.Log($"No tile found at {tilePosition}. This position is empty.");
                    }
                }
            }
        }

        // Optionally, refresh the accent tilemap after placing all accent tiles
        accentTilemap.RefreshAllTiles();
    }

    // Function to return a random accent tile from an array of accent tiles
    private TileBase GetRandomAccentTile(TileBase[] accentTiles)
    {
        if (accentTiles == null || accentTiles.Length == 0) return null;  // Return null if no accent tiles available
        int randomIndex = Random.Range(0, accentTiles.Length);            // Randomly select an accent tile
        return accentTiles[randomIndex];
    }
}
