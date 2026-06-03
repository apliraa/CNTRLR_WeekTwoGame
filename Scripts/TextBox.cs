using Godot;
using System;
using System.Collections.Generic;

public partial class TextBox : CanvasLayer
{
	private Label text;
	private Label endText;
	private MarginContainer textBoxContainer;
	private Tween currentTween;
	
	public enum TextBoxState{
		Idle,
		Reading,
		Finished
	}
	
	private TextBoxState currentState = TextBoxState.Idle;
	private Queue<string> dialogueQueue = new Queue<string>();
	public bool TextBoxCanOpen => currentState != TextBoxState.Idle;
	
	public override void _Ready(){
		
		ChangeState(TextBoxState.Idle);
		text = GetNode<Label>("%Text");
		//text.VisibleCharactersBehavior = TextServer.VisibleCharactersBehavior.CharsAfterShaping;
		textBoxContainer = GetNode<MarginContainer>("%TextBoxContainer");
		endText = GetNode<Label>("%EndText");
		HideTextBox();
		
		}
			
	

	
	public override void _Process(double delta){
		
		switch(currentState){
			case TextBoxState.Idle:
				
				break;
			case TextBoxState.Reading:
					if(Input.IsActionJustPressed("Accept")){
						KillTweening();
						text.VisibleCharacters = -1;
						endText.SelfModulate = new Color(1, 1, 1, 1);
						//endText.Visible = true;
						ChangeState(TextBoxState.Finished);
					}
				break;
			case TextBoxState.Finished:
				if(Input.IsActionJustPressed("Accept")){
						IsDialogueEnded();
						//HideTextBox();
				}
				break;
				}
		
	}
	
	public void HideTextBox(){
		text.Text = "";
		endText.SelfModulate = new Color(1, 1, 1, 0);
		//endText.Visible = false;
		textBoxContainer.Hide();
		ChangeState(TextBoxState.Idle);
	}
	
	public void ShowTextBox(){
		textBoxContainer.Show();
	}
	
	public void ShowNextDialogue(){
		string nextDialogue = dialogueQueue.Dequeue();
		
		text.Text = nextDialogue;
		ChangeState(TextBoxState.Reading);
		text.VisibleCharacters = 0;
		endText.SelfModulate = new Color(1, 1, 1, 0);
		//endText.Visible = false;
		//ShowTextBox();
		
		//da um Kill em um Tween de um texto anterior
		KillTweening();
			
		currentTween = CreateTween();
		float timePerChar = 0.05f;
		float tweenDuration = nextDialogue.Length * timePerChar;

		currentTween.TweenProperty(text, "visible_characters", nextDialogue.Length, tweenDuration);
		
		currentTween.Finished += WhenTweeningEnds;
		
	}
	
	public void StartDialogue(string[] newDialogue){
		dialogueQueue.Clear();
		
		foreach (string speach in newDialogue){
			dialogueQueue.Enqueue(speach);
		}
		ShowTextBox();
		ShowNextDialogue();
	}
	
	public void WhenTweeningEnds(){
			endText.SelfModulate = new Color(1, 1, 1, 1);
			//endText.Visible = true;
			ChangeState(TextBoxState.Finished);
		}
		
	public void IsDialogueEnded(){
			if(dialogueQueue.Count > 0){
				ShowNextDialogue();
			}else{
				HideTextBox();
			}
			
		}
		
	public void ChangeState(TextBoxState nextState){
		currentState = nextState;
		switch(currentState){
			case TextBoxState.Idle:
				break;
			case TextBoxState.Reading:
				break;
			case TextBoxState.Finished:
				break;
		}
	}
		
		public void KillTweening(){
			if (currentTween != null && currentTween.IsValid())
					{
						currentTween.Kill();
					}
		}
		
	}
	
	
