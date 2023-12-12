using Godot;
using System;

public partial class LoadingHandler : CanvasLayer
{
	[Signal]
	public delegate void GenerateNewMapEventHandler();

	MapGenerator mapGenerator;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		mapGenerator = GetTree().CurrentScene.GetNode<MapGenerator>("MapGenerator");
		GenerateNewMap += mapGenerator.GenerateMap;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	void _on_generate_map_pressed(){
		EmitSignal(SignalName.GenerateNewMap);
	}
}
