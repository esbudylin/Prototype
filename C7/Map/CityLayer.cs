using System;
using System.Collections.Generic;
using C7GameData;
using ConvertCiv3Media;
using Godot;
using Serilog;
using Serilog.Events;

namespace C7.Map {
	public partial class CityLayer : LooseLayer {

		private ILogger log = LogManager.ForContext<Game>();

		private ImageTexture cityTexture;
		private Dictionary<string, ImageTexture> cityLabels = new Dictionary<string, ImageTexture>();
		private Vector2 citySpriteSize;
		private FontFile smallFont = new FontFile();
		private FontFile midSizedFont = new FontFile();
		private Pcx cityIcons = Util.LoadPCX("Art/Cities/city icons.pcx");
		private Image nonEmbassyStar;

		const int CITY_LABEL_HEIGHT = 23;
		const int LEFT_RIGHT_BOXES_WIDTH = 24;
		const int LEFT_RIGHT_BOXES_HEIGHT = CITY_LABEL_HEIGHT - 2;
		const int TEXT_ROW_HEIGHT = 9;

		public CityLayer()
		{
			//TODO: Generalize, support multiple city types, etc.
			Pcx pcx = Util.LoadPCX("Art/Cities/rMIDEAST.PCX");
			int height = pcx.Height/4;
			int width = pcx.Width/3;
			cityTexture = Util.LoadTextureFromPCX("Art/Cities/rMIDEAST.PCX", 0, 0, width, height);
			citySpriteSize = new Vector2(width, height);
			smallFont.Data = ResourceLoader.Load<FontFile>("res://Fonts/NotoSans-Regular.ttf").Data;
			// TODO: port to Godot 4
			// smallFont.Size = 11;

			midSizedFont.Data = ResourceLoader.Load<FontFile>("res://Fonts/NotoSans-Regular.ttf").Data;
			// TODO: port to Godot 4
			// midSizedFont.Size = 18;

			nonEmbassyStar = PCXToGodot.getImageFromPCX(cityIcons, 20, 1, 18, 18);
		}

		public override void drawObject(LooseView looseView, GameData gameData, Tile tile, Vector2 tileCenter)
		{
			if (tile.cityAtTile is null) {
				return;
			}

			City city = tile.cityAtTile;
			Rect2 screenRect = new Rect2(tileCenter - (float)0.5 * citySpriteSize, citySpriteSize);
			Rect2 textRect = new Rect2(new Vector2(0, 0), citySpriteSize);
			looseView.DrawTextureRectRegion(cityTexture, screenRect, textRect);

			int turnsUntilGrowth = city.TurnsUntilGrowth();
			string turnsUntilGrowthText = turnsUntilGrowth == int.MaxValue || turnsUntilGrowth < 0 ? "- -" : "" + turnsUntilGrowth;
			string cityNameAndGrowth = $"{city.name} : {turnsUntilGrowthText}";
			string productionDescription = city.itemBeingProduced.name + " : " + city.TurnsUntilProductionFinished();

			int cityNameAndGrowthWidth = (int)smallFont.GetStringSize(cityNameAndGrowth).x;
			int productionDescriptionWidth = (int)smallFont.GetStringSize(productionDescription).x;
			int maxTextWidth = Math.Max(cityNameAndGrowthWidth, productionDescriptionWidth);

			int cityLabelWidth = maxTextWidth + (city.IsCapital()? 70 : 45);	//TODO: Is 65 right?  70?  Will depend on whether it's capital, too
			int textAreaWidth = cityLabelWidth - (city.IsCapital() ? 50 : 25);
			if (log.IsEnabled(LogEventLevel.Verbose)) {
				log.Verbose("Width of city name = " + maxTextWidth);
				log.Verbose("City label width: " + cityLabelWidth);
				log.Verbose("Text area width: " + textAreaWidth);
			}

			Image labelBackground = CreateLabelBackground(cityLabelWidth, city, textAreaWidth);
			ImageTexture cityLabel = ImageTexture.CreateFromImage(labelBackground);

			DrawLabelOnScreen(looseView, tileCenter, cityLabelWidth, city, cityLabel);
			DrawTextOnLabel(looseView, tileCenter, cityNameAndGrowthWidth, productionDescriptionWidth, city, cityNameAndGrowth, productionDescription, cityLabelWidth);
		}
		private void DrawLabelOnScreen(LooseView looseView, Vector2 tileCenter, int cityLabelWidth, City city, ImageTexture cityLabel)
		{

			Rect2 labelDestination = new Rect2(tileCenter + new Vector2(cityLabelWidth / -2, 24), new Vector2(cityLabelWidth, CITY_LABEL_HEIGHT)); //24 is a swag
			Rect2 allOfTheLabel = new Rect2(new Vector2(0, 0), new Vector2(cityLabelWidth, CITY_LABEL_HEIGHT));
			cityLabels[city.name] = cityLabel;
			looseView.DrawTextureRectRegion(cityLabel, labelDestination, allOfTheLabel);
		}

