using Godot;
using System;

public partial class EnemyMage : StaticBody2D 
{
	[Export] public Color mageColor = new Color(1, 1, 1);
	[Export] public int mageID = 1;
	private int mageDialogueState = 0;
	
	private bool playerInArea = false;
	private TextBox sceneTextBox;
	private GameManager gameManager;
	private CollisionShape2D talkBoxCollision;

	public override void _Ready()
	{
		sceneTextBox= GetTree().GetFirstNodeInGroup("UI") as TextBox;
		gameManager = GetNode<GameManager>("/root/GameManager") ;
		talkBoxCollision = GetNode<CollisionShape2D>("TalkBox/CollisionShape2D");
		GetNode<Sprite2D>("Sprite2D").Modulate = mageColor;
		
	}

	public void _on_talk_box_body_entered(Node2D body)
	{
		if (body is Player)
		{
			playerInArea = true;
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
		if(mageID == 1){
		switch (mageDialogueState)
		{
			case 0:
				mageDialogueState = 1;
				return new string[] { 
					"Olá, Mago Fracote.",
					"Preparado para sofrer?!",
					"Muahahahahahaha!!"
				};
			case 1:
				return new string[] { 
					"Você já me derrotou...",
					"Vá embora!"
				};
			default:
			   	 return new string[] { "." };
			}
		}
		
		if(mageID == 2){
			switch(mageDialogueState){
				case 0:
					return new string[]{
						"Bom dia, amigo.",
						"Vamos treinar?"
					};
				default:
					return new string[] { "." };
			}
		}
		return new string[] { "Erro: Mago ID não encontrado." };
		
	}
}
