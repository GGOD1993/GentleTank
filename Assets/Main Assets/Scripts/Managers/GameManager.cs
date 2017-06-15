using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int numRoundsToWin = 5;                  // Ӯ����Ϸ��ҪӮ�Ļغ���
    public float startDelay = 3f;                   // ��ʼ��ʱʱ��
    public float endDelay = 3f;                     // ������ʱʱ��
    public CameraControl cameraControl;             // ������ƽű�
    public MinimapCameraController minimapCamera;   // �������������С��ͼ
    public Text messageText;                        // UI�ı�����һ�ʤ�ȣ�
    public PointList spawnPointList;                // ̹�˳�����
    public PointList wayPointList;                  // AI��Ѳ�ߵ��б�
    public TankArray tankArray;                     // ̹�˹���������

    private int roundNumber;                        // ��ǰ�غ���
    private WaitForSeconds startWait;               // ��ʼ�غ���ʱ
    private WaitForSeconds endWait;                 // �����غ���ʱ
    private TankManager roundWinner;                // ��ǰ�غϻ�ʤ���
    private TankManager gameWinner;                 // ���ջ�ʤ���

    private void Awake()
    {
        startWait = new WaitForSeconds(startDelay);
        endWait = new WaitForSeconds(endDelay);
    }

    private void Start()
    {
        spawnPointList.EnableAllPoints();
        SpawnAllTanks();
        SetCameraTargets();

        minimapCamera.SetTarget(tankArray[0].Instance);     //����С��ͼ����Ŀ��Ϊ��һ�����

        // ��ʼ��Ϸѭ��������ʤ�ߣ����»غϣ�������Ϸ�ȣ�
        StartCoroutine(GameLoop());
    }

    // ��������̹�ˣ�������Һ�AI
    private void SpawnAllTanks()
    {
        for (int i = 0; i < tankArray.Length; i++)
        {
            //��ȡ��Ч��������㣬��ÿ��̹��λ�ò�һ��
            Point spawnPoint = spawnPointList.GetRandomPoint(false);
            if (spawnPoint == null)
                continue;

            tankArray[i].InitTank(Instantiate(tankArray[i].tankPerfab, spawnPoint.position, Quaternion.Euler(spawnPoint.rotation)) as GameObject, i + 1, 0, spawnPoint, wayPointList);
            tankArray[i].SetupTank();
        }
    }

    // ������������̹��
    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[tankArray.Length];

        for (int i = 0; i < targets.Length; i++)
            targets[i] = tankArray[i].Instance.transform;

        cameraControl.targets = targets;
    }

    // ��Ϸ��Э��
    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());           //�غϿ�ʼ����һ����ʱ

        yield return StartCoroutine(RoundPlaying());            //�غ��У�����һ��̹�˴��ʱһֱ��������ѭ��

        yield return StartCoroutine(RoundEnding());             //�غϽ���

        // �����������Ϸ�����¼��س��������������һ�غ�
        if (gameWinner != null)
            SceneManager.LoadScene(0);
        else
            StartCoroutine(GameLoop());
    }

    // �غϿ�ʼ
    private IEnumerator RoundStarting()
    {
        ResetAllTanks();                                // ��������̹��
        DisableTankControl();                           // �����������ǵĿ���Ȩ

        cameraControl.SetStartPositionAndSize();        // �������

        ++roundNumber;                                  // �غ�������                
        messageText.text = "ROUND " + roundNumber;

        yield return startWait;                         // ��ʱһ��ʱ���ٿ�ʼ
    }

    // �غ���
    private IEnumerator RoundPlaying()
    {
        EnableTankControl();                            // ������ҿ���Ȩ

        messageText.text = string.Empty;                // �����ʾ��Ϣ

        while (!OneTankLeft())                          // ֻʣһ��̹�˲Ž�����Э��
            yield return null;
    }

    // �غϽ���
    private IEnumerator RoundEnding()
    {
        DisableTankControl();                           // ������ҿ���Ȩ

        roundWinner = GetRoundWinner();                 // ��ȡ�غ�ʤ�������

        if (roundWinner != null)                        // ��Ϊ�վ͸�ʤ����Ҽӻ�ʤ����
            roundWinner.Win();

        gameWinner = GetGameWinner();                   // ��ȡ���ջ�ʤ���

        string message = EndMessage();                  // ��ȡ������Ϣ����ʾ֮
        messageText.text = message;

        yield return endWait;
    }

    // �����Ƿ�С�ڵ���һ��̹�˴�0��˵����ͬ���ˣ�
    private bool OneTankLeft()
    {
        int numTanksLeft = 0;

        for (int i = 0; i < tankArray.Length; i++)
            if (tankArray[i].Instance.activeSelf)
                numTanksLeft++;

        return numTanksLeft <= 1;
    }

    // ��ȡ��ʤ����ң�Ϊ�վ���ƽ��
    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < tankArray.Length; i++)
            if (tankArray[i].Instance.activeSelf)
                return tankArray[i];

        return null;
    }

    // ��ȡ����ʤ�������
    private TankManager GetGameWinner()
    {
        for (int i = 0; i < tankArray.Length; i++)
            if (tankArray[i].WinTimes == numRoundsToWin)
                return tankArray[i];

        return null;
    }

    // ��ȡ�غϻ��ܵ���Ϸ������Ϣ
    private string EndMessage()
    {
        string message = "DRAW!";                       // Ĭ��ƽ��

        // ��ӻ�ʤ��ҵĴ���ɫ����������ַ���
        if (roundWinner != null)
            message = roundWinner.PlayerName + " WINS THE ROUND!";

        message += "\n\n";

        // ���������һ�ʤ����
        for (int i = 0; i < tankArray.Length; i++)
            message += tankArray[i].PlayerName + " : " + tankArray[i].WinTimes + " WINS\n";

        // �������ʤ���
        if (gameWinner != null)
            message = gameWinner.PlayerName + " WINS THE GAME!";

        return message;
    }

    // ��������̹��
    private void ResetAllTanks()
    {
        spawnPointList.EnableAllPoints();                     // ��ʼ��������
        for (int i = 0; i < tankArray.Length; i++)
        {
            //��ȡ��Ч��������㣬��ÿ��̹��λ�ò�һ��
            Point spawnPoint = spawnPointList.GetRandomPoint(false, true);
            if (spawnPoint == null)
                continue;
            tankArray[i].Reset(spawnPoint);
        }
    }

    // ����������ҿ���Ȩ
    private void EnableTankControl()
    {
        for (int i = 0; i < tankArray.Length; i++)
            tankArray[i].SetControlEnable(true);
    }

    // ����������ҿ���Ȩ
    private void DisableTankControl()
    {
        for (int i = 0; i < tankArray.Length; i++)
            tankArray[i].SetControlEnable(false);
    }

    // �������е�
    private void OnDrawGizmos()
    {
        spawnPointList.DebugDrawPoint();
        wayPointList.DebugDrawPoint();
    }
}
