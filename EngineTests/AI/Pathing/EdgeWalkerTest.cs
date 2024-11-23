using System.Collections.Generic;
using System.Linq;
using C7Engine.Pathing;
using C7GameData;
using Xunit;

namespace EngineTests {
	public class WalkerOnLandTest {
		private WalkerOnLand walker = new();
		private ID id = ID.None("test-tile");
		private Tile mountain  = new(ID.None("")) {
			baseTerrainType = new() {
				Key = "mountains"
			},
			overlayTerrainType = new() {
				Key = "mountains",
				movementCost = 3
			}
		};
		private Tile hill  = new(ID.None("")) {
			baseTerrainType = new() {
				Key = "hills"
			},
			overlayTerrainType = new() {
				Key = "hills",
				movementCost = 2
			}
		};
		private Tile plains  = new(ID.None("")) {
			baseTerrainType = new() {
				Key = "plains"
			},
			overlayTerrainType = new() {
				Key = "plains",
				movementCost = 1
			}
		};
		private Tile coast  = new(ID.None("")) {
			baseTerrainType = new() {
				Key = "coast"
			},
			overlayTerrainType = new() {
				Key = "coast",
				movementCost = 1
			}
		};

		[Fact]
		void testIgnoresWater() {
			Tile start = hill;

			// Add 3 neighbors, one of which is water.
			start.neighbors[TileDirection.NORTH] = coast;
			start.neighbors[TileDirection.SOUTH] = mountain;
			start.neighbors[TileDirection.WEST] = plains;

			// The water tile should be ignored, and the costs should be correct.
			IEnumerable<Edge<Tile>> edges = walker.getEdges(start);
			Assert.Equal(2, edges.Count());

			Assert.Contains(edges, item => item.current == mountain && item.distanceToCurrent == 3);
			Assert.Contains(edges, item => item.current == plains && item.distanceToCurrent == 1);
		}

		[Fact]
		void testRoadOnDestinationNotOnStart() {
			Tile start = hill;

			// Set up a neighbor with a road.
			Tile end = plains;
			end.overlays.road = true;
			start.neighbors[TileDirection.NORTH] = end;

			// The road shouldn't matter, since we don't have a road.
			IEnumerable<Edge<Tile>> edges = walker.getEdges(start);
			Assert.Single(edges);
			Assert.Contains(edges, item => item.current == plains && item.distanceToCurrent == 1);
		}

		[Fact]
		void testRoadOnStartNotOnDestination() {
			Tile start = hill;
			start.overlays.road = true;

			// Set up a neighbor without a road.
			Tile end = plains;
			start.neighbors[TileDirection.NORTH] = end;

			// The road shouldn't matter, since the destination doesn't have a road.
			IEnumerable<Edge<Tile>> edges = walker.getEdges(start);
			Assert.Single(edges);
			Assert.Contains(edges, item => item.current == plains && item.distanceToCurrent == 1);
		}

		[Fact]
		void testRoadOnStartAndDestination() {
			Tile start = hill;
			start.overlays.road = true;

			// Set up a neighbor with a road.
			Tile end = plains;
			end.overlays.road = true;
			start.neighbors[TileDirection.NORTH] = end;

			// The cost should be adjusted because we both have a road.
			IEnumerable<Edge<Tile>> edges = walker.getEdges(start);
			Assert.Single(edges);
			Assert.Contains(edges, item => item.current == plains && item.distanceToCurrent == 1.0f / 3.0f);
		}
	}
}
