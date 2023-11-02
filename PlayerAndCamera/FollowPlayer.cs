using Godot;
using System;

public partial class FollowPlayer : Camera2D
{
	[Export] bool onBoat;
	Node2D player;
	Vector2 modifier;
	public override void _Ready()
	{
		player = GetTree().GetFirstNodeInGroup("player") as Node2D;
	}

	public override void _Process(double delta)
	{
		if(onBoat) modifier = new Vector2(0, -200);
		GlobalPosition = GlobalPosition.Lerp(player.GlobalPosition + modifier, 0.5f);
	}
}
