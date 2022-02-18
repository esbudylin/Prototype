namespace C7GameData
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	public class Tile
	{
		// ExtraInfo will eventually be type object and use a type descriminator in JSON to determine
		//   how to deserialze. See https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-polymorphism
		//   and https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to?pivots=dotnet-5-0#support-polymorphic-deserialization
		//   Needed for saving to and loading from a serializable format
		public Civ3ExtraInfo ExtraInfo;
		public int xCoordinate;
		public int yCoordinate;
		public TerrainType baseTerrainType = TerrainType.NONE;
		public TerrainType overlayTerrainType = TerrainType.NONE;
		public City cityAtTile;
		public bool hasBarbarianCamp = false;

		//One thing to decide is do we want to have a tile have a list of units on it,
		//or a unit have reference to the tile it is on, or both?
		//The downside of both is that both have to be updated (and it uses a miniscule amount
		//of memory for pointers), but I'm inclined to go with both since it makes it easy and
		//efficient to perform calculations, whether you need to know which unit on a tile
		//has the best defense, or which tile a unit is on when viewing the Military Advisor.
		public List<MapUnit> unitsOnTile;

		public Dictionary<TileDirection, Tile> neighbors {get; set;}

		//See discussion on page 4 of the "Babylon" thread (https://forums.civfanatics.com/threads/0-1-babylon-progress-thread.673959) about sub-terrain type and Civ3 properties.
		//We may well move these properties somewhere, whether that's Civ3ExtraInfo, a Civ3Tile child class, a Dictionary property, or something else, in the future.
		public bool isSnowCapped;
		public bool isPineForest;

		public Tile()
		{
			unitsOnTile = new List<MapUnit>();
		}

		public MapUnit findTopDefender()
		{
			if (unitsOnTile.Count > 0) {
				var tr = unitsOnTile[0];
				foreach (var u in unitsOnTile)
					if (u.unitType.defense * u.hitPointsRemaining > tr.unitType.defense * tr.hitPointsRemaining)
						tr = u;
				return tr;
			} else
				return MapUnit.NONE;
		}
		
		public static Tile NONE = new Tile();

		//This should be used when we want to check if land tiles are next to water tiles.
		//Usually this is coast, but it could be Sea - see the "Deepwater Harbours" topics at CFC.
		//Sometimes we care *specifically* about the Coast terrain, e.g. galleys can only move on that terrain, not Sea or Ocean
		//Those cases should not use this method.
		public bool NeighborsWater() {
			foreach (Tile neighbor in getDiagonalNeighbors()) {
				if (neighbor.baseTerrainType.isWater()) {
					return true;
				}
			}
			return false;
		}

		public Tile[] getDiagonalNeighbors() {
			Tile[] diagonalNeighbors =  { neighbors[TileDirection.NORTHEAST], neighbors[TileDirection.NORTHWEST], neighbors[TileDirection.SOUTHEAST], neighbors[TileDirection.SOUTHWEST]};
			return diagonalNeighbors;
		}

		public override string ToString()
		{
			return "[" + xCoordinate + ", " + yCoordinate + "] (" + overlayTerrainType.name + " on " + baseTerrainType.name + ")";
		}

		public List<Tile> GetLandNeighbors() {
			return neighbors.Values.Where(tile => !tile.baseTerrainType.isWater()).ToList();
		}

		/**
		 * Returns neighbors of the "Coast" type, not including Sea or Ocean.  This is used e.g. for Galley movement.
		 * Eventually, this should be refactored into a more general "get valid neighbors to move to" type of method,
		 * which could work e.g. for units that can move anywhere except desert.
		 **/
		public List<Tile> GetCoastNeighbors()
		{
			return neighbors.Values.Where(tile => tile.baseTerrainType.key == "coast").ToList();
		}

		public bool IsLand()
		{
			return !baseTerrainType.isWater();
		}

		public int distanceToOtherTile(Tile other)
		{
			return (Math.Abs(other.xCoordinate - this.xCoordinate) + Math.Abs(other.yCoordinate - this.yCoordinate)) / 2;
		}
	}

	public enum TileDirection {
		NORTH,
		NORTHEAST,
		EAST,
		SOUTHEAST,
		SOUTH,
		SOUTHWEST,
		WEST,
		NORTHWEST
	}

	public static class TileDirectionExtensions {
		public static TileDirection reversed(this TileDirection dir)
		{
			switch (dir) {
			case TileDirection.NORTH:	 return TileDirection.SOUTH;
			case TileDirection.NORTHEAST: return TileDirection.SOUTHWEST;
			case TileDirection.EAST:	  return TileDirection.WEST;
			case TileDirection.SOUTHEAST: return TileDirection.NORTHWEST;
			case TileDirection.SOUTH:	 return TileDirection.NORTH;
			case TileDirection.SOUTHWEST: return TileDirection.NORTHEAST;
			case TileDirection.WEST:	  return TileDirection.EAST;
			case TileDirection.NORTHWEST: return TileDirection.SOUTHEAST;
			default: throw new ArgumentOutOfRangeException("Invalid TileDirection");
			}
		}

		public static (int, int) toCoordDiff(this TileDirection dir)
		{
			switch (dir) {
			case TileDirection.NORTH:	 return ( 0, -2);
			case TileDirection.NORTHEAST: return ( 1, -1);
			case TileDirection.EAST:	  return ( 2,  0);
			case TileDirection.SOUTHEAST: return ( 1,  1);
			case TileDirection.SOUTH:	 return ( 0,  2);
			case TileDirection.SOUTHWEST: return (-1,  1);
			case TileDirection.WEST:	  return (-2,  0);
			case TileDirection.NORTHWEST: return (-1, -1);
			default: throw new ArgumentOutOfRangeException("Invalid TileDirection");
			}
		}
	}

}
