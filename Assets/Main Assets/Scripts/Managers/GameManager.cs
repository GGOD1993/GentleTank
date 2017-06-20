using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public PointList spawnPointList;                // ̹�˳�����
    public AllTanksManager allTanksManager;         // ����̹�˹�����
    public AllTeamsManager allTeamsManager;         // �����Ŷӹ�����
    public ObjectPool arrowPopUpPool;               // ��ʾ��Ҽ�ͷ��   

    public int numRoundsToWin = 5;                  // Ӯ����Ϸ��ҪӮ�Ļغ���
    public float startDelay = 3f;                   // ��ʼ��ʱʱ��
    public float endDelay = 3f;                     // ������ʱʱ��
    public Text messageText;                        // UI�ı�����һ�ʤ�ȣ�
    public CameraControl cameraControl;             // ������ƽű�
    public MinimapManager minimapManager;           // С��ͼ������
    public GameObject groudCanvas;                  // ������ʾ��Ҽ�ͷ

    private WaitForSeconds startWait;               // ��ʼ�غ���ʱ
    private WaitForSeconds endWait;                 // �����غ���ʱ

    private void Start()
    {
        startWait = new WaitForSeconds(startDelay);
        endWait = new WaitForSeconds(endDelay);
        GameRecord.instance = new GameRecord(numRoundsToWin, allTanksManager, allTeamsManager);
        spawnPointList.EnableAllPoints();

        SpawnAllTanks();
        SetupCameraAndMinimap();

        // ��ʼ��Ϸѭ��������ʤ�ߣ����»غϣ�������Ϸ�ȣ�
        GameRecord.instance.StartGame();
        StartCoroutine(GameLoop());
    }

    // ��������̹�ˣ�������Һ�AI
    private void SpawnAllTanks()
    {
        GameObject tanks = new GameObject("Tanks");
        for (int i = 0; i < allTanksManager.Length; i++)
        {
            //��ȡ��Ч��������㣬��ÿ��̹��λ�ò�һ��
            Point spawnPoint = spawnPointList.GetRandomPoint(false);
            if (spawnPoint == null)
                continue;

            allTanksManager[i].InitTank(Instantiate(allTanksManager[i].tankPerfab, spawnPoint.position, Quaternion.Euler(spawnPoint.rotation), tanks.transform), allTeamsManager);
            allTanksManager[i].SetupTank();
        }
    }

    // ��������������̹�ˣ�С��ͼ������׷��Ŀ��
    private void SetupCameraAndMinimap()
    {
        cameraControl.targets = allTanksManager.GetTanksTransform();
        minimapManager.SetupPlayerIconDic(allTanksManager, allTeamsManager);
        minimapManager.SetTarget(allTanksManager[0].Instance.transform);
    }

    // ��Ϸ��ѭ��Э��
    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());           //�غϿ�ʼ����һ����ʱ

        yield return StartCoroutine(RoundPlaying());            //�غ��У�����һ��̹�˴��ʱһֱ��������ѭ��

        yield return StartCoroutine(RoundEnding());             //�غϽ���

        // �����������Ϸ�����¼��س��������������һ�غ�
        if (GameRecord.instance.IsEndOfTheGame())
            SceneManager.LoadScene(0);
        else
            StartCoroutine(GameLoop());
    }

    // �غϿ�ʼ
    private IEnumerator RoundStarting()
    {
        ResetAllTanks();                                // ��������̹��
        SetTanksControlEnable(false);                   // �����������ǵĿ���Ȩ
        ShowTankArrow(groudCanvas);           // ��ʾ��Ҽ�ͷ

        cameraControl.SetStartPositionAndSize();        // �������

        GameRecord.instance.StartRound();
        messageText.text = "ROUND " + GameRecord.instance.CurrentRound;

        yield return startWait;                         // ��ʱһ��ʱ���ٿ�ʼ
    }

    // �غ���
    private IEnumerator RoundPlaying()
    {
        SetTanksControlEnable(true);                    // ������ҿ���Ȩ

        messageText.text = string.Empty;                // �����ʾ��Ϣ

        while (!GameRecord.instance.IsEndOfTheRound())           // �غ�û�����ͼ���
            yield return null;
    }

    // �غϽ���
    private IEnumerator RoundEnding()
    {
        SetTanksControlEnable(false);                   // ������ҿ���Ȩ

        GameRecord.instance.UpdateWonData();                     // ���»�ʤ����

        messageText.text = EndMessage();                // ��ȡ������Ϣ����ʾ֮

        yield return endWait;
    }

    // ��ȡ�غϻ��ܵ���Ϸ������Ϣ
    private string EndMessage()
    {
        string message = "DRAW!";                       // Ĭ��ƽ��

        if (!GameRecord.instance.IsDraw())                       // ����ƽ�֣���ȡʤ����
            message = GameRecord.instance.GetWinnerName() + " WINS THE ROUND!";

        message += "\n\n";

        foreach (var item in GameRecord.instance.playerWonTimes) // ��ȡ�������ʤ����Ϣ
            message += allTanksManager.GetTankByID(item.Key).ColoredPlayerName + " : " + item.Value + "WINS\n";

        if (GameRecord.instance.IsEndOfTheGame())                // �������������������Ӯ�������
            message = GameRecord.instance.GetWinnerName() + " WINS THE GAME!";
        return message;
    }

    // ��������̹��
    private void ResetAllTanks()
    {
        spawnPointList.EnableAllPoints();                     // ��ʼ��������
        for (int i = 0; i < allTanksManager.Length; i++)
        {
            //��ȡ��Ч��������㣬��ÿ��̹��λ�ò�һ��
            Point spawnPoint = spawnPointList.GetRandomPoint(false, true);
            if (spawnPoint == null)
                continue;
            allTanksManager[i].Reset(spawnPoint);
        }
    }

    // ����������ҿ���Ȩ
    private void SetTanksControlEnable(bool enable)
    {
        for (int i = 0; i < allTanksManager.Length; i++)
            allTanksManager[i].SetControlEnable(enable);
    }

    // ��ʾ��ҿ��Ƶ�̹�˼�ͷ
    private void ShowTankArrow(GameObject parent)
    {
        arrowPopUpPool.GetPoolParent().transform.parent = parent.transform;
        for (int i = 0; i < allTanksManager.Length; i++)
        {
            if (allTanksManager[i].isAI)
                continue;
            GameObject arrowPopUp = arrowPopUpPool.GetNextObject();
            arrowPopUp.SetActive(true);
            arrowPopUp.GetComponent<ArrowPopUp>().Setposition(allTanksManager[i].Instance.transform.position);
            arrowPopUp.GetComponent<ArrowPopUp>().SetColor(allTanksManager[i].playerColor);
            arrowPopUp.GetComponent<ArrowPopUp>().SetText("P" + i);
        }
    }
}
