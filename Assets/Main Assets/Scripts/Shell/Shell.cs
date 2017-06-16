using UnityEngine;

public class Shell : MonoBehaviour
{
    public ObjectPool shellExplosionPool;               // ��ը���Գ�

    public LayerMask layerMask;                         // ̹�����֣�"Level"��
    public float maxDamage = 100f;                      // ����˺�
    public float explosionForce = 1000f;                // ��ը���ĵ�����
    public float maxLifeTime = 2f;                      // ը���������ʱ��
    public float explosionRadius = 5f;                  // ��ը�뾶


    // �������κ�����
    private void OnTriggerEnter(Collider other)
    {
        // �ӱ�ը���л�ȡ���󣬲�����λ�ã���ʾ֮
        shellExplosionPool.SetNextObjectActive(transform);

        // ��ȡ��ը��Χ��������ײ��
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, layerMask);

        for (int i = 0; i < colliders.Length; i++)
        {
            Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();
            if (!targetRigidbody)
                continue;

            // ��һ����ը��
            targetRigidbody.AddExplosionForce(explosionForce, transform.position, explosionRadius);

            // ��ȡĿ���Ѫ���������Ѫ����������Ѫ��
            TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();
            if (!targetHealth)
                continue;
            targetHealth.TakeDamage(CalculateDamage(targetRigidbody.position));
        }

        gameObject.SetActive(false);
    }

    // ���ݾ�������˺�
    private float CalculateDamage(Vector3 targetPosition)
    {
        // ���㱬ը���ľ�����Լ��ľ���
        Vector3 explosionToTarget = targetPosition - transform.position;
        float explosionDistance = explosionToTarget.magnitude;

        // ת���ɱ���
        float relativeDistance = (explosionRadius - explosionDistance) / explosionRadius;

        // ���ݱ��������˺�
        return Mathf.Max(0f, relativeDistance * maxDamage);
    }

}
