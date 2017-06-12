using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Complete
{
    public class GameManager : MonoBehaviour
    {
        public int m_NumRoundsToWin = 5;            // Ӯ����Ϸ��ҪӮ�Ļغ���
        public float m_StartDelay = 3f;             // ��ʼ��ʱʱ��
        public float m_EndDelay = 3f;               // ������ʱʱ��
        public CameraControl m_CameraControl;       // ������ƽű�
        public CameraFollowTarget followCamera;     // �������������С��ͼ
        public Text m_MessageText;                  // UI�ı�����һ�ʤ�ȣ�
        public TankManager[] m_Tanks;               // ̹�˹�����
        public List<Transform> wayPointsForAI;      // AI��Ѳ�ߵ��б�

        private int m_RoundNumber;                  // ��ǰ�غ���
        private WaitForSeconds m_StartWait;         // ��ʼ�غ���ʱ
        private WaitForSeconds m_EndWait;           // �����غ���ʱ
        private TankManager m_RoundWinner;          // ��ǰ�غϻ�ʤ���
        private TankManager m_GameWinner;           // ���ջ�ʤ���

        private void Awake()
        {
            m_StartWait = new WaitForSeconds(m_StartDelay);
            m_EndWait = new WaitForSeconds(m_EndDelay);
        }

        private void Start()
        {
            SpawnAllTanks();
            SetCameraTargets();

            followCamera.SetTarget(m_Tanks[0].m_Instance);     //����С��ͼ����Ŀ��Ϊ��һ�����

            // ��ʼ��Ϸѭ��������ʤ�ߣ����»غϣ�������Ϸ�ȣ�
            StartCoroutine(GameLoop());
        }

        // ��������̹�ˣ�������Һ�AI
        private void SpawnAllTanks()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].m_Instance =
                    Instantiate(m_Tanks[i].tankPerfab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
                m_Tanks[i].m_PlayerNumber = i + 1;

                m_Tanks[i].SetupTank(m_Tanks[i].isAI, wayPointsForAI);
            }
        }

        // ������������̹��
        private void SetCameraTargets()
        {
            Transform[] targets = new Transform[m_Tanks.Length];

            for (int i = 0; i < targets.Length; i++)
                targets[i] = m_Tanks[i].m_Instance.transform;

            m_CameraControl.m_Targets = targets;
        }

        // ��Ϸ��Э��
        private IEnumerator GameLoop()
        {
            yield return StartCoroutine(RoundStarting());           //�غϿ�ʼ����һ����ʱ

            yield return StartCoroutine(RoundPlaying());            //�غ��У�����һ��̹�˴��ʱһֱ��������ѭ��

            yield return StartCoroutine(RoundEnding());             //�غϽ���

            // �����������Ϸ�����¼��س��������������һ�غ�
            if (m_GameWinner != null)
                SceneManager.LoadScene(0);
            else
                StartCoroutine(GameLoop());
        }

        // �غϿ�ʼ
        private IEnumerator RoundStarting()
        {
            ResetAllTanks();                                // ��������̹��
            DisableTankControl();                           // �����������ǵĿ���Ȩ

            m_CameraControl.SetStartPositionAndSize();      // �������

            ++m_RoundNumber;                                // �غ�������                
            m_MessageText.text = "ROUND " + m_RoundNumber;

            yield return m_StartWait;                       // ��ʱһ��ʱ���ٿ�ʼ
        }

        // �غ���
        private IEnumerator RoundPlaying()
        {
            EnableTankControl();                            // ������ҿ���Ȩ

            m_MessageText.text = string.Empty;              // �����ʾ��Ϣ

            while (!OneTankLeft())                          // ֻʣһ��̹�˲Ž�����Э��
                yield return null;
        }

        // �غϽ���
        private IEnumerator RoundEnding()
        {
            DisableTankControl();                           // ������ҿ���Ȩ

            m_RoundWinner = GetRoundWinner();               // ��ȡ�غ�ʤ�������

            if (m_RoundWinner != null)                      // ��Ϊ�վ͸�ʤ����Ҽӻ�ʤ����
                m_RoundWinner.m_Wins++;

            m_GameWinner = GetGameWinner();                 // ��ȡ���ջ�ʤ���

            string message = EndMessage();                  // ��ȡ������Ϣ����ʾ֮
            m_MessageText.text = message;

            yield return m_EndWait;
        }

        // �����Ƿ�С�ڵ���һ��̹�˴�0��˵����ͬ���ˣ�
        private bool OneTankLeft()
        {
            int numTanksLeft = 0;

            for (int i = 0; i < m_Tanks.Length; i++)
                if (m_Tanks[i].m_Instance.activeSelf)
                    numTanksLeft++;

            return numTanksLeft <= 1;
        }

        // ��ȡ��ʤ����ң�Ϊ�վ���ƽ��
        private TankManager GetRoundWinner()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
                if (m_Tanks[i].m_Instance.activeSelf)
                    return m_Tanks[i];

            return null;
        }

        // ��ȡ����ʤ�������
        private TankManager GetGameWinner()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
                if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                    return m_Tanks[i];

            return null;
        }

        // ��ȡ�غϻ��ܵ���Ϸ������Ϣ
        private string EndMessage()
        {
            string message = "DRAW!";                       // Ĭ��ƽ��

            // ��ӻ�ʤ��ҵĴ���ɫ����������ַ���
            if (m_RoundWinner != null)
                message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

            message += "\n\n\n\n";

            // ���������һ�ʤ����
            for (int i = 0; i < m_Tanks.Length; i++)
                message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";

            // �������ʤ���
            if (m_GameWinner != null)
                message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

            return message;
        }

        // ��������̹��
        private void ResetAllTanks()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
                m_Tanks[i].Reset();
        }

        // ����������ҿ���Ȩ
        private void EnableTankControl()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
                m_Tanks[i].EnableControl();
        }

        // ����������ҿ���Ȩ
        private void DisableTankControl()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
                m_Tanks[i].DisableControl();
        }
    }
}