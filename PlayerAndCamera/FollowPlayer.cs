using Godot;
using System;

public partial class FollowPlayer : Camera2D
{
	Node2D player;
	public override void _Ready()
	{
		player = GetTree().GetFirstNodeInGroup("player") as Node2D;
	}

	public override void _Process(double delta)
	{
		GlobalPosition = player.GlobalPosition;
	}
}
