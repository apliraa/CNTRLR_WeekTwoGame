using Godot;
using System;

public partial class GameManager : Node
{
	public int eventNumber = 1;

public override void _Ready()
	{
		GD.Print("O GameManager global acordou!");
	}
}
