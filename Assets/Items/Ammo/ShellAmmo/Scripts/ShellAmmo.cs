using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Item.Ammo
{
    public class ShellAmmo : AmmoBase
    {
        public ObjectPool shellExplosionPool;               // ��ը���Գ�
        public float explosionForce = 100f;                 // ��ը���ĵ�����
        public float explosionRadius = 5f;                  // ��ը�뾶

        private HealthManager targetHealth;                 // Ŀ��Ѫ��
        private List<HealthManager> validTargets;           // ��ʱ��Ч����б�        

        protected new void Awake()
        {
            base.Awake();
            validTargets = new List<HealthManager>();
        }

        protected override void OnCollision(Collider other)
        {
            // �ӱ�ը���л�ȡ���󣬲�����λ�ã���ʾ֮
            shellExplosionPool.GetNextObject(transform: transform);

            // ��ȡ��ը��Χ��������ײ��
            FindValidTargets(Physics.OverlapSphere(transform.position, explosionRadius),ref validTargets);

            for (int i = 0; i < validTargets.Count; i++)
                TakeDamage(validTargets[i]);
        }

        /// <summary>
        /// ������Ч�����б�
        /// </summary>
        /// <param name="colliders">������ײ��</param>
        /// <param name="targets">�������ЧĿ���б�</param>
        private void FindValidTargets(Collider[] colliders,ref List<HealthManager> targets)
        {
            targets.Clear();
            for (int i = 0; i < colliders.Length; i++)
            {
                targetHealth = colliders[i].GetComponent<HealthManager>();
                if (!targetHealth)
                    continue;
                if (!targets.Contains(targetHealth))
                    targets.Add(targetHealth);
            }
        }

        /// <summary>
        /// ��ȡĿ���Ѫ���������Ѫ����������Ѫ��
        /// </summary>
        /// <param name="targetHealth">Ŀ���Ѫ��</param>
        private void TakeDamage(HealthManager targetHealth)
        {
            // ����Ŀ����뱬ը���ı���ֵ��0 ~ 1,0Ϊ�У���Խ�����˺�Խ�����Ե�
            targetHealth.SetHealthAmount(-1 * Mathf.Max(0f, GameMathf.Persents(explosionRadius, 0, (targetHealth.transform.position - transform.position).magnitude) * damage), launcher);
        }


    }
}