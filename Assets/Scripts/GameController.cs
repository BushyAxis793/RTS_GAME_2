using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    public static List<Soldier> SoldierList { get { return gameController.soldierList; } }
    public static List<Dragon> DragonList { get { return gameController.dragonList; } }
    static GameController gameController;


    [SerializeField]
    GameObject middlePanel,upperPanel, bottomPanel;
    [SerializeField]
    CameraControl cameraControl;

    List<Soldier> soldierList = new List<Soldier>();
    List<Dragon> dragonList = new List<Dragon>();

    Text text;


    private void Awake()
    {
        text = middlePanel.GetComponentInChildren<Text>(true);
        gameController = this;
    }
    void Update()
    {
        TidyList(soldierList);
        TidyList(dragonList);

        if (soldierList.Count <= 0) Lose();
        else if (dragonList.Count <= 0) Win();
    }

    void TidyList<T>(List<T> list) where T: Unit
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null || list[i].isAlive)
                list.RemoveAt(i--);
        }
    }

    void Lose()
    {
        EndGame();
        text.text = "You lose...";
    }

    void Win()
    {
        EndGame();
        text.text = "Good Job, You win!";
    }

    void EndGame()
    {
        //cameraControl.enabled = false;
        middlePanel.SetActive(true);
        bottomPanel.SetActive(false);
        upperPanel.SetActive(false);

    }
}
