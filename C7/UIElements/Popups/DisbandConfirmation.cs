using Godot;

public class DisbandConfirmation : TextureRect
{
	public DisbandConfirmation() 
	{

	}

	public override void _Ready()
	{
		base._Ready();

		//Dimensions in-game are 530x320
		//The top 110px are for the advisor leaderhead, Domestic in this case.
		//For some reason it uses the Happy graphics.

		//Create a transparent texture background of the appropriate size.
		//This is super important as if we just add the children, the parent won't be able to figure
		//out the size of this TextureRect, and it won't be able to align it properly.
		//I added an extra 10 px on width for margin... maybe we should do margin another way, but 
		//this works reliably.
		ImageTexture thisTexture = new ImageTexture();
		Image image = new Image();
		image.Create(540, 320, false, Image.Format.Rgba8);
		image.Fill(Color.Color8(0, 0, 0, 0));
		thisTexture.CreateFromImage(image);
		this.Texture = thisTexture;


		ImageTexture AdvisorHappy = Util.LoadTextureFromPCX("Art/SmallHeads/popupDOMESTIC.pcx", 1, 40, 149, 110);
		TextureRect AdvisorHead = new TextureRect();
		AdvisorHead.Texture = AdvisorHappy;
		//Appears at 400, 110 in game, but leftmost 25px are transparent with default graphics
		AdvisorHead.SetPosition(new Vector2(375, 0));
		AddChild(AdvisorHead);

		TextureRect background = PopupOverlay.GetPopupBackground(530, 210);
		background.SetPosition(new Vector2(0, 110));
		AddChild(background);

		PopupOverlay.AddHeaderToPopup(this, "Domestic Advisor", 120);

		Label warningMessage = new Label();
		//TODO: General-purpose text breaking up util.  Instead of \n
		//This appears to be the way to do multi line labels, see: https://godotengine.org/qa/11126/how-to-break-line-on-the-label-using-gdscript
		//Maybe there's an awesomer control we can user instead
		warningMessage.AddColorOverride("font_color", new Color(0, 0, 0));
		warningMessage.Text = "Disband Settler?  Pardon me but these are OUR people. Do \nyou really want to disband them?";

		warningMessage.SetPosition(new Vector2(25, 170));
		AddChild(warningMessage);

		PopupOverlay.AddButton(this, "Yes, we need to!", 215, "disband");
		PopupOverlay.AddButton(this, "No. Maybe you are right, advisor.", 245, "cancel");
	}

	private void disband()
	{
		//tell the game to disband it.  right now we're doing that first, which is WRONG!
		GetParent().EmitSignal("UnitDisbanded");
		GetParent().EmitSignal("hide");
	}

	private void cancel()
	{
		GetParent().EmitSignal("hide");
	}
}