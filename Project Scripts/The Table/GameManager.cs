using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using Unity.Loading;
using FishNet.Demo.AdditiveScenes;

public class GameManager : NetworkBehaviour
{
    public static Dictionary<int, PlayerInteraction> PlayerInteractions = new Dictionary<int, PlayerInteraction>();
    public static Dictionary<int, PlayerMovement> PlayerMovements = new Dictionary<int, PlayerMovement>();
    public static Dictionary<int, PlayerDiceRoll> PlayerDiceRolls = new Dictionary<int, PlayerDiceRoll>();
    public static Dictionary<int, PlayerBulletLoad> PlayerBulletLoads = new Dictionary<int, PlayerBulletLoad>();
    public static Dictionary<int, PlayerShoot> PlayerShoots = new Dictionary<int, PlayerShoot>();
    public static Dictionary<int, PlayerHealth> PlayerHealths = new Dictionary<int, PlayerHealth>();

    public static int[] hostDiceResult = new int[6] {0, 0, 0, 0, 0, 0};
    public static int[] clientDiceResult = new int[6] {0, 0, 0, 0, 0, 0};

    public static int hostLiveBulletNum;
    public static int clientLiveBulletNum;
    public static int hostBlankBulletNum;
    public static int clientBlankBulletNum;

    public GameState _currentState;
    private bool loading = false;
    public bool doneRolling = false;
    public bool doneLoading = false;
    public bool doneShooting = false;
    public static bool gameDone = false;
    public int round = 0;

    public GameObject mainCamTransform;
    public GameObject toplight;
    public GameObject uiObject;
    

    void Start()
    {
        if (!base.IsServerInitialized){
            return;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        ServerChangeState(GameState.Loading);
    }

    void Update()
    {
        if (loading && PlayerInteractions.Count == 2){
            Debug.Log("Game Starts.");
            loading = false;
            ServerChangeState(GameState.DiceRolling);
        }
        if (doneRolling){
            ServerChangeState(GameState.BulletLoading);
            doneRolling = false;
        }
        if (doneLoading){
            ServerChangeState(GameState.Shoot);
            doneLoading = false;
        }
        if (doneShooting){
            ServerChangeState(GameState.BulletLoading);
            doneShooting = false;
        }
        if (gameDone){
            ServerChangeState(GameState.Finish);
            gameDone = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerChangeState(GameState state){
        ObserverChangeState(state);
    }

    [ObserversRpc]
    public void ObserverChangeState(GameState state)
    {
        _currentState = state;
        
        switch(state)
        {
            case GameState.Loading:
                StartLoading();
                break;
            case GameState.DiceRolling:
                toplight.GetComponent<Animator>().Play("lightblink");
                toplight.GetComponent<AudioSource>().Play(0);
                DiceRoll();
                break;
            case GameState.BulletLoading:
                BulletLoad();
                break;
            case GameState.Shoot:
                Shoot();
                break;
            case GameState.Finish:
                Finish();
                break;
            case GameState.Restart:
                StartRestart();
                break;
            default:
                break;
        }
    }

    void StartLoading()
    {
        loading = true;
        Debug.Log("Waiting for players...");
    }

    void DiceRoll()
    {
        GetComponent<GameStateDice>().begin(PlayerDiceRolls, PlayerInteractions, this);
        Debug.Log("Game State: Dice Roll");
    }
    
    void BulletLoad(){
        GetComponent<GameStateBulletLoad>().begin(PlayerBulletLoads, PlayerInteractions,
            this, PlayerDiceRolls[0].diceResult, PlayerDiceRolls[1].diceResult, round);
        Debug.Log("Game State: Bullet Loading");
    }

    void Shoot()
    {
        GetComponent<GameStateShoot>().begin(PlayerShoots, PlayerInteractions, this);
        Debug.Log("Game State: Shoot");
    }

    void Finish()
    {
        ServerPlayFinalAnim();
        Debug.Log("Game State: Finish");
    }

    void StartRestart()
    {
        Debug.Log("Restarting...");
        ServerChangeState(GameState.Loading);
    }

    public enum GameState
    {
        Loading = 0,
        DiceRolling = 1,
        BulletLoading = 2,
        Shoot = 3,
        Finish = 4,
        Restart = 5
    }

    public static void PlayerDead(){
        bool hostDead = PlayerHealths[0].dead;
        PlayerInteractions[0].enabled = false;
        PlayerInteractions[1].enabled = false;
        PlayerMovements[0].enabled = false;
        PlayerMovements[1].enabled = false;
        PlayerBulletLoads[0].enabled = false;
        PlayerBulletLoads[1].enabled = false;
        PlayerShoots[0].enabled = false;
        PlayerShoots[1].enabled = false;
        gameDone = true;
    }

    [ServerRpc(RequireOwnership = false)]
    void ServerPlayFinalAnim(){
        ObserverPlayFinalAnim();
    }
    [ObserversRpc]
    void ObserverPlayFinalAnim(){
        StopAllCoroutines();
        uiObject.SetActive(false);
        GameObject[] objs = PlayerHealths[0].dead ? PlayerHealths[0].objectsToDisable : PlayerHealths[1].objectsToDisable;
        foreach (GameObject obj in objs){
            obj.SetActive(false);
        }
        toplight.GetComponent<Animator>().enabled = false;
        toplight.GetComponent<Light>().intensity = 0;
        RenderSettings.ambientLight = new Color(0, 0, 0);
        Camera.main.GetComponent<EndingAnimation>().startAnim();
        // Exit to main menu when anim is done
    }
}
