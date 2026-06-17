using Godot;


public partial class Player : CharacterBody2D
{
	// Movimento 
	public const float Speed     = 300f;
	public const float JumpSpeed = -400f;
	public float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	// Stats de Batalha 
	[ExportGroup("Stats de Batalha")]
	[Export] public int MaxHp          = 100;
	[Export] public int MaxMp          = 30;
	[Export] public int AttackPower    = 25;
	[Export] public int MpCostPerShot  = 10;
	[Export] public int MpRechargeAmount = 10;
	
	//Visual
	private const string AnimIdle = "idle";
	private const string AnimWalk = "walk"; //não existe ainda
	private const string AnimJump = "jump"; // não existe ainda
	private AnimatedSprite2D _sprite;


	// Estado de runtime — modificados pelo BattleController durante a batalha.
	public int CurrentHp { get; set; }
	public int CurrentMp { get; set; }

	// Estado interno 
	private Door _nearbyDoor = null;
	private int  _lockCount  = 0;
	public bool IsMovementLocked => _lockCount > 0;

	public override void _Ready()
	{
		CurrentHp = MaxHp;
		CurrentMp = MaxMp;

		_sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		if (_sprite == null)
			GD.PushWarning("[Player] 'AnimatedSprite2D' não encontrado. Sem animação será tocada.");
			
		var textBox = GetTree().GetFirstNodeInGroup("UI") as TextBox;
		if (textBox != null)
		{
			textBox.DialogueStarted  += () => SetMovementLocked(true);
			textBox.DialogueFinished += () => SetMovementLocked(false);
		}
		else
		{
			GD.PushWarning("[Player] TextBox não encontrada no grupo 'UI'.");
		}

		CallDeferred(MethodName.SnapToDoor);
	}

	//Travamento de movimento
	public void SetMovementLocked(bool locked)
	{
		_lockCount += locked ? 1 : -1;
		_lockCount  = Mathf.Max(0, _lockCount);
		if (IsMovementLocked) Velocity = Vector2.Zero;
	}

	//Física
	public override void _PhysicsProcess(double delta)
	{
		if (IsMovementLocked)
		{
			Vector2 vel = Velocity;
			if (!IsOnFloor()) vel.Y += Gravity * (float)delta;
			vel.X   = 0f;
			Velocity = vel;
			MoveAndSlide();
			_UpdateAnimation(0f);
			return;
		}

		Vector2 velocity = Velocity;
		float direction  = Input.GetAxis("MoveLeft", "MoveRight");
		velocity.X = direction != 0f ? direction * Speed : 0f;
		if (!IsOnFloor()) velocity.Y += Gravity * (float)delta;
		if (IsOnFloor() && Input.IsActionJustPressed("Jump")) velocity.Y = JumpSpeed;
		Velocity = velocity;
		MoveAndSlide();
		_UpdateAnimation(direction);

	}
	
	//Animação
	private void _UpdateAnimation(float direction)
	{
		if (_sprite == null) return;
 
		// Vira o sprite horizontalmente conforme a direção do movimento.
		if (direction > 0f)      _sprite.FlipH = false;
		else if (direction < 0f) _sprite.FlipH = true;
 
		string targetAnim;
		if (!IsOnFloor())
			 targetAnim = AnimJump;
		else if (direction != 0f)
			targetAnim = AnimWalk;
		else
			targetAnim = AnimIdle;
 
		// Só chama Play se a animação existir no SpriteFrames, e se ainda não
		// estiver tocando ESSA animação. Checamos IsPlaying() além do nome:
		// o Animation pode já conter o nome certo sem nunca ter sido iniciado
		// (ex: Autoplay desligado), o que travaria o sprite no frame estático.
		if (_sprite.SpriteFrames != null && _sprite.SpriteFrames.HasAnimation(targetAnim))
		{
			bool alreadyPlayingTarget = _sprite.IsPlaying() && _sprite.Animation == targetAnim;
			if (!alreadyPlayingTarget)
				_sprite.Play(targetAnim);
		}
	}

	//Input 
	public override void _Input(InputEvent @event)
	{
		if (!IsMovementLocked && _nearbyDoor != null && @event.IsActionReleased("Interact"))
			_nearbyDoor.TryEnter();
	}

	//Door
	public void SetNearbyDoor(Door door)  => _nearbyDoor = door;
	public void ClearNearbyDoor(Door door)
	{
		if (_nearbyDoor == door) _nearbyDoor = null;
	}

	//Spawn 
	private void SnapToDoor()
	{
		var gm       = GetNode<GameManager>("/root/GameManager");
		int targetID = gm.targetDoorID;
		foreach (Node node in GetTree().GetNodesInGroup("door"))
		{
			if (node is Door door && door.myDoorID == targetID)
			{
				GlobalPosition = door.GlobalPosition;
				_nearbyDoor    = null;
				return;
			}
		}
	}
}
