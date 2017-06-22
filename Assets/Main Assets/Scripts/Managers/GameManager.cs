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

    public int numRoundsToWin = 5;                  // Ӯ����Ϸ��ҪӮ�Ļغ���
    public float startDelay = 3f;                   // ��ʼ��ʱʱ��
    public float endDelay = 3f;                     // ������ʱʱ��
    public Text messageText;                        // UI�ı�����һ�ʤ�ȣ�
    public CameraControl cameraControl;             // ������ƽű�
    public MinimapManager minimapManager;           // С��ͼ������
    public SpawnAllPopUpArrow spawnAllPopUpArrow;   // ������ʾ��Ҽ�ͷ

    private WaitForSeconds startWait;               // ��ʼ�غ���ʱ
    private WaitForSeconds endWait;                 // �����غ���ʱ

    /// <summary>
    /// ��ʼ����Ϸ��¼ʵ������������̹�ˡ��������Ŀ�ꡢС��ͼ��ʼ������ʼ��Ϸѭ��
    /// </summary>
    private void Start()
    {
        startWait = new WaitForSeconds(startDelay);
        endWait = new WaitForSeconds(endDelay);
        GameRecord.instance = new GameRecord(numRoundsToWin, allTanksManager, allTeamsManager);

        SetupGame();                                // ������Ϸ

        // ��ʼ��Ϸѭ��������ʤ�ߣ����»غϣ�������Ϸ�ȣ�
        GameRecord.instance.StartGame();
        StartCoroutine(GameLoop());
    }

    /// <summary>
    /// ��������̹�ˣ�������Һ�AI�������þ�ͷ����׷��Ŀ�ꡢС��ͼ��ʼ��
    /// </summary>
    private void SetupGame()
    {
        GameObject tanks = new GameObject("Tanks");
        for (int i = 0; i < allTanksManager.Count; i++)
            allTanksManager[i].InitTank(Instantiate(allTanksManager[i].tankPerfab, tanks.transform), allTeamsManager);

        cameraControl.targets = allTanksManager.GetTanksTransform();
        cameraControl.SetStartPositionAndSize();

        minimapManager.SetupPlayerIconDic(allTanksManager, allTeamsManager);
        minimapManager.SetTarget(allTanksManager[0].Instance.transform);
    }

    /// <summary>
    /// ��������̹�˳�����
    /// </summary>
    private void ResetAllTanksSpawnPoint()
    {
        spawnPointList.EnableAllPoints();                     // ��ʼ��������
        for (int i = 0; i < allTanksManager.Count; i++)
        {
            //��ȡ��Ч��������㣬��ÿ��̹��λ�ò�һ��
            Point spawnPoint = spawnPointList.GetRandomPoint(false, true);
            if (spawnPoint == null)
                continue;
            allTanksManager[i].Reset(spawnPoint);
        }
    }

    /// <summary>
    /// ����������ҿ���Ȩ
    /// </summary>
    /// <param name="enable">����״̬</param>
    private void SetTanksControlEnable(bool enable)
    {
        for (int i = 0; i < allTanksManager.Count; i++)
            allTanksManager[i].SetControlEnable(enable);
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
        if (GameRecord.instance.IsEndOfTheGame())
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
        spawnAllPopUpArrow.Spawn(allTanksManager);      // ��ʾ��Ҽ�ͷ
        GameRecord.instance.StartRound();

        messageText.text = "ROUND " + GameRecord.instance.CurrentRound;

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

        while (!GameRecord.instance.IsEndOfTheRound())           // �غ�û�����ͼ���
            yield return null;
    }

    /// <summary>
    /// �غϽ���
    /// </summary>
    /// <returns></returns>
    private IEnumerator RoundEnding()
    {
        SetTanksControlEnable(false);                               // ������ҿ���Ȩ

        GameRecord.instance.UpdateWonData();                        // ���»�ʤ����

        messageText.text = GameRecord.instance.GetEndingMessage();  // ��ȡ������Ϣ����ʾ֮

        yield return endWait;
    }


}
