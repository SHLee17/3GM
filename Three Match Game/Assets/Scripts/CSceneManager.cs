using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CSceneManager : CSingleton<CSceneManager>
{
    [System.Serializable]
    public class CStageScene
    {
        public bool isClear;
        public string stageDescription;
    }
    public List<CStageScene> stageSceneList;

    public Text txtStageInfo;
    public GameObject objStartButton;

    int currentStageSelect;
    public void StageButton(int i)
    {
        //SceneManager.LoadScene(i);
        txtStageInfo.text = stageSceneList[i - 1].stageDescription;
        objStartButton.SetActive(true);
        currentStageSelect = i;
    }

    public void StartButton()
    {
        SceneManager.LoadScene(currentStageSelect);
    }
}
