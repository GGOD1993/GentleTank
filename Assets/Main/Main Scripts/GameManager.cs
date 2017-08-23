using Item.Tank;
using Widget.ArrowPopUp;
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

    public PointList spawnPointList;                // ̹�˳�����
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

        new GameRound(numRoundsToWin);             // ����һ����Ϸ��¼ʵ��
        GameRound.Instance.StartGame();            // ��ʼ��Ϸѭ��������ʤ�ߣ����»غϣ�������Ϸ�ȣ�
        StartCoroutine(GameLoop());
    }

    /// <summary>
    /// ��������̹�ˣ�������Һ�AI�������þ�ͷ����׷��Ŀ�ꡢС��ͼ��ʼ��
    /// </summary>
    private void SetupGame()
    {
        allPlayerManager.SetupInstance();
        AllPlayerManager.Instance.CreatePlayerGameObjects(new GameObject("Tanks").transform);
        for (int i = 0; i < AllPlayerManager.Instance.Count; i++)
        {
            tankList.Add(AllPlayerManager.Instance[i].GetComponent<TankManager>());
            tankList[i].Init();
            if (AllPlayerManager.Instance[i] == AllPlayerManager.Instance.MyPlayer)
                myTank = tankList[i];
        }

        allCameraRig.Init(myTank.transform, AllPlayerManager.Instance.GetAllPlayerTransform());

        if (myTank != null)
        {
            minimap.SetTarget(myTank.transform);
            minimap.SetMinimapActive(true);
            ((ChargeButtonInput)VirtualInput.GetButton("TankShooting")).Setup(myTank.tankShooting.coolDownTime, myTank.tankShooting.minLaunchForce, myTank.tankShooting.maxLaunchForce, myTank.tankShooting.ChargeRate);
        }
    }

    /// <summary>
    /// ��������̹�˳�����
    /// </summary>
    private void ResetAllTanksSpawnPoint()
    {
        spawnPointList.EnableAllPoints();                     // ��ʼ��������
        for (int i = 0; i < tankList.Count; i++)
        {
            //��ȡ��Ч��������㣬��ÿ��̹��λ�ò�һ��
            Point spawnPoint = spawnPointList.GetRandomPoint(false, true);
            if (spawnPoint == null)
                continue;
            tankList[i].ResetSpawnPoint(spawnPoint);
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
        allCameraRig.TurnToMultiCam();
    }

    /// <summary>
    /// ��Ϸ��ѭ��Э��
    /// </summary>
    /// <returns></returns>
    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());           //�غϿ�ʼ����һ����ʱ

        yield return StartCoroutine(RoundPlaying());            //�غ���

        yield return StartCoroutine(RoundEnding());             //�غϽ���

        // �����������Ϸ�����¼��س��������������һ�غ�
        if (GameRound.Instance.IsEndOfTheGame())
            SceneManager.LoadScene(0);
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
        allCameraRig.TurnToAutoCam();
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
        allCameraRig.TurnToMultiCam();

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
        AllSceneManager.LoadScene(GameScene.MainMenuScene);
    }
}