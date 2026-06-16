using Godot;
using Godot.Collections; 

public partial class GameManager : Node
{
	public int LevelNumber = 1;
	public int targetDoorID = 0;

	public Dictionary<int, string> levelPaths = new Dictionary<int, string>();

	public override void _Ready()
	{
		levelPaths.Add(1, "res://Scenes/gameTeste.tscn");
		levelPaths.Add(2, "res://Scenes/stage_2.tscn");

		// levelPaths.Add(3, "res://Cenas/stage_3.tscn"); 
	}

	// A porta vai chamar isso aqui!
	public void ChangeToLevel(int nextLevel)
	{
		// Verifica se o número da fase que a porta pediu existe no nosso Dicionário
		if (levelPaths.ContainsKey(nextLevel))
		{
			LevelNumber = nextLevel;
			GetTree().ChangeSceneToFile(levelPaths[nextLevel]);
		}
		else
		{
			GD.PrintErr("Erro: O caminho para o level " + nextLevel + " não foi configurado no GameManager!");
		}
	}
}
