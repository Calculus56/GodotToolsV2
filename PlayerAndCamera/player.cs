using Godot;
using System;

public partial class player : CharacterBody2D
{
	// If you want to rotate the character around like in an ocean or space game then you only need to worry about turning the velocity on and off.
	// If you want the velocity to go in a certain direction then don't rotate the character, just change the sprites.
	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	PlayerSettings playerSettings = new PlayerSettings();
	
    public override void _Ready()
    {
        playerSettings = GetNode<PlayerSettings>("PlayerSettings");
    }

	//If you want to add other scripts, make nodes under player.
    public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		if(playerSettings.gameType == PlayerSettings.GameType.SideScroller){
			// Add the gravity.
			if (!IsOnFloor() )
				velocity.Y += gravity * (float)delta;

			// Handle Jump.
			if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
				velocity.Y = JumpVelocity;
			
			Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
			if(direction.X == -1){
				//animSpr2D.FlipH = true;
			}else if(direction.X == 1){
				//animSpr2D.FlipH = false;
			}
		}
		
		if(GlobalPosition.Y < 0) GlobalPosition = new Vector2(GlobalPosition.X, 1);
		
		velocity = Velocity;
		switch(playerSettings.inputType){
			case PlayerSettings.InputType.Keyboard:
				// If using keyboard controls.
				//RotateObjects(GetGlobalMousePosition());
				break;
			case PlayerSettings.InputType.Controller:
				//If using controller controls.
				Vector2 joyPos = new Vector2
				{
					X = Input.GetActionStrength("ControllerRight") - Input.GetActionStrength("ControllerLeft"),
					Y = Input.GetActionStrength("ControllerDown") - Input.GetActionStrength("ControllerUp"),
				};

				//RotateObjects(joyPos);
				break;
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
