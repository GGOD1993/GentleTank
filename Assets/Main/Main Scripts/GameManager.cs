using CameraRig;
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
    public PointList spawnPointList;                // ̹�˳�����

    public AllPlayerManager allPlayerManager;       // �������
    public int numRoundsToWin = 5;                  // Ӯ����Ϸ��ҪӮ�Ļغ���
    public float startDelay = 3f;                   // ��ʼ��ʱʱ��
    public float endDelay = 3f;                     // ������ʱʱ��
    public Text messageText;                        // UI�ı�����һ�ʤ�ȣ�
    public MultiplayerCameraManager cameraControl;  // ����������
    public MinimapManager minimapManager;           // С��ͼ������
    public AllArrowPopUpManager spawnAllPopUpArrow; // ������ʾ��Ҽ�ͷ

    private List<TankManager> tankList;             // �������̹��
    private TankManager myTank;                     // �Լ���̹��
    private WaitForSeconds startWait;               // ��ʼ�غ���ʱ
    private WaitForSeconds endWait;                 // �����غ���ʱ

    private void Awake()
    {
        tankList = new List<TankManager>();
        startWait = new WaitForSeconds(startDelay); // ��Ϸ�غϿ�ʼ��ʱ
        endWait = new WaitForSeconds(endDelay);     // ��Ϸ�غϽ�����ʱ
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
            if (AllPlayerManager.Instance[i].IsMine && myTank == null)
                myTank = tankList[i];
        }

        cameraControl.targets = AllPlayerManager.Instance.GetAllPlayerTransform();
        cameraControl.SetStartPositionAndSize();

        minimapManager.SetupPlayerIconDic();
        if (myTank != null)
        {
            minimapManager.SetTarget(myTank.transform);
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
        spawnAllPopUpArrow.Spawn();      // ��ʾ��Ҽ�ͷ
        GameRound.Instance.StartRound();

        messageText.text = "ROUND " + GameRound.Instance.CurrentRound;

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

        while (!GameRound.Instance.IsEndOfTheRound())           // �غ�û�����ͼ���
            yield return null;
    }

    /// <summary>
    /// �غϽ���
    /// </summary>
    /// <returns></returns>
    private IEnumerator RoundEnding()
    {
        SetTanksControlEnable(false);                               // ������ҿ���Ȩ

        GameRound.Instance.UpdateWonData();                        // ���»�ʤ����

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