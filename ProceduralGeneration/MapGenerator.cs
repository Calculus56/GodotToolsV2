using Godot;
using System;
using System.Collections.Generic;
using System.Text;

public partial class MapGenerator : Node2D
{
	ProceduralTilemap tilemapVisualizer;
	//LoadingHandler loadingHandler;
	[Export]
	public int sizeX, sizeY;
	int width, height, neighbourTiles = 5;
	int startingWidth, startingHeight;
	int[,] chunks;
	int chunkX = 0, chunkY = 0;
	int[,] newArrX, newArrY;
	int currentChunk;
	public string seed;
	[Export]
	public bool useRandomSeed;
	// Slider in the editor.
	// [Range(0,100)]
	[Export(PropertyHint.Range, "0, 100,")]
	public int randomFillPercent;

	List<List<Coord>> wallRegions;
	List<List<Coord>> roomRegions;

	// Comma in the brackets means it's a 2-D array. Holds either a 0 or 1
	List<int[,]> maps = new List<int[,]>();
	List<List<int[,]>> layerMaps = new List<List<int[,]>>();
	int[,] map;
	IEnumerable<Vector2I> floorPositions, emptyPositions;

	RandomNumberGenerator rng = new RandomNumberGenerator();
	public override void _Ready() {
		tilemapVisualizer = GetParent<ProceduralTilemap>();
		//loadingHandler = FindAnyObjectByType<LoadingHandler>();
		width = 50; height = 50;
		startingWidth = width; startingHeight = height;
		newArrX = new int[width * 2, height];
		newArrY = new int[width, height * 2];
		GenerateMap();
	}
	// Godot 1:08 500 * 5000
	// Unity 2:15 500 * 5000 (10,000,000)
	// 9:19 8400*2400 (20,160,000) 50*50, a large Terraria world.
	// 4:36 is how long it takes to load 800x5000 (4,000,000) tiles. 100*100
	// 1:51 50*50
	// 1:31 25*25
	public void GenerateMap() {
		//map = new int[width, height];
		
		// 25*25 1 minute
		// 100*100 2 minutes
		layerMaps = new List<List<int[,]>>();
		chunks = new int[sizeX / width, sizeY / height];
		int totalChunks = chunks.GetLength(1) * chunks.GetLength(0);
		for (int chunkY = 0; chunkY < chunks.GetLength(1); chunkY++) {
			for (int chunkX = 0; chunkX < chunks.GetLength(0); chunkX++) {
				map = new int[width, height];
				currentChunk++;
				//loadingHandler.ChangeProcessName($"Generating Chunks: {currentChunk} / {totalChunks}");
				//loadingHandler.ChangeSliderValue(currentChunk / (float)totalChunks);
				//yield return new WaitForEndOfFrame();
				RandomFillMap();
				for (int i = 0; i < neighbourTiles; i++) {
					// Almost instant
					SmoothMap();
				}
				// Another way to do this is to process the map by chunks
				ProcessMap();
				maps.Add(map);

				// Do this: Call tilemap visualizer
				ConvertVectors(chunkX, chunkY);
				tilemapVisualizer.PaintFloorTiles(floorPositions, emptyPositions);
				// Need to add 1 because for loop adds a the end.
				this.chunkX = chunkX + 1;
			}
			layerMaps.Add(maps);
			maps = new List<int[,]>();
			this.chunkY = chunkY + 1;
			chunkX = 0;
		}

		ConnectMaps();
		//StartCoroutine(ConnectMaps());
		// Save to txt file
		//yield return new WaitForEndOfFrame();
	}

	public void ConnectMapsButton() {
		ConnectMaps();
	}

