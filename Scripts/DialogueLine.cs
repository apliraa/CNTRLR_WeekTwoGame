using Godot;

// Resource de uma única linha de diálogo.
// Extensível no futuro para condições, escolhas do jogador e flags de estado.
[GlobalClass]
public partial class DialogueLine : Resource
{
	[Export(PropertyHint.MultilineText)]
	public string text = "";
}
