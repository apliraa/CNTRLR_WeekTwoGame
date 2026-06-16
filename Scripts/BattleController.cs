using Godot;

// PARA BOTÕES DE UI NO FUTURO:
//   Chame ReceivePlayerAction() de qualquer botão. O _Input abaixo pode ser removido.

public partial class BattleController : Node
{
	// Inspector 
	[ExportGroup("Batalha")]
	[Export] public float TurnTimeLimit   = 5.0f; // 0 = sem limite
	[Export] public float PlayerMoveSpeed = 200f;
	[Export] public int   ChipDamage      = 0;    // dano ao defender com sucesso

	// Estado 
	public enum BattleState { Idle, MovingPlayer, WaitingInput, Resolving, BattleOver }
	private BattleState _state = BattleState.Idle;

	// Referências 
	private Player     _player;
	private EnemyMage  _mage;
	private Marker2D   _battlePosition;
	private Timer      _turnTimer;
	private Tween      _moveTween;

	//Ações do turno 
	private BattleAction _playerAction = BattleAction.None;
	private BattleAction _mageAction   = BattleAction.None;

	// Inicialização 
	public override void _Ready()
	{
		_turnTimer         = new Timer();
		_turnTimer.OneShot = true;
		_turnTimer.Timeout += _OnTurnTimerTimeout;
		AddChild(_turnTimer);

		_battlePosition = GetParent().GetNodeOrNull<Marker2D>("BattlePosition");
		if (_battlePosition == null)
			GD.PushWarning("[BattleController] 'BattlePosition' não encontrado. Player não será movido.");
	}


	public void StartBattle(Player player, EnemyMage mage)
	{
		if (_state != BattleState.Idle) return;

		_player = player;
		_mage   = mage;

		// Reseta HP e mana de ambos para os máximos configurados
		_player.CurrentHp = _player.MaxHp;
		_player.CurrentMp = _player.MaxMp;
		_mage.currentHp   = _mage.maxHp;
		_mage.currentMp   = _mage.maxMp;

		_player.SetMovementLocked(true);

		GD.Print("=== BATALHA INICIADA ===");
		_PrintStats();

		_ChangeState(BattleState.MovingPlayer);
	}

	// Chamado pelo _Input ou por botões de UI no futuro.
	public void ReceivePlayerAction(BattleAction action)
	{
		if (_state != BattleState.WaitingInput) return;

		if (action == BattleAction.Attack && _player.CurrentMp < _player.MpCostPerShot)
		{
			GD.Print("[Batalha] Player sem mana para atacar!");
			return;
		}

		_playerAction = action;
		GD.Print($"[Batalha] Player escolheu: {action}");
		_turnTimer.Stop();
		_ResolveTurn();
	}

	// Máquina de estados 

	private void _ChangeState(BattleState next)
	{
		_state = next;
		switch (_state)
		{
			case BattleState.MovingPlayer:
				_MovePlayerToBattlePosition();
				break;

			case BattleState.WaitingInput:
				_playerAction = BattleAction.None;
				_mageAction   = BattleAction.None;
				GD.Print($"\n--- Turno  (1=Atacar  2=Defender  3=Recarregar  — {TurnTimeLimit}s) ---");
				_PrintStats();
				if (TurnTimeLimit > 0f)
				{
					_turnTimer.WaitTime = TurnTimeLimit;
					_turnTimer.Start();
				}
				break;

			case BattleState.Resolving:
				_ResolveActions();
				break;

			case BattleState.BattleOver:
				_EndBattle();
				break;
		}
	}

	// Movimento do player

	private void _MovePlayerToBattlePosition()
	{
		if (_battlePosition == null) { _ChangeState(BattleState.WaitingInput); return; }

		Vector2 target   = _battlePosition.GlobalPosition;
		float   distance = _player.GlobalPosition.DistanceTo(target);
		float   duration = distance / PlayerMoveSpeed;

		if (_moveTween != null && _moveTween.IsValid()) _moveTween.Kill();

		_moveTween = CreateTween();
		_moveTween.TweenProperty(_player, "global_position", target, duration)
				  .SetTrans(Tween.TransitionType.Sine)
				  .SetEase(Tween.EaseType.InOut);
		_moveTween.Finished += () => _ChangeState(BattleState.WaitingInput);
	}

