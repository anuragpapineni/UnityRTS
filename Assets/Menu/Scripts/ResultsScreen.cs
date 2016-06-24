using UnityEngine;
using System.Collections.Generic;
using RTS;
 
public class ResultsScreen : MonoBehaviour {
     
    public GUISkin skin;
     
    private Player winner;
    private VictoryCondition metVictoryCondition;
     
    void OnGUI() {
        GUI.skin = skin;
         
        GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
         
        //display 
        float padding = ResourceManager.Padding;
        float itemHeight = ResourceManager.ButtonHeight;
        float buttonWidth = ResourceManager.ButtonWidth;
        float leftPos = padding;
        float topPos = padding;
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
        string message = "Game Over";
        if(winner) message = "Congratulations! You have won by " + metVictoryCondition.GetDescription();
        GUI.Label(new Rect(leftPos, topPos, Screen.width - 2 * padding, itemHeight), message);
        leftPos = Screen.width / 2 - padding / 2 - buttonWidth;
        topPos += itemHeight + padding;
        if(GUI.Button(new Rect(leftPos, topPos, buttonWidth, itemHeight), "New Game")) {
            //makes sure that the loaded level runs at normal speed
            Time.timeScale = 1.0f;
            ResourceManager.MenuOpen = false;
            Application.LoadLevel("Game");
        }
        leftPos += padding + buttonWidth;
        if(GUI.Button(new Rect(leftPos, topPos, buttonWidth, itemHeight), "Main Menu")) {
            Application.LoadLevel("MainMenu");
            Cursor.visible = true;
        }
         
        GUI.EndGroup();
    }
     
    public void SetMetVictoryCondition(VictoryCondition victoryCondition) {
        if(!victoryCondition) return;
        metVictoryCondition = victoryCondition;
        winner = metVictoryCondition.GetWinner();
    }
}