	public void ConvertVectors(int chunkX, int chunkY) {
		floorPositions = new List<Vector2I>();
		emptyPositions = new List<Vector2I>();
		HashSet<Vector2I> hashFloorPositions = new HashSet<Vector2I>();
		HashSet<Vector2I> hashEmptyPositions = new HashSet<Vector2I>();
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				Vector2I pos = new Vector2I(
					(-sizeX / 2 + x) + (chunkX * startingWidth), (-sizeY / 2 + y) + (chunkY * startingHeight)
					);
				// Should add if map is 0
				switch (map[x, y]) {
					case 0:
						hashEmptyPositions.Add(pos);
						break;
					case 1:
						hashFloorPositions.Add(pos);
						break;
				}
			}
		}
		floorPositions = hashFloorPositions;
		emptyPositions = hashEmptyPositions;
	}

	// Any regions that are less than 50 tiles will be deleted.
	void ProcessMap() {
		wallRegions = GetRegions(1);
		roomRegions = GetRegions(0);

		RemoveSmallRooms(1, wallRegions);
		RemoveSmallRooms(0, roomRegions);
		int roomThresholdSize = width/2;
		List<Room> survivingsRooms = new List<Room>();

		foreach (List<Coord> roomRegion in roomRegions) {
			if (roomRegion.Count < roomThresholdSize) {
				foreach (Coord tile in roomRegion) {
					map[tile.tileX, tile.tileY] = 1;
				}
			}
			else {
				survivingsRooms.Add(new Room(roomRegion, map));
			}
		}

		survivingsRooms.Sort();
		try {
			survivingsRooms[0].isMainRoom = true;
			survivingsRooms[0].isAccessibleFromMainRoom = true;
		}catch{}
		// Connecting rooms take a long time
		ConnectClosestRooms(survivingsRooms);
	}

	void ConnectMaps() {
		chunkX = 0;
		chunkY = 0;
		int randomPathDown;
		int layerSize = layerMaps.Count;
		foreach (List<int[,]> layerMap in layerMaps) {
			randomPathDown = rng.RandiRange(0, layerMap.Count)/2;
			// Need to make some kind of for loop.
			for (int i = 0; i < layerMap.Count; i++) {
				CombineArraysX(layerMap, i);
				if(randomPathDown == i) {
					randomPathDown = rng.RandiRange(0, layerMap.Count);
					CombineArraysY(i);
				}
				chunkX++;
			}
			chunkY++;
			//loadingHandler.ChangeProcessName($"Connecting Chunk Layers: {chunkY} / {layerSize}");
			//loadingHandler.ChangeSliderValue(chunkY / (float)layerSize);
			//yield return new WaitForEndOfFrame();
			chunkX = 0;
		}
		chunkY = 0;
		//yield return new WaitForEndOfFrame();
		//loadingHandler.ChangeProcessName("Loading Scene");
	}

	void ProcessPathBetweenMaps(int[,] newArr) {
		map = newArr;
		//Debug.Log($"{newArrX.GetLength(0)} {newArrX.GetLength(1)}");
		ProcessMap();
		ConvertVectors(chunkX, chunkY);
		tilemapVisualizer.PaintFloorTiles(emptyPositions);
	}

	void PrintArrs() {

		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < newArrX.GetLength(0); i++) {
			for (int j = 0; j < newArrX.GetLength(1); j++) {
				sb.Append(newArrX[i, j]);
				sb.Append(' ');
			}
			sb.AppendLine();
		}
		//Debug.Log(sb.ToString());
		//foreach(int[,] m in maps) {
		//    // Combine map next in line 
		//}

		//StringBuilder ab = new StringBuilder();
		//foreach (Vector2Int floor in floorPositions) {
		//    ab.Append(floor);
		//    ab.AppendLine();
		//}
		//Debug.Log(ab.ToString());
	}

	void CombineArraysX(List<int[,]> layerMap, int startingIndex) {
		width = newArrX.GetLength(0); height = newArrX.GetLength(1);
		try {
			int[,] first = layerMap[startingIndex];
			int[,] second = layerMap[startingIndex + 1];
			for (int i = 0; i < first.GetLength(0); i++) {
				for (int j = 0; j < first.GetLength(1); j++) {
					newArrX[i, j] = first[i, j];
					newArrX[i + first.GetLength(0), j] = second[i, j];
				}
			}

			ProcessPathBetweenMaps(newArrX);
		}
		catch{}
	}
	void CombineArraysY(int startingIndex) {
		width = newArrY.GetLength(0); height = newArrY.GetLength(1);
		// Debug.Log($"{chunkY + 1} {startingIndex}");
		try {
			List<int[,]> yChunk1 = layerMaps[chunkY];
			List<int[,]> yChunk2 = layerMaps[chunkY + 1];
			int[,] first = yChunk1[startingIndex];
			int[,] second = yChunk2[startingIndex];
			for (int i = 0; i < first.GetLength(0); i++) {
				for (int j = 0; j < first.GetLength(1); j++) {
					newArrY[i, j] = first[i, j];
					newArrY[i, j + first.GetLength(1)] = second[i, j];
				}
			}

			ProcessPathBetweenMaps(newArrY);
		}
		catch {}
	}

	void RemoveSmallRooms(int num, List<List<Coord>> region) {
		List<List<Coord>> wallRegions = region;
		int wallThresholdSize = width/2;
		foreach (List<Coord> wallRegion in wallRegions) {
			if (wallRegion.Count < wallThresholdSize) {
				foreach (Coord tile in wallRegion) {
					map[tile.tileX, tile.tileY] = (num == 0) ? 1 : 0;
				}
			}
		}
	}

	void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false) {

		List<Room> roomListA = new List<Room>();
		List<Room> roomListB = new List<Room>();
		// Forces rooms not connected to other rooms to connect to the main room, will seperate the rooms that is accessible to the main room and not.
		if (forceAccessibilityFromMainRoom) {
			foreach(Room room in allRooms) {
				if (room.isAccessibleFromMainRoom) {
					roomListB.Add(room);
				}
				else {
					roomListA.Add(room);
				}
			}
		}
		else {
			roomListA = allRooms;
			roomListB = allRooms;
		}

		int bestDistance = 0;
		Coord bestTileA = new Coord();
		Coord bestTileB = new Coord();
		Room bestRoomA = new Room();
		Room bestRoomB = new Room();
		bool possibleConnectionFound = false;

		foreach(Room roomA in roomListA) {
			if (!forceAccessibilityFromMainRoom) {
				possibleConnectionFound = false;
				if(roomA.connectedRooms.Count > 0) {
					continue;
				}
			}


			foreach (Room roomB in roomListB) {
				// Rooms will evenetually be compared to itself
				if(roomA == roomB || roomA.IsConnected(roomB)) {
					continue;
				}
				for(int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++) {
					for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++) {
						Coord tileA = roomA.edgeTiles[tileIndexA];
						Coord tileB = roomB.edgeTiles[tileIndexB];
						int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

						if(distanceBetweenRooms < bestDistance || !possibleConnectionFound) {
							bestDistance = distanceBetweenRooms;
							possibleConnectionFound = true;
							bestTileA = tileA;
							bestTileB = tileB;
							bestRoomA = roomA;
							bestRoomB = roomB;
						}
					}
				}
			}

			if (possibleConnectionFound && !forceAccessibilityFromMainRoom) {
				CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
			}
		}
		// Get all rooms that isn't accessible to the main room and then find which is closest.
		if (possibleConnectionFound && forceAccessibilityFromMainRoom) {
			CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
			ConnectClosestRooms(allRooms, true);
		}

		if (!forceAccessibilityFromMainRoom) {
			ConnectClosestRooms(allRooms, true);
		}
	}
	// Connects the rooms 
	void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB) {
		Room.ConnectRooms(roomA, roomB);
		//if(map.GetLength(0) == 100) Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.green, 100);
		//else Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.red, 100);

		HashSet<Vector2I> line = GetLine(tileA, tileB);
		foreach(Vector2I c in line) {
			DrawCircle(c, 2);
		}
	}

	// Draws a circle at each coordinate point.
	void DrawCircle(Vector2I c, int r) {
		for(int x = -r; x <= r; x++) {
			for (int y = -r; y <= r; y++) {
				if(x*x + y*y <= r * r) {
					int drawX = Mathf.CeilToInt(c.X) + x;
					int drawY = Mathf.CeilToInt(c.Y) + y;

					if(IsInMapRange(drawX, drawY)) {
						map[drawX, drawY] = 0;
					}
				}
			}
		}
	}

	// Calculates the Coordinates that the line will make up.
	HashSet<Vector2I> GetLine(Coord from, Coord to) {
		HashSet<Vector2I> line = new HashSet<Vector2I>();

		int x = from.tileX;
		int y = from.tileY;

		int dx = to.tileX - from.tileX;
		int dy = to.tileY - from.tileY;

		bool inverted = false;
		int step = Math.Sign(dx);
		int gradientStep = Math.Sign(dy);

		int longest = Mathf.Abs(dx);
		int shortest = Mathf.Abs(dy);

		if (longest < shortest) {
			inverted = true;
			longest = Mathf.Abs(dy);
			shortest = Mathf.Abs(dx);

			step = Math.Sign(dy);
			gradientStep = Math.Sign(dx);
		}
		int gradientAccumulation = longest / 2;
		for (int i = 0; i < longest; i++) {
			line.Add(new Vector2I(x, y));

			if (inverted) {
				y += step;
			}
			else {
				x += step;
			}

			gradientAccumulation += shortest;
			if (gradientAccumulation >= longest) {
				if (inverted) {
					x += gradientStep;
				}
				else {
					y += gradientStep;
				}
				gradientAccumulation -= longest;
			}
		}

		return line;
	}

	Vector2I CoordToWorldPoint(Coord tile) {
		return new Vector2I((-sizeX / 2 + tile.tileX) + (chunkX * startingWidth), 
			(-sizeY / 2 + tile.tileY) + (chunkY * startingHeight));
	}

	// Scans the regions to tell what area of tiles are there.
	List<List<Coord>> GetRegions(int tileType) {
		List<List<Coord>> regions = new List<List<Coord>>();
		int[,] mapFlags = new int[width, height];

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (mapFlags[x, y] == 0 && map[x,y] == tileType) {
					List<Coord> newRegion = GetRegionTiles(x, y);
					regions.Add(newRegion);

					foreach(Coord tile in newRegion) {
						mapFlags[tile.tileX, tile.tileY] = 1;
					}
				}
			}
		}
		return regions;
	}

	// Starts at one tile and scans the tiles around them until there are no more tiles.
	List<Coord> GetRegionTiles(int startX, int startY) {
		List<Coord> tiles = new List<Coord>();
		int[,] mapFlags = new int[width, height];
		int tileType = map[startX, startY];

		Queue<Coord> queue = new Queue<Coord>();
		queue.Enqueue(new Coord(startX, startY));
		// Shows that we looked at the tile.
		mapFlags[startX, startY] = 1;

		// Stops when queue.Enqueue is never called.
		while(queue.Count > 0) {
			Coord tile = queue.Dequeue();
			tiles.Add(tile);
			// Scans the tiles around the starting tile.
			for(int x = tile.tileX - 1; x <= tile.tileX + 1; x++) {
				for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++) {
					if(IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX)) {
						if (mapFlags[x, y] == 0 && map[x, y] == tileType) {
							mapFlags[x, y] = 1;
							queue.Enqueue(new Coord(x, y));
						}
					}
				}
			}
		}
		return tiles;
	}

	bool IsInMapRange(int x, int y) {
		return x >= 0 && x < width && y >= 0 && y < height;
	}

	void RandomFillMap()
	{
		if (useRandomSeed) {
			seed = Time.GetTicksMsec().ToString();
		}
		// Intilaizes a Random object
		System.Random pseudoRandom = new System.Random(seed.GetHashCode() * currentChunk);

		for(int x = 0; x < width; x++){
			for (int y = 0; y < height; y++){
				if(x == 0 || x == width-1 || y == 0 || y== height - 1)
				{
					map[x, y] = 1;
				}
				else {
					map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
				}
			}
		}
	}

	// Cleans up the generated mess by checking tiles next to it.
	void SmoothMap() {
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				int neighbourWallTiles = GetSurrondingWallCount(x, y);

				if (neighbourWallTiles > neighbourTiles - 1)
					map[x, y] = 1;
				else if (neighbourWallTiles < neighbourTiles - 1)
					map[x, y] = 0;
			}
		}
	}
	/// <summary>
	/// Gets the neighboring tiles to see how many are white.
	/// </summary>
	int GetSurrondingWallCount(int gridX, int gridY) {
		int wallCount = 0;
		for(int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) {
			for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) {
				if (IsInMapRange(neighbourX, neighbourY)) {
					if (neighbourX != gridX || neighbourY != gridY) {
						wallCount += map[neighbourX, neighbourY];
					}
				}
				else {
					wallCount += 1;
				}
			}
		}
		return wallCount;
	}

	struct Coord {
		public int tileX;
		public int tileY;

		public Coord(int x, int y) {
			tileX = x;
			tileY = y;
		}
	}

	//private void OnDrawGizmos()
	//{
	//    if(map != null)
	//    {
	//        for (int x = 0; x < width; x++)
	//        {
	//            for (int y = 0; y < height; y++)
	//            {
	//                Gizmos.color = (map[x,y] == 1)? Color.black : Color.white;
	//                Vector3 pos = new Vector3(-width / 2 + x + .5f, -height/2 + y + 0.5f);
	//                Gizmos.DrawCube(pos, Vector3.one);
	//            }
	//        }
	//    }
	//}

	class Room : IComparable<Room>{
		public List<Coord> tiles;
		public List<Coord> edgeTiles;
		public List<Room> connectedRooms;
		public int roomSize;
		public bool isAccessibleFromMainRoom;
		public bool isMainRoom;

		// An empty contructor used to instantiate an object.
		public Room() {

		}

		public Room(List<Coord> roomTiles, int[,] map) {
			tiles = roomTiles;
			roomSize = tiles.Count;
			connectedRooms = new List<Room>();

			edgeTiles = new List<Coord>();
			// Checks if a tile is a wall so we know we're on the edge.
			foreach(Coord tile in tiles) {
				for(int x = tile.tileX-1; x <= tile.tileX + 1; x++) {
					for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++) {
						if(x == tile.tileX || y == tile.tileY) {
							if(map[x,y] == 1) {
								edgeTiles.Add(tile);
							}
						}
					}
				}
			}
		}

		public void SetAccessibleFromMainRoom() {
			if (!isAccessibleFromMainRoom) {
				isAccessibleFromMainRoom = true;
				foreach(Room connectedRoom in connectedRooms) {
					connectedRoom.SetAccessibleFromMainRoom();
				}
			}
		}
		// When rooms are connected rooms are changed to accessible from main room. 
		public static void ConnectRooms(Room roomA, Room roomB) {
			if (roomA.isAccessibleFromMainRoom) {
				roomB.SetAccessibleFromMainRoom();
			}else if (roomB.isAccessibleFromMainRoom) {
				roomA.SetAccessibleFromMainRoom();
			}
			roomA.connectedRooms.Add(roomB);
			roomB.connectedRooms.Add(roomA);
		}

		public bool IsConnected(Room otherRoom) {
			return connectedRooms.Contains(otherRoom);
		}

		public int CompareTo(Room otherRoom) {
			return otherRoom.roomSize.CompareTo(roomSize);
		}
	}
}
