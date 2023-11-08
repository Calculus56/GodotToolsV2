using Godot;
using System;

public partial class water_spring : Node2D
{
	public float velocity = 0, force = 0, height = 0, targetHeight = 0;

	public void WaterUpdate(float springConstant, float dampening){
		height = Position.Y;

		var springExtension = height - targetHeight;
		var loss = -dampening * velocity;

		force = -springConstant * springExtension + loss;

		velocity += force;

		Position += new Vector2(0, velocity);
	}

	public void Initialize(int x_position){
		height = Position.Y;
		targetHeight = Position.Y;
		velocity = 0;
		Position = new Vector2(x_position, Position.X);
	}
}
