using Godot;

// NPC simples: só recebe diálogo e exibe a animação idle.
// Não inicia batalha, não tem stats — DialogueSequenceFinished não faz nada.
//
// ESTRUTURA ESPERADA NA CENA:
//   Npc (StaticBody2D)
//   ├── AnimatedSprite2D
//   ├── CollisionShape2D
//   ├── TalkBox (Area2D)         ← zona de detecção do player
//   │     └── CollisionShape2D
//   └── DialogueCaller (Node)
//
// CONEXÃO DE SINAIS DO TalkBox (Area2D, no Inspector):
//   body_entered → Npc._on_talk_box_body_entered
//   body_exited  → Npc._on_talk_box_body_exited
//
// MUDANÇA: os sinais do TalkBox agora são conectados ao próprio Npc
// (em vez do DialogueCaller), porque o editor do Godot não estava listando
// o DialogueCaller — um Node puro — como destino possível na interface de
// "Connect a Signal". O Npc recebe o sinal e repassa para o DialogueCaller.

public partial class Npc : StaticBody2D
{
	[ExportGroup("Diálogo")]
	[Export] public DialogueData dialogueData;
	[Export] public bool dialogueRepeatable = false;

	private const string AnimIdle = "idle";

	protected AnimatedSprite2D _sprite;
	protected DialogueCaller   _dialogueCaller;

	public override void _Ready()
	{
		_sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		if (_sprite != null)
		{
			if (_sprite.SpriteFrames != null && _sprite.SpriteFrames.HasAnimation(AnimIdle))
				_sprite.Play(AnimIdle);
			else
				GD.PushWarning($"[Npc] '{Name}': animação '{AnimIdle}' não encontrada no SpriteFrames.");
		}
		else
		{
			GD.PushWarning($"[Npc] '{Name}': nó 'AnimatedSprite2D' não encontrado.");
		}

		_dialogueCaller = GetNodeOrNull<DialogueCaller>("DialogueCaller");
		if (_dialogueCaller != null)
		{
			_dialogueCaller.SetDialogueData(dialogueData, dialogueRepeatable);
			// O NPC não faz nada ao terminar o diálogo — só conversa.
			_dialogueCaller.DialogueSequenceFinished += _OnDialogueFinished;
		}
		else
		{
			GD.PushWarning($"[Npc] '{Name}': nó 'DialogueCaller' não encontrado.");
		}
	}

	private void _OnDialogueFinished()
	{
		// Intencionalmente vazio: o NPC só conversa, não dispara batalha
		// nem qualquer outra ação.
	}

	// ── Sinais do TalkBox (conectados aqui no Inspector) ──────────────────────
	// Repassam diretamente para os métodos já existentes no DialogueCaller.

	public void _on_talkbox_body_entered(Node2D body)
	{
		GD.Print($"[DIAGNÓSTICO] _on_talk_box_body_entered chamado. Body: {body.Name} ({body.GetType()})");
		_dialogueCaller?._on_talk_box_body_entered(body);
	}

	public void _on_talkbox_body_exited(Node2D body)
	{
		GD.Print($"[DIAGNÓSTICO] _on_talk_box_body_exited chamado. Body: {body.Name}");
		_dialogueCaller?._on_talk_box_body_exited(body);
	}
}
