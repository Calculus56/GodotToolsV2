using Godot;
using System;

public partial class BackgroundNode : Node2D
{
	Camera2D camera;
	Vector2 spriteSize = new Vector2(228, 128), scale;
	Node2D GetRoot(){
		// IF you have an autoload script, root will not work, so it's better to use current scene.
		return GetTree().CurrentScene.GetChild(0) as Node2D;
	}
	Window windowBounds;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		windowBounds = GetWindow();
		//windowBounds.SizeChanged += UpdateScale; 
		GD.Print(GetRoot());
		camera = GetTree().Root.GetCamera2D();
		// Sets the background initial position to the camera.
		Position = camera.Position;
		UpdateScale();
	}

	void UpdateScale(){
        scale = windowBounds.Size / spriteSize;
		Scale = new Vector2(scale.Y, scale.Y);
	}
}
