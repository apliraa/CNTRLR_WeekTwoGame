using Godot;

// CONFIGURAR NO INSPECTOR:
//   myDoorID     = ID desta porta (Player procura por este número ao nascer)
//   nextLevelNumber = fase de destino (0 = não configurado, vai dar erro)
//   targetDoorID = ID da porta de destino na fase seguinte

public partial class Door : Node2D
{
	[Export] public int myDoorID = 0;
	[Export] public int nextLevelNumber = 0;
	[Export] public int targetDoorID = 0;

	private GameManager _gameManager;

	public override void _Ready()
	{
		_gameManager = GetNode<GameManager>("/root/GameManager");

		if (nextLevelNumber == 0)
			GD.PushWarning($"[Door] '{Name}': nextLevelNumber não configurado no Inspector!");
	}

	public void TryEnter()
	{
		if (nextLevelNumber == 0)
		{
			GD.PrintErr($"[Door] '{Name}': nextLevelNumber é 0. Configure no Inspector.");
			return;
		}

		_gameManager.targetDoorID = targetDoorID;
		_gameManager.ChangeToLevel(nextLevelNumber);
	}

	public void _on_door_box_body_entered(Node2D body)
	{
		
		if (body is Player player)
			player.SetNearbyDoor(this);
	}

	public void _on_door_box_body_exited(Node2D body)
	{
		if (body is Player player)
			player.ClearNearbyDoor(this);
	}
}
