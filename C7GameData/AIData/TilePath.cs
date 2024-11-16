using System;
using System.Collections.Generic;

namespace C7GameData
{
	public class TilePath
	{
		private Tile destination; //stored in case we need to re-calculate
		public Queue<Tile> path {get; private set;}

		private TilePath() {
			destination = Tile.NONE;
			path = new Queue<Tile>();
		}

		public TilePath(Tile destination, Queue<Tile> path) {
			this.destination = destination;
			this.path = path;
		}

		// The next tile in the path, or Tile.NONE if there
		// are no remaining tiles, or the path is invalid
		public Tile Next() {
			return PathLength() > 0 ? path.Dequeue() : Tile.NONE;
		}

		//TODO: Once we have roads, we should return the calculated cost, not just the length.
		//This will require Dijkstra or another fancier pathing algorithm
		public int PathLength() {
			return path != null ? path.Count : -1;
		}

		public int PathCost(Tile from, float unitMovementPoints) {
			if (path == null) { return 0; }

			int turns = 0;
			float remainingMovementPoints = unitMovementPoints;
			foreach (Tile tile in path) {
				float cost = getMovementCost(from, from.directionTo(tile), tile);
				Console.WriteLine("Cost from " + from.ToString() + " to " + tile.ToString() + " is " + cost);

				remainingMovementPoints -= cost;
				if (remainingMovementPoints <= 0) {
					++turns;
					remainingMovementPoints = unitMovementPoints;
				}

				from = tile;
			}
			return turns;
		}

		// Indicates no path was found to the requested destination.
		public static TilePath NONE = new TilePath();

		// A valid path of length 0
		public static TilePath EmptyPath(Tile destination) {
			return new TilePath(destination, new Queue<Tile>());
		}

		public static float getMovementCost(Tile from, TileDirection dir, Tile newLocation) {
			if (from.HasRiverCrossing(dir)) return newLocation.MovementCost();
			if (from.overlays.railroad && newLocation.overlays.railroad) return 0;
			if ((from.overlays.railroad || from.overlays.road) && newLocation.overlays.road) return 1.0f / 3;
			return newLocation.MovementCost();
		}
	}
}
