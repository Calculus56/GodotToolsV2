using Godot;
using System;

public partial class game_screen : CanvasLayer
{
	[Export]
	PackedScene pauseScreen;
	GameManager gameManager;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		gameManager = GetTree().Root.GetNode<GameManager>("GameManager");
	}
	
	void _on_pause_button_pressed(){
		gameManager.pauseGame();

		var pauseMenuInstance = pauseScreen.Instantiate();
		GetTree().Root.AddChild(pauseMenuInstance);
	}
}
