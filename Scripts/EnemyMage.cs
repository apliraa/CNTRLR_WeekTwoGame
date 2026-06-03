using Godot;
using System;

public partial class EnemyMage : StaticBody2D 
{
	private bool playerInArea = false;
	private TextBox sceneTextBox;
	private GameManager gameManager;
	private CollisionShape2D talkBoxCollision;

	public override void _Ready()
	{
		sceneTextBox= GetTree().GetFirstNodeInGroup("UI") as TextBox;
		gameManager = GetNode<GameManager>("/root/GameManager") ;
		talkBoxCollision = GetNode<CollisionShape2D>("TalkBox/CollisionShape2D");
		
	}

	public void _on_talk_box_body_entered(Node2D body)
	{
		if (body is Player)
		{
			playerInArea = true;
			GD.Print("Player chegou perto do NPC!");
		}
	}

	public void _on_talk_box_body_exited(Node2D body)
	{
		if (body is Player)
		{
			playerInArea = false;
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (playerInArea && @event.IsActionPressed("Interact") && !sceneTextBox.TextBoxCanOpen)
		{
			string[] currentDialogue = CurrentDialogue();
			sceneTextBox.StartDialogue(currentDialogue);
		}
	}

	private string[] CurrentDialogue()
	{
		switch (gameManager.eventNumber)
		{
			case 1:
				return new string[] { 
					"Olá, forasteiro!", 
					"Eu perdi minha espada na floresta.", 
					"Você poderia encontrá-la para mim?" ,
				};
			case 2:
				return new string[] { 
					"Pelos deuses! Você a encontrou!", 
                    "Muito obrigado, pegue este ouro como recompensa." 
				};
			default:
				return new string[] { 
                    "Um belo dia para descansar, não acha?" 
				};
		}
	}
}
