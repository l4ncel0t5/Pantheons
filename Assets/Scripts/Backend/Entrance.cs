﻿
using UnityEngine;
using UnityEngine.SceneManagement;


public class Entrance : MonoBehaviour
{
    private LevelManager levelManager;
    [SerializeField]
    public Scene nextArea;
    private GameObject entrance;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && PlayerManager.instance.canChangeScenes)
        {
            levelManager.LoadScene(nextArea);

            foreach (GameObject obj in nextArea.GetRootGameObjects())
            {
                if (obj.GetComponent<Entrance>() != null && obj.name == $"{nextArea.name}")
                {
                    entrance = obj;
                }
            }
            Transform playerTrans = PlayerManager.instance.transform;
            Transform entrySpawnpoint = entrance.transform.GetChild(0).transform;
            Transform finalStandpoint = entrance.transform.GetChild(1).transform;

            playerTrans.position = entrySpawnpoint.transform.position;
            Mathf.MoveTowards(playerTrans.position.x, finalStandpoint.position.x, PlayerManager.instance.MaxSpeed);
            if (playerTrans.position.x == entrySpawnpoint.position.x +- .3f) PlayerManager.instance.movementDisable = false;
        }
    }


}

