using Godot;
using System.Collections.Generic;
using System.Linq;
using System;

namespace C7.Map {

	class TerrainPcx {
		private static Random prng = new Random();
		private string name;
		// abc refers to the layout of the terrain tiles in the pcx based on
		// the positions of each terrain at the corner of 4 tiles.
		// - https://forums.civfanatics.com/threads/terrain-editing.622999/
		// - https://forums.civfanatics.com/threads/editing-terrain-pcx-files.102840/
		private string[] abc;
		public int atlas;
		public TerrainPcx(string name, string[] abc, int atlas) {
			this.name = name;
			this.abc = abc;
			this.atlas = atlas;
		}
		public bool validFor(string[] corner) {
			return corner.All(tile => abc.Contains(tile));
		}
		private int abcIndex(string terrain) {
			List<int> indices = new List<int>();
			for (int i = 0; i < abc.Count(); i++) {
				if (abc[i] == terrain) {
					indices.Add(i);
				}
			}
			return indices[prng.Next(indices.Count)];
		}

		// getTextureCoords looks up the correct texture index in the pcx
		// for the given position of each corner terrain type
		public Vector2I getTextureCoords(string[] corner) {
			int top = abcIndex(corner[0]);
			int right = abcIndex(corner[1]);
			int bottom = abcIndex(corner[2]);
			int left = abcIndex(corner[3]);
			int index = top + (left * 3) + (right * 9) + (bottom * 27);
			return new Vector2I(index % 9, index / 9);
		}
	}

	partial class Corners : Node2D {
		private List<string> terrainPcxFiles = new List<string> {
			"Art/Terrain/xtgc.pcx",
			"Art/Terrain/xpgc.pcx",
			"Art/Terrain/xdgc.pcx",
			"Art/Terrain/xdpc.pcx",
			"Art/Terrain/xdgp.pcx",
			"Art/Terrain/xggc.pcx",
			"Art/Terrain/wCSO.pcx",
			"Art/Terrain/wSSS.pcx",
			"Art/Terrain/wOOO.pcx",
		};
		private List<TerrainPcx> terrainPcxList;
		private string[,]terrain;
		private TileMap tilemap;
		private TileSet tileset;
		private List<ImageTexture> textures;
		private Vector2I tileSize = new Vector2I(128, 64);
		private int width;
		private int height;

		private void initializeTileMap() {
			this.tilemap = new TileMap();
			this.tileset = new TileSet();

			this.tileset.TileShape = TileSet.TileShapeEnum.Isometric;
			this.tileset.TileLayout = TileSet.TileLayoutEnum.Stacked;
			this.tileset.TileOffsetAxis = TileSet.TileOffsetAxisEnum.Horizontal;
			this.tileset.TileSize = this.tileSize;

			foreach (ImageTexture texture in this.textures) {
				TileSetAtlasSource source = new TileSetAtlasSource();
				source.Texture = texture;
				source.TextureRegionSize = this.tileSize;
				for (int x = 0; x < 9; x++) {
					for (int y = 0; y < 9; y++) {
						source.CreateTile(new Vector2I(x, y));
					}
				}
				this.tileset.AddSource(source);
			}
			this.tilemap.TileSet = tileset;

			this.terrainPcxList = new List<TerrainPcx>() {
				new TerrainPcx("tgc", new string[]{"tundra", "grassland", "coast"}, 0),
				new TerrainPcx("pgc", new string[]{"plains", "grassland", "coast"}, 1),
				new TerrainPcx("dgc", new string[]{"desert", "grassland", "coast"}, 2),
				new TerrainPcx("dpc", new string[]{"desert", "plains", "coast"}, 3),
				new TerrainPcx("dgp", new string[]{"desert", "grassland", "plains"}, 4),
				// new TerrainPcx("ggc", new string[]{"grassland", "grassland", "coast"}, 5),
				new TerrainPcx("cso", new string[]{"coast", "sea", "ocean"}, 6),
				new TerrainPcx("sss", new string[]{"sea", "sea", "sea"}, 7),
				new TerrainPcx("ooo", new string[]{"ocean", "ocean", "ocean"}, 8),
			};
			AddChild(this.tilemap);
		}

		private TerrainPcx getPcxForCorner(string[] corner) {
			return terrainPcxList.Find(tpcx => tpcx.validFor(corner));
		}

		void fill(Vector2I cell, int atlas, Vector2I texCoords) {
			this.tilemap.SetCell(0, cell, atlas, texCoords);
		}

		public Corners(C7GameData.GameMap gameMap) {
			this.textures = terrainPcxFiles.ConvertAll(path => Util.LoadTextureFromPCX(path));
			this.initializeTileMap();
			this.width = gameMap.numTilesWide / 2;
			this.height = gameMap.numTilesTall;
			this.terrain = new string[width, height];

			foreach (C7GameData.Tile t in gameMap.tiles) {
				int x = t.xCoordinate;
				int y = t.yCoordinate;
				// stacked coordinates
				x = y % 2 == 0 ? x / 2 : (x - 1) / 2;
				this.terrain[x, y] = t.baseTerrainTypeKey;
			}

			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					Vector2I cell = new Vector2I(x, y);
					string left = terrain[x, y];
					string right = terrain[(x + 1) % width, y];
					bool even = y % 2 == 0;
					string top = "coast";
					if (y > 0) {
						top = even ? terrain[x, y-1] : terrain[(x + 1) % width, y - 1];
					}
					string bottom = "coast";
					if (y < height - 1) {
						bottom = even ? terrain[x, y+1] : terrain[(x + 1) % width, y + 1];
					}
					string[] corner = new string[4]{top, right, bottom, left};
					TerrainPcx pcx = getPcxForCorner(corner);
					Vector2I texCoords = pcx.getTextureCoords(corner);
					fill(cell, pcx.atlas, texCoords);
				}
			}

		}
	}
}