using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceGroupManager : MonoBehaviour
{
    public DiceState[] diceList = new DiceState[5];

    public int[] reportDiceResult(){
        int[] result = new int[5] {0, 0, 0, 0, 0};
        for(int i = 0; i < diceList.Length; i++){
            result[i] = diceList[i].reportFaceUp();
        }
        return result;
    }
}
