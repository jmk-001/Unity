using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameManager manager;
    public Texture[] diceImg;
    public int[] hostDiceInfo;
    public RawImage[] hostDiceInfoScreen;
    public bool hostDiceInfoChanged = false;
    public RawImage[] clientDiceInfoScreen;
    public int[] clientDiceInfo;
    public bool clientDiceInfoChanged = false;
    public TextMeshProUGUI roundInfo;

    void Update()
    {
        if (manager != null) {
            refreshRound();
            refreshHostDiceInfo();
            refreshClientDiceInfo();
        }
    }

    void refreshRound(){
        roundInfo.text = "Round: " + (manager.round+1);
    }

    void refreshHostDiceInfo(){
        if (!arrayEqual(hostDiceInfo, GameManager.hostDiceResult)){
            hostDiceInfo = GameManager.hostDiceResult;
            for (int i=0; i<hostDiceInfo.Length; i++){
                hostDiceInfoScreen[i].texture = diceImg[hostDiceInfo[i]-1];
                Color tmp = hostDiceInfoScreen[i].color;
                tmp.a = 255f;
                hostDiceInfoScreen[i].color = tmp;
            }
        }
    }

    void refreshClientDiceInfo(){
        if (!arrayEqual(clientDiceInfo, GameManager.clientDiceResult)){
            clientDiceInfo = GameManager.clientDiceResult;
            for (int i=0; i<clientDiceInfo.Length; i++){
                clientDiceInfoScreen[i].texture = diceImg[clientDiceInfo[i]-1];
                Color tmp = clientDiceInfoScreen[i].color;
                tmp.a = 255f;
                clientDiceInfoScreen[i].color = tmp;
            }
        }
    }

    bool arrayEqual(int[] arr1, int[] arr2){
        for (int i=0; i<arr1.Length; i++){
            if (arr1[i] != arr2[i]){
                return false;
            }
        }
        return true;
    }
}
