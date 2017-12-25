using Item.Tank;
using Widget;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Widget.Minimap;
using System.Collections.Generic;
using CrossPlatformInput;

public class GameManager : MonoBehaviour
{
    public GameManager Instance { get; private set; }

    public Points spawnPoints;
    public Points wayPoints;
    public AllPlayerManager allPlayerManager;       // �������
    public int numRoundsToWin = 5;                  // Ӯ����Ϸ��ҪӮ�Ļغ���
    public float startDelay = 3f;                   // ��ʼ��ʱʱ��
    public float endDelay = 3f;                     // ������ʱʱ��
    public float changeCamDelay = 2f;               // ת����ͷ��ʱʱ��
    public Text messageText;                        // UI�ı�����һ�ʤ�ȣ�
    public AllCameraRigManager allCameraRig;        // ���о�ͷ������
    public MinimapWitchCamera minimap;              // С��ͼ����
    public AllArrowPopUpManager spawnAllPopUpArrow; // ������ʾ��Ҽ�ͷ

    public TankManager MyTank { get { return myTank; } }

    private List<TankManager> tankList;             // �������̹��
    private List<int> spawnIndexList = new List<int>();
    private TankManager myTank;                     // �Լ���̹��
    private WaitForSeconds startWait;               // ��ʼ�غ���ʱ
    private WaitForSeconds changeCamWait;           // ת����ͷ��ʱ
    private WaitForSeconds endWait;                 // �����غ���ʱ

    private void Awake()
    {
        Instance = this;
        tankList = new List<TankManager>();
        startWait = new WaitForSeconds(startDelay);         // ��Ϸ�غϿ�ʼ��ʱ
        endWait = new WaitForSeconds(endDelay);             // ��Ϸ�غϽ�����ʱ
        changeCamWait = new WaitForSeconds(changeCamDelay); // ��ͷת����ʱ
    }

    /// <summary>
    /// ��ʼ����Ϸ��¼ʵ������������̹�ˡ��������Ŀ�ꡢС��ͼ��ʼ������ʼ��Ϸѭ��
    /// </summary>
    private void Start()
    {
        SetupGame();                                // ������Ϸ
        //CreateMasterTank();

        GameRound.Instance.maxRound = numRoundsToWin;             // ���þ���
        GameRound.Instance.StartGame();            // ��ʼ��Ϸѭ��������ʤ�ߣ����»غϣ�������Ϸ�ȣ�
        StartCoroutine(GameLoop());
    }

    /// <summary>
    /// ��������̹�ˣ�������Һ�AI�������þ�ͷ����׷��Ŀ�ꡢС��ͼ��ʼ��
    /// </summary>
    private void SetupGame()
    {
        myTank = CreateMasterTank();
        allPlayerManager.SetupInstance();
        AllPlayerManager.Instance.CreatePlayerGameObjects(new GameObject("Tanks").transform, myTank);
        tankList.Add(myTank);
        myTank.Init(wayPoints);
        for (int i = 1; i < AllPlayerManager.Instance.Count; i++)
        {
            tankList.Add(AllPlayerManager.Instance[i].GetComponent<TankManager>());
            tankList[i].Init(wayPoints);
            //if (AllPlayerManager.Instance[i] == AllPlayerManager.Instance.MyPlayer)
            //    myTank = tankList[i];
        }

        allCameraRig.Init(myTank?myTank.transform : null, AllPlayerManager.Instance.GetAllPlayerTransform());

        if (myTank != null)
        {
            minimap.SetTarget(myTank.transform);
            minimap.SetMinimapActive(true);
            if (VirtualInput.GetButton("Attack") != null)
                ((ChargeButtonInput)VirtualInput.GetButton("Attack")).Setup(myTank.tankAttack,myTank.tankAttack.coolDownTime, myTank.tankAttack.minLaunchForce, myTank.tankAttack.maxLaunchForce, myTank.tankAttack.ChargeRate);
        }
    }

    /// <summary>
    /// �������̹��
    /// </summary>
    private TankManager CreateMasterTank()
    {
        GameObject tank = Instantiate(MasterManager.Instance.StandardPrefab);
        MasterManager.Instance.SelectedTank.CreateTank(tank.transform);

        TankManager manager = tank.GetComponent<TankManager>();
        MasterManager.Instance.SelectedTank.InitTankComponents(manager);

        MasterData data = MasterManager.Instance.data;
        manager.Information = new PlayerInformation(0, data.masterName,data.isAI, data.representColor, data.team);
        manager.stateController.defaultStats = data.aiState;

        return manager;
    }

