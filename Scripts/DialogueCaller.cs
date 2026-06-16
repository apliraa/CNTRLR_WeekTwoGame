using Godot;

// Componente de disparo de diálogo.
// Deve ser filho direto de qualquer nó que precise falar (EnemyMage, NPC, placa...).
//
// SINAL:
//   DialogueSequenceFinished — emitido quando a TextBox fecha após a última linha.
//   O pai (ex: EnemyMage) conecta este sinal para iniciar batalha ou qualquer outra ação.


public partial class DialogueCaller : Node
{
	[Signal] public delegate void DialogueSequenceFinishedEventHandler();

	private bool        _repeatable    = false;
	private DialogueData _dialogueData = null;
	private bool        _playerInArea  = false;
	private bool        _hasTriggered  = false;
	private TextBox     _sceneTextBox  = null;

	public override void _Ready()
	{
		_sceneTextBox = GetTree().GetFirstNodeInGroup("UI") as TextBox;

		if (_sceneTextBox == null)
		{
			GD.PushError("[DialogueCaller] TextBox não encontrada no grupo 'UI'.");
			return;
		}

		// Quando a TextBox fecha, verifica se foi o nosso diálogo e emite o sinal próprio.
		_sceneTextBox.DialogueFinished += _OnTextBoxFinished;
	}

	// ── API chamada pelo pai (EnemyMage, NPC, etc.) ───────────────────────────

	public void SetDialogueData(DialogueData data, bool repeatable)
	{
		_dialogueData = data;
		_repeatable   = repeatable;

		if (_dialogueData == null)
			GD.PushWarning($"[DialogueCaller] '{GetParent().Name}': dialogueData é null.");
	}

	// ── Detecção de proximidade ───────────────────────────────────────────────

	public void _on_talk_box_body_entered(Node2D body)
	{
		if (body is Player) _playerInArea = true;
	}

	public void _on_talk_box_body_exited(Node2D body)
	{
		if (body is Player) _playerInArea = false;
	}

	// ── Input ─────────────────────────────────────────────────────────────────

	public override void _Input(InputEvent @event)
	{
		if (!_playerInArea)                                           return;
		if (!@event.IsActionPressed("Interact"))                      return;
		if (_sceneTextBox == null || _sceneTextBox.isOpen)            return;
		if (_hasTriggered && !_repeatable)                            return;
		if (_dialogueData == null || _dialogueData.lines.Count == 0)  return;

		_TriggerDialogue();
	}

	// ── Internos ──────────────────────────────────────────────────────────────

	private void _TriggerDialogue()
	{
		_hasTriggered = true;

		string[] lines = new string[_dialogueData.lines.Count];
		for (int i = 0; i < _dialogueData.lines.Count; i++)
			lines[i] = _dialogueData.lines[i].text;

		_sceneTextBox.StartDialogue(lines);
	}

	private void _OnTextBoxFinished()
	{
		// Só emite se foi este DialogueCaller que iniciou o diálogo atual.
		// _hasTriggered garante isso: só é true após _TriggerDialogue().
		if (_hasTriggered)
			EmitSignal(SignalName.DialogueSequenceFinished);
	}
}
