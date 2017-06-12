using UnityEngine;

namespace Complete
{
    public class ShellExplosion : MonoBehaviour
    {
        public LayerMask layerMask;                         // ̹�����֣�"Leve"��
        public ParticleSystem explosionParticles;           // ��ը����
        public AudioSource explosionAudio;                  // ��ը����
        public float maxDamage = 100f;                      // ����˺�
        public float explosionForce = 1000f;                // ��ը���ĵ�����
        public float maxLifeTime = 2f;                      // ը���������ʱ��
        public float explosionRadius = 5f;                  // ��ը�뾶

        private void Start ()
        {
            Destroy (gameObject, maxLifeTime);
        }

        // �������κ�����
        private void OnTriggerEnter (Collider other)
        {
			// ��ȡ��ը��Χ������
            Collider[] colliders = Physics.OverlapSphere (transform.position, explosionRadius, layerMask);

            for (int i = 0; i < colliders.Length; i++)
            {
                Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody> ();
                if (!targetRigidbody)
                    continue;

                // ��һ����ը��
                targetRigidbody.AddExplosionForce (explosionForce, transform.position, explosionRadius);

                // ��ȡĿ���Ѫ���������Ѫ����������Ѫ��
                TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth> ();
                if (!targetHealth)
                    continue;
                float damage = CalculateDamage (targetRigidbody.position);
                targetHealth.TakeDamage (damage);
            }

            // ��ʾ��ը����
            explosionParticles.transform.parent = null;
            explosionParticles.Play();

            // ������ը��Ч
            explosionAudio.Play();

            ParticleSystem.MainModule mainModule = explosionParticles.main;
            Destroy (explosionParticles.gameObject, mainModule.duration);
            Destroy (gameObject);
        }

        // ���ݾ�������˺�
        private float CalculateDamage (Vector3 targetPosition)
        {
            // ���㱬ը���ľ�����Լ��ľ���
            Vector3 explosionToTarget = targetPosition - transform.position;
            float explosionDistance = explosionToTarget.magnitude;

            // ת���ɱ���
            float relativeDistance = (explosionRadius - explosionDistance) / explosionRadius;

            // ���ݱ��������˺�
            float damage = relativeDistance * maxDamage;

            damage = Mathf.Max (0f, damage);
            return damage;
        }
    }
}