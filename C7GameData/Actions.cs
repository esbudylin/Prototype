namespace C7GameData {
	public static class C7Action {
		public const string EndTurn = "end_turn";
		public const string Escape = "escape";
		public const string MoveUnitSouthwest = "move_unit_southwest";
		public const string MoveUnitSouth = "move_unit_south";
		public const string MoveUnitSoutheast = "move_unit_southeast";
		public const string MoveUnitWest = "move_unit_west";
		public const string MoveUnitEast = "move_unit_east";
		public const string MoveUnitNorthwest = "move_unit_northwest";
		public const string MoveUnitNorth = "move_unit_north";
		public const string MoveUnitNortheast = "move_unit_northeast";
		public const string ToggleAnimations = "toggle_animations";
		public const string ToggleGrid = "toggle_grid";
		public const string ToggleZoom = "toggle_zoom";
		public const string UnitBombard = "unit_bombard";
		public const string UnitBuildCity = "unit_build_city";
		public const string UnitBuildRoad = "unit_build_road";
		public const string UnitDisband = "unit_disband";
		public const string UnitExplore = "unit_explore";
		public const string UnitFortify = "unit_fortify";
		public const string UnitGoto = "unit_goto";
		public const string UnitHold = "unit_hold";
		public const string UnitSentry = "unit_sentry";
		public const string UnitSentryEnemyOnly = "unit_sentry_enemy_only";
		public const string UnitWait = "unit_wait";

		// This method transforms an action string into a TileDirection.
		// The boolean value in the returned tuple indicates whether the conversion was successful.
		public static (bool, TileDirection) ToTileDirection(string action) {
			// TODO: replace bool with an invalid TileDirection enum
			// More in this issue: https://github.com/C7-Game/Prototype/issues/397
			return action switch {
				MoveUnitSouthwest => (true, TileDirection.SOUTHWEST),
				MoveUnitSouth => (true, TileDirection.SOUTH),
				MoveUnitSoutheast => (true, TileDirection.SOUTHEAST),
				MoveUnitWest => (true, TileDirection.WEST),
				MoveUnitEast => (true, TileDirection.EAST),
				MoveUnitNorthwest => (true, TileDirection.NORTHWEST),
				MoveUnitNorth => (true, TileDirection.NORTH),
				MoveUnitNortheast => (true, TileDirection.NORTHEAST),
				_ => (false, TileDirection.NORTH),
			};
		}
	}
}
