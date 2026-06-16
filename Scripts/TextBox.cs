using Godot;
using System.Collections.Generic;


public partial class TextBox : CanvasLayer
{
	//Signals 
	[Signal] public delegate void DialogueStartedEventHandler();
	[Signal] public delegate void DialogueFinishedEventHandler();

	//Estado 
	public enum TextBoxState { Idle, Reading, Finished }
	private TextBoxState currentState = TextBoxState.Idle;
	public bool isOpen => currentState != TextBoxState.Idle;

	//Fila 
	private Queue<string> dialogueQueue = new Queue<string>();

	//Referências
	private Label text;
	private Label endText;
	private MarginContainer textBoxContainer;
	private Tween currentTween;

	public override void _Ready()
	{
		text               = GetNode<Label>("%Text");
		textBoxContainer   = GetNode<MarginContainer>("%TextBoxContainer");
		endText            = GetNode<Label>("%EndText");
		HideTextBox();
	}

	public override void _Process(double delta)
	{
		switch (currentState)
		{
			case TextBoxState.Reading:
				if (Input.IsActionJustPressed("Accept"))
				{
					KillTweening();
					text.VisibleCharacters = -1;
					endText.SelfModulate = new Color(1, 1, 1, 1);
					ChangeState(TextBoxState.Finished);
				}
				break;

			case TextBoxState.Finished:
				if (Input.IsActionJustPressed("Accept"))
					IsDialogueEnded();
				break;
		}
	}



	public void StartDialogue(string[] newDialogue)
	{
		dialogueQueue.Clear();
		foreach (string line in newDialogue)
			dialogueQueue.Enqueue(line);

		ShowTextBox();
		ShowNextDialogue();
		EmitSignal(SignalName.DialogueStarted); 
	}

	public void HideTextBox()
	{
		text.Text = "";
		endText.SelfModulate = new Color(1, 1, 1, 0);
		textBoxContainer.Hide();
		ChangeState(TextBoxState.Idle);
	}

	//Internos 

	public void ShowTextBox()
	{
		textBoxContainer.Show();
	}

	public void ShowNextDialogue()
	{
		string nextDialogue = dialogueQueue.Dequeue();
		text.Text = nextDialogue;
		text.VisibleCharacters = 0;
		endText.SelfModulate = new Color(1, 1, 1, 0);
		ChangeState(TextBoxState.Reading);

		KillTweening();
		currentTween = CreateTween();
		float tweenDuration = nextDialogue.Length * 0.05f;
		currentTween.TweenProperty(text, "visible_characters", nextDialogue.Length, tweenDuration);
		currentTween.Finished += WhenTweeningEnds;
	}

	public void WhenTweeningEnds()
	{
		endText.SelfModulate = new Color(1, 1, 1, 1);
		ChangeState(TextBoxState.Finished);
	}

	public void IsDialogueEnded()
	{
		if (dialogueQueue.Count > 0)
		{
			ShowNextDialogue();
		}
		else
		{
			HideTextBox();
			EmitSignal(SignalName.DialogueFinished); 
		}
	}

	public void ChangeState(TextBoxState nextState) => currentState = nextState;

	public void KillTweening()
	{
		if (currentTween != null && currentTween.IsValid())
			currentTween.Kill();
	}
}
