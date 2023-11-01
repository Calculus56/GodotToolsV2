using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// PLace this on a 
public partial class ParallaxScroll : Sprite2D
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
	Vector2 spriteSize = new Vector2(192, 128);

	List<Sprite2D> backgrounds = new List<Sprite2D>();
	Node2D parent;
	Camera2D camera;
	Vector2 startPos, lastCameraPos, basePos, scale, addedPos;
	Window windowBounds;
	// multiplier needs to be tested to see what works.
	float length, multiplier = 0.59f;

	Node2D GetRoot(){
		return GetTree().Root.GetChild(0) as Node2D;
	}

	public override void _Ready()
	{
		startPos = Position;
		windowBounds = GetWindow();
		
		scale = windowBounds.Size / spriteSize;
		length = spriteSize.X * scale.X;
		parent = GetParent<Node2D>();
		camera = GetRoot().GetNode<Camera2D>("MainCamera");
		lastCameraPos = camera.Position;
		// Gets all background sprites below the node connected to the script.
		// var temp = GetChildren();

		// Formats the children into an array then adds them to an array after
		// converting the type.
		// Array.ForEach(temp.ToArray(), item => backgrounds.Add(item as Sprite2D));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//scale = windowBounds.Size / spriteSize;
		Vector2 deltaMovement = camera.Position - lastCameraPos;
		addedPos += new Vector2{
			X = deltaMovement.X * parallaxEffectMultiplier.X / scale.X, 
			Y = deltaMovement.Y * parallaxEffectMultiplier.Y / scale.Y,
		};
		Position = startPos;
		lastCameraPos = camera.Position;
		// If the camera position is greater than the background position it 
		if(isRepeated){
			// When you scale up, the position needs to scale.
			if(camera.GlobalPosition.X - Position.X * scale.X > length) startPos.X += length * multiplier;
			GD.Print($"{Position.X * scale.X - camera.GlobalPosition.X} {length}");
			//else if(camera.GlobalPosition.X + length < GlobalPosition.X) startPos.X -= length;
		}
	}
}
