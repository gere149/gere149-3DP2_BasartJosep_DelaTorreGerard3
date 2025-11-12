using UnityEngine;

public class CompanionCube : MonoBehaviour
{
    Rigidbody m_Rigidbody;
    Vector3 m_Scale;
    public float m_PortaDistance = 1.5f;
    public float m_MaxAngleToTeleport = 25.0f;
    bool m_AttachedObject = false;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Scale = transform.localScale;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Portal"))
        {
            Portal l_Portal=other.GetComponent<Portal>();
            if(CanTeleport(l_Portal))
                Teleport(l_Portal);
        }
    }
    bool CanTeleport(Portal _Portal)
    {
        float l_DotValue = Vector3.Dot(_Portal.transform.forward, -m_Rigidbody.linearVelocity.normalized);
        return !m_AttachedObject && l_DotValue > Mathf.Cos(m_MaxAngleToTeleport * Mathf.Deg2Rad);
    }
    void Teleport(Portal _Portal)
    {
        Vector3 l_Direccion = m_Rigidbody.linearVelocity.normalized;
        Vector3 l_WorldPosition=transform.position+l_Direccion*m_PortaDistance;
        Vector3 l_LocalPosition=_Portal.m_OhterPortalTransform.InverseTransformPoint(l_WorldPosition);
        transform.position = _Portal.m_MirrorPortal.transform.TransformPoint(l_LocalPosition);

        Vector3 l_WorldDirection = transform.forward;
        Vector3 l_LocalDirection = _Portal.m_OhterPortalTransform.InverseTransformDirection(l_WorldDirection);
        transform.forward = _Portal.m_MirrorPortal.transform.TransformDirection(l_LocalDirection);

        Vector3 l_LocalVelocity = _Portal.m_OhterPortalTransform.InverseTransformDirection(m_Rigidbody.linearVelocity);
        m_Rigidbody.linearVelocity=_Portal.m_MirrorPortal.transform.TransformDirection(l_LocalVelocity);

        float l_Sclae=_Portal.m_MirrorPortal.transform.localScale.x/_Portal.transform.localScale.x;
        transform.localScale = m_Scale * l_Sclae;
    }

    public void SetAttachedObject(bool AttachedObject)
    {
        m_AttachedObject = AttachedObject;
    }
}