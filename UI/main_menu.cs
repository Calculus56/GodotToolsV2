using Godot;
using System;

public partial class main_menu : CanvasLayer
{
	[Export] PackedScene level1;
	GameManager gameManager;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		gameManager = GetTree().Root.GetNode<GameManager>("GameManager");
	}

	void _on_play_pressed(){
		gameManager.startGame(level1);
	}

	void _on_exit_pressed(){
		gameManager.exitGame();
	}
}
