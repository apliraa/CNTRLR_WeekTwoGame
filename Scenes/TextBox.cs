using Godot;
using System;

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
	
	public override void _Ready(){
		
		ChangeState(TextBoxState.Idle);
		text = GetNode<Label>("%Text");
		textBoxContainer = GetNode<MarginContainer>("%TextBoxContainer");
		endText = GetNode<Label>("%EndText");
		HideTextBox();
		
		AddText("Teste texto texto teste!");
		
	}

	
	public override void _Process(double delta){
		
		switch(currentState){
			case TextBoxState.Idle:
				
				break;
			case TextBoxState.Reading:
					if(Input.IsActionJustPressed("Accept")){
						KillTweening();
						text.VisibleCharacters = -1;
						endText.Visible = true;
						ChangeState(TextBoxState.Finished);
					}
				break;
			case TextBoxState.Finished:
				if(Input.IsActionJustPressed("Accept")){
						HideTextBox();
				}
				break;
				}
		
	}
	
	public void HideTextBox(){
		text.Text = "";
		endText.Visible = false;
		textBoxContainer.Hide();
		ChangeState(TextBoxState.Idle);
	}
	
	public void ShowTextBox(){
		textBoxContainer.Show();
	}
	
	public void AddText(string nextText){
		text.Text = nextText;
		ChangeState(TextBoxState.Reading);
		text.VisibleCharacters = 0;
		endText.Visible = false;
		ShowTextBox();
		
		//da um Kill em um Tween de um texto anterior
		KillTweening();
			
		currentTween = CreateTween();
		float timePerChar = 0.05f;
		float tweenDuration = nextText.Length * timePerChar;

		currentTween.TweenProperty(text, "visible_characters", nextText.Length, tweenDuration);
		
		currentTween.Finished += WhenTweeningEnds;
		
	}
	
	public void WhenTweeningEnds(){
			endText.Visible = true;
			ChangeState(TextBoxState.Finished);
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
	
	
