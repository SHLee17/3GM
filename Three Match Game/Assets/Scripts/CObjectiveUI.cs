using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CObjectiveUI : CSingleton<CObjectiveUI>
{
    [System.Serializable]
    public class CObjective
    {
        public GameObject obj;
        public Image imageObjective;
        public Text txtCount;
        public int count;
        public EObstructionType eObstructionType;
        public ETileColor eTileColor;
    }
    public List<CObjective> cObjectiveList;
    public int targetCount;
    public bool isClear;

    private void Start()
    {
        isClear = false;
        InitStageOption();
    }

    private void Update()
    {
        for (int i = 0; i < targetCount; i++)
            cObjectiveList[i].txtCount.text = "" + cObjectiveList[i].count;

        switch (targetCount)
        {
            case 1:
                if (cObjectiveList[0].count == 0)
                    isClear = true;
                break;
            case 2:
                if (cObjectiveList[0].count == 0 && cObjectiveList[1].count == 0)
                    isClear = true;
                break;
        }
    }

    public void InitStageOption()
    {
        for (int i = 0; i < targetCount; i++)
        {
            cObjectiveList[i].txtCount.text = "" + cObjectiveList[i].count;

            switch (cObjectiveList[i].eObstructionType)
            {
                case EObstructionType.ICE_BOX:
                    cObjectiveList[i].imageObjective.sprite = CObjectPool.Instance.SOspriteManger.sprIceBoxList[0];
                    break;
                case EObstructionType.TILE:
                    cObjectiveList[i].imageObjective.sprite = CObjectPool.Instance.SOspriteManger.sprNormalTileList[(int)cObjectiveList[i].eTileColor];
                    break;
            }

        }

        if (targetCount == 1)
        {
            cObjectiveList[1].obj.SetActive(false);

            cObjectiveList[0].obj.transform.localPosition = new Vector2(0, 0);
        }
    }
}
