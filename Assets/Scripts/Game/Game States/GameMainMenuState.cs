using System;
using UnityEngine.SceneManagement;

public class GameMainMenuState : GameState
{
    public GameMainMenuState(GameStatemachine machine) : base(machine)
    {
        this.machine = machine;
    }
    public override async void EnterState()
    {
        GameManager.Instance.Area = "MainMenu";
        if (SceneManager.GetActiveScene().name != "MainMenu") await LevelManager.LoadScene("MainMenu", false);
        //AudioManager.Instance.Play("");
    }
    public override void ExitState()
    {
        GameManager.Instance.Area = null;
    }
    public override void Update()
    {
        base.Update();
    }
}
