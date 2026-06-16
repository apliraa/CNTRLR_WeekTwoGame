using Godot;


public partial class EnemyMage : StaticBody2D
{
	//Inspector 

	[ExportGroup("Visual")]
	[Export] public Color mageColor = new Color(1, 1, 1);

	[ExportGroup("Diálogo")]
	[Export] public DialogueData dialogueData;
	[Export] public bool dialogueRepeatable = false;

	[ExportGroup("Stats de Batalha")]
	[Export] public int maxHp            = 100;
	[Export] public int maxMp            = 30;
	[Export] public int attackPower      = 25;
	[Export] public int mpCostPerShot    = 10;
	[Export] public int mpRechargeAmount = 10;

	//Runtime 
	public int currentHp { get; set; }
	public int currentMp { get; set; }

	//Referências 
	protected Sprite2D         _sprite;
	protected DialogueCaller   _dialogueCaller;
	protected BattleController _battleController;

	//Inicialização 
	public override void _Ready()
	{
		currentHp = maxHp;
		currentMp = maxMp;

		_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		if (_sprite != null) _sprite.Modulate = mageColor;

		_dialogueCaller = GetNodeOrNull<DialogueCaller>("DialogueCaller");
		if (_dialogueCaller != null)
		{
			_dialogueCaller.SetDialogueData(dialogueData, dialogueRepeatable);
			_dialogueCaller.DialogueSequenceFinished += _OnDialogueFinished;
		}
		else
			GD.PushWarning($"[EnemyMage] '{Name}': 'DialogueCaller' não encontrado.");

		_battleController = GetNodeOrNull<BattleController>("BattleController");
		if (_battleController == null)
			GD.PushWarning($"[EnemyMage] '{Name}': 'BattleController' não encontrado.");

		OnReady();
	}

	protected virtual void OnReady() { }


	public virtual int TakeDamage(int damage)
	{
		currentHp = Mathf.Max(0, currentHp - damage);
		return damage;
	}

	public virtual int GetAttackValue() => attackPower;

	public virtual void SpendMp()
	{
		currentMp = Mathf.Max(0, currentMp - mpCostPerShot);
	}

	public virtual void Recharge()
	{
		currentMp = Mathf.Min(maxMp, currentMp + mpRechargeAmount);
	}

	// ── Decisão do enemy no combate
	public virtual BattleAction ChooseAction()
	{
		if (currentMp < mpCostPerShot)
		{
			GD.Print($"[{Name}] Sem mana para atacar.");
			return GD.Randf() < 0.5f ? BattleAction.Defend : BattleAction.Recharge;
		}

		float roll = GD.Randf();
		if (roll < 0.55f) return BattleAction.Attack;
		if (roll < 0.80f) return BattleAction.Defend;
		return BattleAction.Recharge;
	}

	//Começar batalha
	private void _OnDialogueFinished()
	{
		if (_battleController == null) return;

		var player = GetTree().GetFirstNodeInGroup("player") as Player;
		if (player == null)
		{
			GD.PushError("[EnemyMage] Player não encontrado no grupo 'player'.");
			return;
		}

		_battleController.StartBattle(player, this);
	}
}
