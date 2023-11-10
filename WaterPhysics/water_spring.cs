using Godot;
using System;

public partial class water_spring : Node2D
{
	public float velocity = 0, force = 0, height = 0, targetHeight = 0;
	Vector2 startPos;
	CollisionShape2D collsion;

	int index = 0;
	// How much an object affects the water.
	float motion_factor = 0.02f;

	Node collided_with = null; 

	[Signal]
	public delegate void splashEventHandler(int index, float speed);

    public override void _Ready()
    {
		startPos = Position;
        collsion = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
    }

    public void WaterUpdate(float springConstant, float dampening){
		height = Position.Y;

		var springExtension = height - targetHeight;
		var loss = -dampening * velocity;

		force = -springConstant * springExtension + loss;

		velocity += force;

		Position += new Vector2(0, velocity);
	}

	public void Initialize(int x_position, int id){
		height = Position.Y;
		targetHeight = Position.Y;
		velocity = 0;
		Position = new Vector2(x_position, Position.X);
		index = id;
	}
	// X needs to be 0 at the start.
	public void set_collsion_shape(int value){
		var extents = collsion.Shape.GetRect().Size;
		var new_extents = new Vector2(value/2, extents.Y);
		GD.Print($"{new_extents.X} {new_extents.Y}");
		(collsion.Shape as RectangleShape2D).Size = new_extents;
	}

	void _on_area_2d_body_entered(Node body){
		if(body == collided_with) return;
		collided_with = body;
		var speed = (body as CharacterBody2D).Velocity.Y * motion_factor;
		EmitSignal("splash", index, speed);
	}
}
