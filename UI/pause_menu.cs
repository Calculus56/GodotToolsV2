using Godot;
using System;

public partial class pause_menu : CanvasLayer
{
	GameManager gameManager;
	public override void _Ready()
	{
		gameManager = GetTree().Root.GetNode<GameManager>("GameManager");
	}

	// Will have to change the process mode to always, of the game will
	// stay paused and not continue.
	void _on_continue_pressed(){
		gameManager.ContinueGame();
		QueueFree();
	}
}