	// Input 
	// InputMap esperado: battle_attack→1  battle_defend→2  battle_recharge→3

	public override void _Input(InputEvent @event)
	{
		if (_state != BattleState.WaitingInput) return;

		if      (@event.IsActionPressed("battle_attack"))   ReceivePlayerAction(BattleAction.Attack);
		else if (@event.IsActionPressed("battle_defend"))   ReceivePlayerAction(BattleAction.Defend);
		else if (@event.IsActionPressed("battle_recharge")) ReceivePlayerAction(BattleAction.Recharge);
	}

	//Timer 

	private void _OnTurnTimerTimeout()
	{
		if (_state != BattleState.WaitingInput) return;
		GD.Print("[Batalha] Tempo esgotado — player não age.");
		_playerAction = BattleAction.None;
		_ResolveTurn();
	}

	// Resolução do combate

	private void _ResolveTurn()
	{
		// O inimigo decide por conta própria — IA e restrições de mana ficam nele.
		_mageAction = _mage.ChooseAction();
		GD.Print($"[Batalha] Inimigo escolheu: {_mageAction}");

		_ChangeState(BattleState.Resolving);
	}

	private void _ResolveActions()
	{
		GD.Print($"[Batalha] Resolvendo: Player={_playerAction} vs Inimigo={_mageAction}");

		bool playerAttacked = _playerAction == BattleAction.Attack;
		bool mageAttacked   = _mageAction   == BattleAction.Attack;
		bool playerDefended = _playerAction == BattleAction.Defend;
		bool mageDefended   = _mageAction   == BattleAction.Defend;

		//Ataque do player 
		if (playerAttacked)
		{
			_player.CurrentMp = Mathf.Max(0, _player.CurrentMp - _player.MpCostPerShot);
			int dmg = mageDefended ? ChipDamage : _player.AttackPower;
			_mage.TakeDamage(dmg);
			GD.Print(mageDefended
				? $"[Batalha] Inimigo defendeu! Tomou {dmg} de chip damage."
				: $"[Batalha] Player atacou! Inimigo tomou {dmg} de dano.");
		}

		// Ataque do inimigo 
		if (mageAttacked)
		{
			_mage.SpendMp();
			int dmg = playerDefended ? ChipDamage : _mage.GetAttackValue();
			_player.CurrentHp = Mathf.Max(0, _player.CurrentHp - dmg);
			GD.Print(playerDefended
				? $"[Batalha] Player defendeu! Tomou {dmg} de chip damage."
				: $"[Batalha] Inimigo atacou! Player tomou {dmg} de dano.");
		}

		// Recarga 
		if (_playerAction == BattleAction.Recharge)
		{
			_player.CurrentMp = Mathf.Min(_player.MaxMp, _player.CurrentMp + _player.MpRechargeAmount);
			GD.Print($"[Batalha] Player recarregou. MP: {_player.CurrentMp}/{_player.MaxMp}");
		}
		if (_mageAction == BattleAction.Recharge)
		{
			_mage.Recharge();
			GD.Print($"[Batalha] Inimigo recarregou. MP: {_mage.currentMp}/{_mage.maxMp}");
		}

		_PrintStats();

		if (_player.CurrentHp <= 0 || _mage.currentHp <= 0)
		{
			_ChangeState(BattleState.BattleOver);
			return;
		}

		_ChangeState(BattleState.WaitingInput);
	}

	//Fim de batalha 

	private void _EndBattle()
	{
		string result = _player.CurrentHp <= 0 ? "INIMIGO VENCEU" : "PLAYER VENCEU";
		GD.Print($"\n=== BATALHA ENCERRADA — {result} ===");
		_PrintStats();
		_player.SetMovementLocked(false);
	}

	//Status 

	private void _PrintStats()
	{
		GD.Print($"  Player  → HP: {_player.CurrentHp}/{_player.MaxHp}  MP: {_player.CurrentMp}/{_player.MaxMp}");
		GD.Print($"  Inimigo → HP: {_mage.currentHp}/{_mage.maxHp}  MP: {_mage.currentMp}/{_mage.maxMp}");
	}
}

public enum BattleAction { None, Attack, Defend, Recharge }
