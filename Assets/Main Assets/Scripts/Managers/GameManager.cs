using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Complete
{
    public class GameManager : MonoBehaviour
    {
        public int numRoundsToWin = 5;              // Ӯ����Ϸ��ҪӮ�Ļغ���
        public float startDelay = 3f;               // ��ʼ��ʱʱ��
        public float endDelay = 3f;                 // ������ʱʱ��
        public CameraControl cameraControl;         // ������ƽű�
        public CameraFollowTarget followCamera;     // �������������С��ͼ
        public Text messageText;                    // UI�ı�����һ�ʤ�ȣ�
        public TankManager[] tankManagerArray;      // ̹�˹�����
        public List<Transform> wayPointsList;       // AI��Ѳ�ߵ��б�
        public List<Transform> spawnPointsList;     // ̹�˳�����

        private List<bool> spawnPointsValid;        // ��Ӧ̹�˳������Ƿ���Ч
        private int roundNumber;                    // ��ǰ�غ���
        private WaitForSeconds startWait;           // ��ʼ�غ���ʱ
        private WaitForSeconds endWait;             // �����غ���ʱ
        private TankManager roundWinner;            // ��ǰ�غϻ�ʤ���
        private TankManager gameWinner;             // ���ջ�ʤ���

        private void Awake()
        {
            startWait = new WaitForSeconds(startDelay);
            endWait = new WaitForSeconds(endDelay);
        }

        private void Start()
        {
            InitSpawnPointsValidList();
            SpawnAllTanks();
            SetCameraTargets();

            followCamera.SetTarget(tankManagerArray[0].instance);     //����С��ͼ����Ŀ��Ϊ��һ�����

            // ��ʼ��Ϸѭ��������ʤ�ߣ����»غϣ�������Ϸ�ȣ�
            StartCoroutine(GameLoop());
        }

        // ��������̹�ˣ�������Һ�AI
        private void SpawnAllTanks()
        {
            for (int i = 0; i < tankManagerArray.Length; i++)
            {
                //��ȡ��Ч��������㣬��ÿ��̹��λ�ò�һ��
                Transform spawnPoint = GetRandomSpawnPoint(false);
                if (spawnPoint == null)
                    continue;

                tankManagerArray[i].instance = Instantiate(tankManagerArray[i].tankPerfab, spawnPoint.transform.position, spawnPoint.transform.rotation) as GameObject;
                tankManagerArray[i].playerNumber = i + 1;
                tankManagerArray[i].spawnPoint = spawnPoint;

                tankManagerArray[i].SetupTank(tankManagerArray[i].isAI, wayPointsList);
            }
        }

        // ��ʼ�����г�����
        private void InitSpawnPointsValidList()
        {
            spawnPointsValid = new List<bool>();
            for (int i = 0; i < spawnPointsList.Count; i++)
                spawnPointsValid.Add(true);
        }

        // ��ȡ���Ϊʹ�ù��Ĳ����㣬�Ƿ�����ظ���
        private Transform GetRandomSpawnPoint(bool canSame)
        {
            if (canSame)
                return spawnPointsList[Random.Range(0, spawnPointsList.Count)].transform;

            bool haveSpawnPoint = false;
            List<int> validPoints = new List<int>();        // �����Ч�������Ӧ����������

            for (int i = 0; i < spawnPointsList.Count; i++)
                if (spawnPointsValid[i])
                {
                    haveSpawnPoint = true;
                    validPoints.Add(i);
                }

            if (haveSpawnPoint)
            {
                int spawnIndex = Random.Range(0, validPoints.Count);
                spawnPointsValid[validPoints[spawnIndex]] = false;  // ���ó�������Ч���Ѿ�ʹ�ù���
                return spawnPointsList[validPoints[spawnIndex]].transform;
            }

            return null;
        }

        // ������������̹��
        private void SetCameraTargets()
        {
            Transform[] targets = new Transform[tankManagerArray.Length];

            for (int i = 0; i < targets.Length; i++)
                targets[i] = tankManagerArray[i].instance.transform;

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
                roundWinner.winsTime++;

            gameWinner = GetGameWinner();                   // ��ȡ���ջ�ʤ���

            string message = EndMessage();                  // ��ȡ������Ϣ����ʾ֮
            messageText.text = message;

            yield return endWait;
        }

        // �����Ƿ�С�ڵ���һ��̹�˴�0��˵����ͬ���ˣ�
        private bool OneTankLeft()
        {
            int numTanksLeft = 0;

            for (int i = 0; i < tankManagerArray.Length; i++)
                if (tankManagerArray[i].instance.activeSelf)
                    numTanksLeft++;

            return numTanksLeft <= 1;
        }

        // ��ȡ��ʤ����ң�Ϊ�վ���ƽ��
        private TankManager GetRoundWinner()
        {
            for (int i = 0; i < tankManagerArray.Length; i++)
                if (tankManagerArray[i].instance.activeSelf)
                    return tankManagerArray[i];

            return null;
        }

        // ��ȡ����ʤ�������
        private TankManager GetGameWinner()
        {
            for (int i = 0; i < tankManagerArray.Length; i++)
                if (tankManagerArray[i].winsTime == numRoundsToWin)
                    return tankManagerArray[i];

            return null;
        }

        // ��ȡ�غϻ��ܵ���Ϸ������Ϣ
        private string EndMessage()
        {
            string message = "DRAW!";                       // Ĭ��ƽ��

            // ��ӻ�ʤ��ҵĴ���ɫ����������ַ���
            if (roundWinner != null)
                message = roundWinner.coloredPlayerText + " WINS THE ROUND!";

            message += "\n\n";

            // ���������һ�ʤ����
            for (int i = 0; i < tankManagerArray.Length; i++)
                message += tankManagerArray[i].coloredPlayerText + ": " + tankManagerArray[i].winsTime + " WINS\n";

            // �������ʤ���
            if (gameWinner != null)
                message = gameWinner.coloredPlayerText + " WINS THE GAME!";

            return message;
        }

        // ��������̹��
        private void ResetAllTanks()
        {
            InitSpawnPointsValidList();                     // ��ʼ��������
            for (int i = 0; i < tankManagerArray.Length; i++)
            {
                //��ȡ��Ч��������㣬��ÿ��̹��λ�ò�һ��
                Transform spawnPoint = GetRandomSpawnPoint(false);
                if (spawnPoint == null)
                    continue;
                tankManagerArray[i].spawnPoint = spawnPoint;
                tankManagerArray[i].Reset();
            }
        }

        // ����������ҿ���Ȩ
        private void EnableTankControl()
        {
            for (int i = 0; i < tankManagerArray.Length; i++)
                tankManagerArray[i].EnableControl();
        }

        // ����������ҿ���Ȩ
        private void DisableTankControl()
        {
            for (int i = 0; i < tankManagerArray.Length; i++)
                tankManagerArray[i].DisableControl();
        }
    }
}