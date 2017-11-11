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
        private List<HealthManager> validTargets = new List<HealthManager>();          // ��ʱ��Ч����б�        

        protected override void OnCollision(Collider other)
        {
        }

        protected override void OnCrashed(Collider other)
        {
            // �ӱ�ը���л�ȡ���󣬲�����λ�ã���ʾ֮
            shellExplosionPool.GetNextObject(true, transform);

            // ��ȡ��ը��Χ��������ײ��
            ComponentUtility.GetUniquelyComponentInParent(Physics.OverlapSphere(transform.position, explosionRadius), ref validTargets);

            for (int i = 0; i < validTargets.Count; i++)
                TakeDamage(validTargets[i]);

            gameObject.SetActive(false);
        }

        /// <summary>
        /// ��ȡĿ���Ѫ���������Ѫ����������Ѫ��
        /// </summary>
        /// <param name="targetHealth">Ŀ���Ѫ��</param>
        protected void TakeDamage(HealthManager targetHealth)
        {
            // ����Ŀ����뱬ը���ı���ֵ��0 ~ 1,0Ϊ�У���Խ�����˺�Խ�����Ե�
            targetHealth.SetHealthAmount(-1 * Mathf.Max(0f, GameMathf.Persents(explosionRadius, 0, (targetHealth.transform.position - transform.position).magnitude) * damage), launcher);
        }


    }
}