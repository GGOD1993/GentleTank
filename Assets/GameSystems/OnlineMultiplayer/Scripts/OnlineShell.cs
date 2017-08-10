using UnityEngine;

public class OnlineShell : Photon.MonoBehaviour
{
    public ObjectPool shellExplosionPool;               // ��ը���Գ�

    public LayerMask layerMask;                         // ̹�����֣�"Level"��
    public float maxDamage = 100f;                      // ����˺�
    public float explosionForce = 100f;                 // ��ը���ĵ�����
    public float maxLifeTime = 2f;                      // ը���������ʱ��
    public float explosionRadius = 5f;                  // ��ը�뾶

    private Collider[] colliders;                       // ��ײ������
    private Rigidbody targetRigidbody;                  // Ŀ�����
    private OnlineTankHealth targetHealth;              // Ŀ��Ѫ��
    private bool isExplosion;                           // ��ը����
    //private OnlineShellPool onlineShellPool;            // �ӵ���

    ///// <summary>
    ///// ��ʼ���������
    ///// </summary>
    //private void Awake()
    //{
    //    if (PhotonNetwork.isMasterClient)               // �������ͻ��˵���Ҫ�ֶ���ӵ��ӵ���
    //        return;
    //    onlineShellPool = GameObject.FindGameObjectWithTag("ShellPool").GetComponent<OnlineShellPool>();
    //    onlineShellPool.AddToPool(gameObject);
    //}

    /// <summary>
    /// �������κ����壬"Level"���ֲ���
    /// </summary>
    /// <param name="other">����������</param>
    private void OnTriggerEnter(Collider other)
    {
        gameObject.SetActive(false);
        if (PhotonNetwork.isMasterClient)               // ������Ϊ��׼
            photonView.RPC("Explosion", PhotonTargets.AllViaServer,transform.position);

        //PhotonNetwork.Destroy(photonView);
    }

    /// <summary>
    /// ��ը
    /// </summary>
    [PunRPC]
    public void Explosion(Vector3 position)
    {
        gameObject.SetActive(false);
        // �ӱ�ը���л�ȡ���󣬲�����λ�ã���ʾ֮
        shellExplosionPool.GetNextObject(transform: transform);

        if (isExplosion)
            return;
        isExplosion = true;
        // ��ȡ��ը��Χ��������ײ��
        colliders = Physics.OverlapSphere(position, explosionRadius, layerMask);

        for (int i = 0; i < colliders.Length; i++)
        {
            AddForce(colliders[i]);
            TakeDamage(position,colliders[i]);
        }

    }

    /// <summary>
    ///  ��һ����ը��
    /// </summary>
    /// <param name="collider">��ײ��������</param>
    private void AddForce(Collider collider)
    {
        targetRigidbody = collider.GetComponent<Rigidbody>();
        if (!targetRigidbody)
            return;
        targetRigidbody.AddExplosionForce(explosionForce, transform.position, explosionRadius);
    }

    /// <summary>
    /// ����λ�ø����˺�
    /// </summary>
    /// <param name="collider">��ײ������</param>
    private void TakeDamage(Vector3 center, Collider collider)
    {
        targetHealth = collider.GetComponent<OnlineTankHealth>();
        if (!targetHealth || !targetHealth.photonView.isMine)
            return;
        targetHealth.TakeDamage(CalculateDamage(center,targetRigidbody.position));
    }

    /// <summary>
    /// ���ݾ�������˺�
    /// </summary>
    /// <param name="center">��ը����λ��</param>
    /// <param name="targetPosition">Ŀ���λ��</param>
    /// <returns></returns>
    private float CalculateDamage(Vector3 center, Vector3 targetPosition)
    {
        // ���㱬ը���ľ�����Լ��ľ��룬��ת���ɱ���
        float relativeDistance = (explosionRadius - (targetPosition - center).magnitude) / explosionRadius;

        // ���ݱ��������˺�
        return Mathf.Max(0f, relativeDistance * maxDamage);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
