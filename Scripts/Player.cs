using Godot;
using System;

public partial class Player : CharacterBody2D
{
	public const float speed = 300f;
	public const float jumpSpeed = -400f;
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	public override void _PhysicsProcess(double delta)
	{
		//movimentação
		Vector2 velocity = Velocity; //variavel do godot (Velocity)
		Vector2 direction = Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");		
	
		if (direction != Vector2.Zero){velocity.X = direction.X * speed;}
		else{velocity.X = 0f;}
		
		
		//gravidade no pulo
		if (!IsOnFloor()){ velocity.Y += gravity * (float)delta; }
		
		//pulo
		if(IsOnFloor() && Input.IsActionJustPressed("Jump")){  velocity.Y = jumpSpeed;  }
		
		

		Velocity = velocity;
		MoveAndSlide();
	}
}
