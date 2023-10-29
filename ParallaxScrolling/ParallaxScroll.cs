using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

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

	List<Sprite2D> backgrounds = new List<Sprite2D>();


	public override void _Ready()
	{
		// Gets all background sprites below the node connected to the script.
		var temp = GetChildren();
		// Gets the Height and Width of the screen.
		var camera = GetViewportRect().Size;
		// Formats the children into an array then adds them to an array after
		// converting the type.
		Array.ForEach(temp.ToArray(), item => backgrounds.Add(item as Sprite2D));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
