using Godot;
using System;

public partial class Player : CharacterBody2D
{
	// If you want to rotate the character around like in an ocean or space game then you only need to worry about turning the velocity on and off.
	// If you want the velocity to go in a certain direction then don't rotate the character, just change the sprites.
	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	public PlayerSettings playerSettings;
	[Export]
	PlayerSettings _playerAsset;
	[Export]
	Node2D[] NodesToRotate;
	float angle;

	Vector2 direction = new Vector2();
	
    public override void _Ready()
    {
		playerSettings = _playerAsset;
		// Get the script from Autoload scripts.
        //playerSettings = GetTree().Root.GetNode<PlayerSettings>("PlayerSettings");
		//Node _player = _playerAsset.Instantiate();
		//Initialize();

		// It Works, when you print playerSettings it will give you the node the script is on.
		// GD.Print("Script: " + GetTree().Root.GetNode<PlayerSettings>("PlayerSettings").GetScript());
    }

	//If you want to add other scripts, make nodes under player.
    public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		direction = new Vector2(0, 0);

		/// Sidescoller
		///		Controller, Keyboard
		///	TopDown
		///		Controller, Keyboard
		switch(playerSettings.gameType){
			case PlayerSettings.GameType.SideScroller:
				// Add the gravity.
				if (!IsOnFloor())
					velocity.Y += gravity * (float)delta;

				// Handle Jump.
				if (Input.IsActionJustPressed("Jump") && IsOnFloor())
					velocity.Y = JumpVelocity;
				
				Movement();
				velocity.X = direction.X * Speed;
				Velocity = velocity;
				break;
			case PlayerSettings.GameType.TopDown:
				switch(playerSettings.rotationType){
					case PlayerSettings.RotationType.Velocity:
						Movement();
						velocity.X = direction.X * Speed;
						velocity.Y = direction.Y * Speed;
						Velocity = velocity;
						break;
					case PlayerSettings.RotationType.Character:
						// Uses the mouseposition if the input is keyboard, a controller will be based on the joyaxis.
						RotateObjects(playerSettings.inputType == PlayerSettings.InputType.Keyboard
							 ? GetGlobalMousePosition() : GetControllerStrength()*100 + GlobalPosition);
						angle = NodesToRotate[0].RotationDegrees;
						ScaleObjects();
						velocity = new Vector2(Mathf.Cos(Mathf.DegToRad(angle)), Mathf.Sin(Mathf.DegToRad(angle)));
						float mult = Input.GetActionStrength("Move") * Speed;
						Velocity = velocity * mult;
						break;
				}
				break;
		}
		
		//if(GlobalPosition.Y < 0) GlobalPosition = new Vector2(GlobalPosition.X, 1);
		
		// Controls for aiming at something, using mouse or controller.
		switch(playerSettings.inputType){
			case PlayerSettings.InputType.Keyboard:
				// If using keyboard controls.
				//RotateObjects(GetGlobalMousePosition());
				break;
			case PlayerSettings.InputType.Controller:
				//If using controller controls.
				Vector2 joyPos = new Vector2
				{
					X = Input.GetActionStrength("Right") - Input.GetActionStrength("Left"),
					// Only if topdown.
					Y = Input.GetActionStrength("Down") - Input.GetActionStrength("Up"),
				};
				
				if(playerSettings.gameType == PlayerSettings.GameType.SideScroller) 
					joyPos.Y = 0;
				
				//RotateObjects(joyPos);
				break;
		}

		MoveAndSlide();
	}

	Vector2 GetControllerStrength(){
		Vector2 joyPos = new Vector2
		{
			X = Input.GetActionStrength("Right") - Input.GetActionStrength("Left"),
			// Only if topdown.
			Y = Input.GetActionStrength("Down") - Input.GetActionStrength("Up"),
		};

		return joyPos;
	}

	void Initialize(PlayerSettings data){
		playerSettings = data;
	}

	// For keyboard controls. Need to look at how to get the latest button prees.
	// Keep the sprite delay when you move, it creates a sense that you're going in the diretion.
	void Movement(){
        // If x and y are not 0 then you move faster.
        if(Input.IsActionPressed("Up")){
            direction.Y = -1;
        }else if(Input.IsActionPressed("Down")){
            direction.Y = 1;
        }
        if(Input.IsActionPressed("Right")){
            direction.X = 1;
        }else if(Input.IsActionPressed("Left")){
            direction.X = -1;
        }
    }

	void RotateObjects(Vector2 pos){
		foreach(Node2D node in NodesToRotate){
			node.LookAt(pos);
		}
	}

	void ScaleObjects(){
		foreach(Node2D node in NodesToRotate){
			node.Scale = new Vector2(node.Scale.X, InAngleRange() ? -1 : 1);
		}
		}

	bool InAngleRange(){
		// If an angle is bigger than 360. Mod by 360 to get the remainder.
		angle = angle%360;
		if((-90 >= angle && angle >= -270) 
		|| (90 <= angle && angle <= 270)){
			return true;
		}else{
			return false;
		}
	}
}
