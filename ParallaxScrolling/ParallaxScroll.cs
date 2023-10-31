using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// PLace this on a 
public partial class ParallaxScroll : Node2D
{
	// Works by having Sprite2D children.
	
	// Parallax Width should match the game width or higher.
	// Light can affect canvases if the light cull mask is the same.
	// DirectionaLight2D ignores the cull mask.

	// Depth Parallax Scrolling can be achieved by using trigonometry to find
	// the midpoint between the camera center and original posiiton of the
	// object you want to use. You can use MoveSpeed as the paramter,
	// the higher the value the closer it is. 1 will mean it moves with the player
	// or doesn't have any drag.

	// Need to add a mask to the background so waves disappear.
	[Export] Vector2 parallaxEffectMultiplier = new Vector2(1, 1);
	[Export] bool isRepeated;
	// Change the x and y based on the size of the sprite that covers the whole screen.
	Vector2 spriteSize = new Vector2(228, 128);

	List<Sprite2D> backgrounds = new List<Sprite2D>();
	Node2D parent;
	Camera2D camera;
	Vector2 lastCameraPos, basePos, scale;
	Window windowBounds;

	Node2D GetRoot(){
		return GetTree().Root.GetChild(0) as Node2D;
	}

	public override void _Ready()
	{
		parent = GetParent<Node2D>();
		camera = GetRoot().GetNode<Camera2D>("MainCamera");
		lastCameraPos = camera.Position;
		// Gets all background sprites below the node connected to the script.
		// var temp = GetChildren();
		// Gets the Height and Width of the screen.
		windowBounds = GetWindow();

		// Formats the children into an array then adds them to an array after
		// converting the type.
		// Array.ForEach(temp.ToArray(), item => backgrounds.Add(item as Sprite2D));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		scale = windowBounds.Size / spriteSize;
		Vector2 deltaMovement = camera.Position - lastCameraPos;
		Position += new Vector2{
			X = deltaMovement.X * parallaxEffectMultiplier.X / scale.X, 
			Y = deltaMovement.Y * parallaxEffectMultiplier.Y / scale.Y,
		};
		lastCameraPos = camera.Position;
	}
}
