using Godot;
using System;
using System.Collections.Generic;

public partial class ProceduralTilemap : TileMap
{
	[Export]
	public bool ClearTileMap{
		get => false;
		set{
			if(value){
				Clear();
			}
		}
	}
	
	Vector2I tilePosition;
	Vector2I floorTile = new Vector2I(3, 0);
	Vector2I emptyTile = new Vector2I(1, 0);
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// tilePosition = new Vector2I(0, 0);
		// SetCell(0, tilePosition, 0, floorTile);
		// tilePosition = new Vector2I(0, 1);
		// SetCell(0, tilePosition, 0, emptyTile);
	}

	public void PaintFloorTiles(IEnumerable<Vector2I> floorPositions, IEnumerable<Vector2I> emptyPositions) {
            PaintTiles(floorPositions, floorTile);
            PaintTiles(emptyPositions, emptyTile);
    }

	public void PaintFloorTiles(IEnumerable<Vector2I> emptyPositions) {
            PaintTiles(emptyPositions, emptyTile);
    }
	
	void PaintTiles(IEnumerable<Vector2I> positions, Vector2I tile) {
            foreach (var position in positions) {
                PaintSingleTile(tile, position);
            }
        }
	private void PaintSingleTile(Vector2I tile, Vector2I position) {
            SetCell(0, position, 0, tile);
    }

	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest)
			Clear();
	}
}
