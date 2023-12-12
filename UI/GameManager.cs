using Godot;
using System;

public partial class GameManager : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		RenderingServer.SetDefaultClearColor(Colors.Black);
	}

	public void startGame(PackedScene scene){
		if(GetTree().Paused){
			ContinueGame();
			return;
		}
		TransitionToScene(scene);
	}

	public void exitGame(){
		GetTree().Quit();
	}

	public void pauseGame(){
		GetTree().Paused = true;
	}

	public void ContinueGame(){
		GetTree().Paused = false;
	}

	async void TransitionToScene(PackedScene scene){
		// Show transition
		await ToSignal(GetTree().CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);
		GetTree().ChangeSceneToPacked(scene);
	}

}
