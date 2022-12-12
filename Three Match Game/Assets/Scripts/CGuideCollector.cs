using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CGuideCollector : MonoBehaviour
{
    public GameObject objTreeMatch;
    public GameObject objFourMatch;
    public Dictionary<int, Vector3> directionDict;

    private void Start()
    {
        directionDict = new Dictionary<int, Vector3>();

        directionDict.Add(3, new Vector3(0, 0, -60));
        directionDict.Add(4, new Vector3(0, 0, 0));
        directionDict.Add(5, new Vector3(0, 0, 60));
    }

    public void Deactiivation()
    {
        if (objTreeMatch.activeSelf)
            objTreeMatch.SetActive(false);
        else if (objFourMatch.activeSelf)
            objFourMatch.SetActive(false);
    }

    public void ActiveGuide(int matchNumber,Vector2 pos ,int direction)
    {
        //gameObject.SetActive(true);
        switch (matchNumber)
        {
            case 3:
                objTreeMatch.SetActive(true);
                objTreeMatch.transform.position = pos;
                switch (direction)
                {
                    case 3:
                        objTreeMatch.transform.rotation = Quaternion.Euler(directionDict[3]);
                        break;
                    case 4:
                        objTreeMatch.transform.rotation = Quaternion.Euler(directionDict[4]);
                        break;
                    case 5:
                        objTreeMatch.transform.rotation = Quaternion.Euler(directionDict[5]);
                        break;
                }
                break;

            case 4:
                objFourMatch.SetActive(true);
                objFourMatch.transform.position = pos;
                switch (direction)
                {
                    case 3:
                        objFourMatch.transform.rotation = Quaternion.Euler(directionDict[3]);
                        break;
                    case 4:
                        objFourMatch.transform.rotation = Quaternion.Euler(directionDict[4]);
                        break;
                    case 5:
                        objFourMatch.transform.rotation = Quaternion.Euler(directionDict[5]);
                        break;
                }
                break;
        }

        
    }
}
