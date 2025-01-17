using System.Collections.Generic;
using C7GameData;
using Godot;
using ConvertCiv3Media;

namespace C7.Map {
	public class BorderLayer : LooseLayer {
		private readonly string texturePath = "Art/Terrain/Territory.pcx";
		private readonly ImageTexture[] borderGraphics = new ImageTexture[8];
		private readonly Dictionary<(int, Color), ImageTexture> textureCache = new();

		public BorderLayer() {
			Pcx texturePcx = Util.LoadPCX(texturePath);

			int textureWidth = 128;
			int textureHeight = 72;

			for (int j = 0; j < 4; j++) {
				for (int k = 0; k < 2; k++) {
					borderGraphics[j * 2 + k] = PCXToGodot.getImageTextureFromPCX(texturePcx, k * textureWidth, j * textureHeight, textureWidth, textureHeight, true);
				}
			}
		}

		// TODO: This method doesn't precisely mirror Civ3 coloring
		private static Color CalcSecondaryColor(Color mainColor) {
			return new Color(mainColor.R * 0.8f, mainColor.G * 0.8f, mainColor.B * 0.8f);
		}

		private ImageTexture GetBorderTexture(int textureIndex, Color borderColor) {
			if (textureCache.TryGetValue((textureIndex, borderColor), out ImageTexture res)) {
				return res;
			}

			ImageTexture texture = borderGraphics[textureIndex];

			Color secondaryColor = CalcSecondaryColor(borderColor);

			Dictionary<Color, Color> colorReplacements = new Dictionary<Color, Color>
			{
				{ Color.Color8(236, 255, 0), borderColor },
				{ Color.Color8(255, 0, 0), secondaryColor}
			};

			Image image = Util.TransformColors(texture.GetImage(), colorReplacements);

			var newTexture = ImageTexture.CreateFromImage(image);
			textureCache[(textureIndex, borderColor)] = newTexture;

			return newTexture;
		}

		public override void drawObject(LooseView looseView, GameData gameData, Tile tile, Vector2 tileCenter) {
			if (tile.owner is null) {
				return;
			}

			var directionToTextureIdx = new Dictionary<TileDirection, int>
				{
					{ TileDirection.NORTHWEST, 0 },
					{ TileDirection.NORTHEAST, 2 },
					{ TileDirection.SOUTHWEST, 4 },
					{ TileDirection.SOUTHEAST, 6 }
				};

			Color borderColor = Util.LoadColor(tile.owner.colorIndex);

			foreach (var entry in directionToTextureIdx) {
				if (tile.neighbors[entry.Key].owner != tile.owner) {
					ImageTexture texture = GetBorderTexture(entry.Value, borderColor);
					Vector2 offset = texture.GetSize() * 0.5f;

					looseView.DrawTexture(texture, tileCenter - offset);
				}
			}
		}
	}
}
