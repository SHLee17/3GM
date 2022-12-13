using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EObstructionType
{
    ICE_BOX,
    TILE,
    NONE
}
public class CObstruction : MonoBehaviour
{
    public int count;
    public EObstructionType eObstructionType;
    public SpriteRenderer sprRenderer;
    public bool isMatchPossible;
    public bool isMovePossible;
    public bool hasDamaged;
    void Start()
    {
        Init(eObstructionType);
        hasDamaged = false;
    }

    public void Init(EObstructionType _eObstructionType)
    {
        switch (_eObstructionType)
        {
            case EObstructionType.ICE_BOX:
                sprRenderer.sprite = CObjectPool.Instance.SOspriteManger.sprIceBoxList[0];
                gameObject.SetActive(true);
                count = 2;
                isMatchPossible = false;
                isMovePossible = false;
                break;
            case EObstructionType.NONE:
                gameObject.SetActive(false);
                break;
        }
    }

    private void OnDisable()
    {
        eObstructionType = EObstructionType.NONE;
        Init(eObstructionType);
    }


    public void Damage()
    {
        switch (eObstructionType)
        {
            
            case EObstructionType.ICE_BOX:
                if (count == 2)
                {
                    count -= 1;
                    sprRenderer.sprite = CObjectPool.Instance.SOspriteManger.sprIceBoxList[1];
                    hasDamaged = true;
                }
                else
                {
                    for (int i = 0; i < CObjectiveUI.Instance.targetCount; i++)
                    {
                        if (CObjectiveUI.Instance.cObjectiveList[i].eObstructionType == eObstructionType) 
                        {
                            if (CObjectiveUI.Instance.cObjectiveList[i].count > 0)
                                CObjectiveUI.Instance.cObjectiveList[i].count -= 1;
                        }
                    }
                    eObstructionType = EObstructionType.NONE;
                    Init(eObstructionType);
                    
                }
                break;
            case EObstructionType.NONE:
                break;
        }
    }

    public void ResetHasDamaged()
    {
        if (gameObject.activeSelf)
            hasDamaged = false;
    }
}