		private void DrawTextOnLabel(LooseView looseView, Vector2 tileCenter, int cityNameAndGrowthWidth, int productionDescriptionWidth, City city, string cityNameAndGrowth, string productionDescription, int cityLabelWidth)
		{
			//Destination for font is based on lower-left of baseline of font, not upper left as for blitted rectangles
			int cityNameOffset = cityNameAndGrowthWidth / -2;
			int prodDescriptionOffset = productionDescriptionWidth / -2;
			if (!city.IsCapital()) {
				cityNameOffset += 12;
				prodDescriptionOffset += 12;
			}
			Vector2 cityNameDestination = tileCenter + new Vector2(cityNameOffset, 24) + new Vector2(0, 10);
			looseView.DrawString(smallFont, cityNameDestination, cityNameAndGrowth, modulate: Color.Color8(255, 255, 255, 255));
			Vector2 productionDestination = tileCenter + new Vector2(prodDescriptionOffset, 24) + new Vector2(0, 20);
			looseView.DrawString(smallFont, productionDestination, productionDescription, modulate: Color.Color8(255, 255, 255, 255));

			//City pop size
			string popSizeString = "" + city.size;
			int popSizeWidth = (int)midSizedFont.GetStringSize(popSizeString).x;
			int popSizeOffset = LEFT_RIGHT_BOXES_WIDTH / 2 - popSizeWidth / 2;
			Vector2 popSizeDestination = tileCenter + new Vector2(cityLabelWidth / -2, 24) + new Vector2(popSizeOffset, 18);
			Color popColor = Color.Color8(255, 255, 255, 255);
			if (city.TurnsUntilGrowth() < 0) {
				popColor = Color.Color8(255, 0, 0, 255);
			}
			looseView.DrawString(midSizedFont, popSizeDestination, popSizeString, modulate: popColor);
		}

		private Image CreateLabelBackground(int cityLabelWidth, City city, int textAreaWidth)
		{
			//Label/name/producing area
			Image labelImage = new Image();
			labelImage.Create(cityLabelWidth, CITY_LABEL_HEIGHT, false, Image.Format.Rgba8);
			labelImage.Fill(Color.Color8(0, 0, 0, 0));
			byte transparencyLevel = 192; //25%
			Color civColor = new Color((uint)city.owner.color);
			civColor = new Color(civColor, transparencyLevel);
			Color civColorDarker = Color.Color8(0, 0, 138, transparencyLevel); //todo: automate the darker() function.  maybe less transparency?
			Color topRowGrey = Color.Color8(32, 32, 32, transparencyLevel);
			Color bottomRowGrey = Color.Color8(48, 48, 48, transparencyLevel);
			Color backgroundGrey = Color.Color8(64, 64, 64, transparencyLevel);
			Color borderGrey = Color.Color8(80, 80, 80, transparencyLevel);

			Image horizontalBorder = new Image();
			horizontalBorder.Create(cityLabelWidth - 2, 1, false, Image.Format.Rgba8);
			horizontalBorder.Fill(borderGrey);
			labelImage.BlitRect(horizontalBorder, new Rect2i(0, 0, new Vector2i(cityLabelWidth - 2, 1)), new Vector2i(1, 0));
			labelImage.BlitRect(horizontalBorder, new Rect2i(0, 0, new Vector2i(cityLabelWidth - 2, 1)), new Vector2i(1, 22));

			Image verticalBorder = new Image();
			verticalBorder.Create(1, CITY_LABEL_HEIGHT - 2, false, Image.Format.Rgba8);
			verticalBorder.Fill(borderGrey);
			labelImage.BlitRect(verticalBorder, new Rect2i(0, 0, new Vector2i(1, 23)), new Vector2i(0, 1));
			labelImage.BlitRect(verticalBorder, new Rect2i(0, 0, new Vector2i(1, 23)), new Vector2i(cityLabelWidth - 1, 1));

			Image bottomRow = new Image();
			bottomRow.Create(textAreaWidth, 1, false, Image.Format.Rgba8);
			bottomRow.Fill(bottomRowGrey);
			labelImage.BlitRect(bottomRow, new Rect2i(0, 0, new Vector2i(textAreaWidth, 1)), new Vector2i(25, 21));

			Image topRow = new Image();
			topRow.Create(textAreaWidth, 1, false, Image.Format.Rgba8);
			topRow.Fill(topRowGrey);
			labelImage.BlitRect(topRow, new Rect2i(0, 0, new Vector2i(textAreaWidth, 1)), new Vector2i(25, 1));

			Image background = new Image();
			background.Create(textAreaWidth, TEXT_ROW_HEIGHT, false, Image.Format.Rgba8);
			background.Fill(backgroundGrey);
			labelImage.BlitRect(background, new Rect2i(0, 0, new Vector2i(textAreaWidth, 9)), new Vector2i(25, 2));
			labelImage.BlitRect(background, new Rect2i(0, 0, new Vector2i(textAreaWidth, 9)), new Vector2i(25, 12));

			Image centerDivider = new Image();
			centerDivider.Create(textAreaWidth, 1, false, Image.Format.Rgba8);
			centerDivider.Fill(civColor);
			labelImage.BlitRect(centerDivider, new Rect2i(0, 0, new Vector2i(textAreaWidth, 1)), new Vector2i(25, 11));

			Image leftAndRightBoxes = new Image();
			leftAndRightBoxes.Create(LEFT_RIGHT_BOXES_WIDTH, LEFT_RIGHT_BOXES_HEIGHT, false, Image.Format.Rgba8);
			leftAndRightBoxes.Fill(civColor);
			labelImage.BlitRect(leftAndRightBoxes, new Rect2i(0, 0, new Vector2i(24, 21)), new Vector2i(1, 1));
			if (city.IsCapital()) {
				labelImage.BlitRect(leftAndRightBoxes, new Rect2i(0, 0, new Vector2i(24, 21)), new Vector2i(cityLabelWidth - 25, 1));
				labelImage.BlendRect(nonEmbassyStar, new Rect2i(0, 0, new Vector2i(18, 18)), new Vector2i(cityLabelWidth - 24, 2));
			}
			//todo: darker shades of civ color around edges
			return labelImage;
		}
	}
}
