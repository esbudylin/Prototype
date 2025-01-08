using System;
using System.Collections.Generic;
using C7GameData;
using Godot;

namespace C7.Map {
	public partial class TileOverlayLayer : LooseLayer {
		private readonly ImageTexture roadTexture;
		private readonly ImageTexture railroadTexture;
		private readonly ImageTexture mineTexture;
		private readonly Vector2 tileSize;

		public TileOverlayLayer() {
			roadTexture = Util.LoadTextureFromPCX("Art/Terrain/roads.pcx");
			railroadTexture = Util.LoadTextureFromPCX("Art/Terrain/railroads.pcx");
			tileSize = roadTexture.GetSize() / 16;
			// grid 16x16 tiles
			// assume that roads and railroads textures have the same size

			// TerrainBuildings.pcx contains multiple pieces of art in a grid, with each
			// item being 128x64 pixesl.
			//
			// The basic version is:
			//  Fortress (ancient)     | Colony (an)   | Barb camp
			//  Fortress (medieval)    | Colony (me)   | Mine
			//  Fortress (industrial)  | Colony (in)   | Empty
			//  Fortress (modern)      | Colony (mo)   | Empty
			mineTexture = Util.LoadTextureFromPCX("Art/Terrain/TerrainBuildings.pcx", 128 * 2, 64, 128, 64);
		}

		public override void drawObject(LooseView looseView, GameData gameData, Tile tile, Vector2 tileCenter) {
			if (!HasAnyOverlays(tile)) return;

			Rect2 screenTarget = new Rect2(tileCenter - tileSize / 2, tileSize);

			if (hasRoad(tile) && !hasRailRoad(tile)) {
				int roadIndex = 0;
				foreach (KeyValuePair<TileDirection, Tile> dirToTile in tile.neighbors) {
					if (hasRoad(dirToTile.Value)) {
						roadIndex |= getFlag(dirToTile.Key);
					}
				}
				looseView.DrawTextureRectRegion(roadTexture, screenTarget, getRect(roadIndex));
			}

			if (hasRailRoad(tile)) {
				int roadIndex = 0;
				int railroadIndex = 0;
				foreach (KeyValuePair<TileDirection, Tile> dirToTile in tile.neighbors) {
					if (hasRailRoad(dirToTile.Value)) {
						railroadIndex |= getFlag(dirToTile.Key);
					} else if (hasRoad(dirToTile.Value)) {
						roadIndex |= getFlag(dirToTile.Key);
					}
				}
				if (roadIndex != 0) {
					looseView.DrawTextureRectRegion(roadTexture, screenTarget, getRect(roadIndex));
				}
				looseView.DrawTextureRectRegion(railroadTexture, screenTarget, getRect(railroadIndex));
			}

			if (tile.overlays.mine) {
				looseView.DrawTexture(mineTexture, screenTarget.Position);
			}
		}

		private Rect2 getRect(int index) {
			int row = index >> 4;
			int column = index & 0xF;
			return new Rect2(column * tileSize.X, row * tileSize.Y, tileSize);
		}

		private static int getFlag(TileDirection direction) {
			return direction switch {
				TileDirection.NORTHEAST => 0x1,
				TileDirection.EAST => 0x2,
				TileDirection.SOUTHEAST => 0x4,
				TileDirection.SOUTH => 0x8,
				TileDirection.SOUTHWEST => 0x10,
				TileDirection.WEST => 0x20,
				TileDirection.NORTHWEST => 0x40,
				TileDirection.NORTH => 0x80,
				_ => throw new ArgumentOutOfRangeException("Invalid TileDirection")
			};
		}

		private static bool HasAnyOverlays(Tile tile) {
			return tile.overlays.road || tile.overlays.railroad || tile.overlays.mine;
		}

		private static bool hasRoad(Tile tile) {
			return tile.overlays.road;
		}

		private static bool hasRailRoad(Tile tile) {
			return tile.overlays.railroad;
		}
	}
}