    /// <summary>
    /// ��������̹�˳�����
    /// </summary>
    private void ResetAllTanksSpawnPoint()
    {
        spawnIndexList.Clear();
        for (int i = 0; i < spawnPoints.Count; i++)
            spawnIndexList.Add(i);

        int randomI = 0;
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            //randomI = spawnIndexList[Random.Range(0, spawnIndexList.Count - 1)];
            //tankList[i].ResetToSpawnPoint(spawnPoints[randomI]);
            tankList[i].ResetToSpawnPoint(spawnPoints[(i + GameRound.Instance.CurrentRound * spawnPoints.Count / 2) % spawnPoints.Count]);
            spawnIndexList.Remove(randomI);
        }
    }

    /// <summary>
    /// ����������ҿ���Ȩ
    /// </summary>
    /// <param name="enable">����״̬</param>
    private void SetTanksControlEnable(bool enable)
    {
        for (int i = 0; i < tankList.Count; i++)
            tankList[i].SetControlEnable(enable);
    }

    /// <summary>
    /// �Լ���̹�˻��ˣ�ת����ͷ
    /// </summary>
    private void OnMyTankBroken()
    {
        if (myTank == null || myTank.isActiveAndEnabled)
            return;
        minimap.SetMinimapActive(false);
        allCameraRig.ChangeCameraRig(AllCameraRigManager.CameraRigType.MultiTarget);
        //allCameraRig.ChangeCameraRig(AllCameraRigManager.CameraRigType.CMMultiTarget);
        //allCameraRig.ChangeCMFollowToCMMultiTrigger();
    }

    /// <summary>
    /// ��Ϸ��ѭ��Э��
    /// </summary>
    /// <returns></returns>
    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());           //�غϿ�ʼ����һ����ʱ
        GameRound.Instance.OnGameRoundStartEvent.Invoke();
        yield return StartCoroutine(RoundPlaying());            //�غ���
        GameRound.Instance.OnGameRoundEndEvent.Invoke();
        yield return StartCoroutine(RoundEnding());             //�غϽ���

        // �����������Ϸ�����¼��س��������������һ�غ�
        if (GameRound.Instance.IsEndOfTheGame())
            AllSceneManager.LoadScene(AllSceneManager.GameSceneType.SoloScene);
        else
            StartCoroutine(GameLoop());
    }

    /// <summary>
    /// �غϿ�ʼ
    /// </summary>
    /// <returns></returns>
    private IEnumerator RoundStarting()
    {
        SetTanksControlEnable(false);                   // ����̹���ǵĿ���Ȩ
        ResetAllTanksSpawnPoint();                      // ��������̹��λ��
        spawnAllPopUpArrow.Spawn();                     // ��ʾ��Ҽ�ͷ
        minimap.SetMinimapActive(true);
        GameRound.Instance.StartRound();

        messageText.text = "ROUND " + GameRound.Instance.CurrentRound;

        yield return changeCamWait;                     // ��ʱһ��ʱ��ת���ɵ�����ͷ
        if (myTank != null &&!myTank.IsAI)
            //allCameraRig.ChangeCameraRig(AllCameraRigManager.CameraRigType.CMFollow);
            allCameraRig.ChangeCameraRig(AllCameraRigManager.CameraRigType.AutoFollow);
        yield return startWait;                         // ��ʱһ��ʱ���ٿ�ʼ
    }

    /// <summary>
    /// �غ���
    /// </summary>
    /// <returns></returns>
    private IEnumerator RoundPlaying()
    {
        SetTanksControlEnable(true);                    // ������ҿ���Ȩ

        messageText.text = string.Empty;                // �����ʾ��Ϣ

        while (!GameRound.Instance.IsEndOfTheRound())   // �غ�û�����ͼ���
        {
            OnMyTankBroken();
            yield return null;
        }
    }

    /// <summary>
    /// �غϽ���
    /// </summary>
    /// <returns></returns>
    private IEnumerator RoundEnding()
    {
        allCameraRig.ChangeCameraRig(AllCameraRigManager.CameraRigType.MultiTarget);
        //allCameraRig.ChangeCameraRig(AllCameraRigManager.CameraRigType.CMMultiTarget);
        //allCameraRig.ChangeCMFollowToCMMultiTrigger();

        SetTanksControlEnable(false);                   // ������ҿ���Ȩ

        GameRound.Instance.UpdateWonData();             // ���»�ʤ����

        messageText.text = GameRound.Instance.GetEndingMessage();  // ��ȡ������Ϣ����ʾ֮

        yield return endWait;
    }

    /// <summary>
    /// �ص����˵�
    /// </summary>
    public void BackToMainScene()
    {
        AllSceneManager.LoadScene(AllSceneManager.GameSceneType.MainMenuScene);
    }
}