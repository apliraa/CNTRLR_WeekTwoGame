using Godot;
using Godot.Collections;

// Resource de diálogo configurável no Inspector.

// COMO USAR:
//   1. No FileSystem do Godot: botão direito > New Resource > DialogueData > salve como .tres
//   2. Dentro do .tres, adicione itens ao array "lines" (cada um é um DialogueLine)
//   3. Arraste o .tres para o campo "dialogueData" do DialogueCaller no Inspector
[GlobalClass]
public partial class DialogueData : Resource
{
	[Export] public Array<DialogueLine> lines = new Array<DialogueLine>();
}
