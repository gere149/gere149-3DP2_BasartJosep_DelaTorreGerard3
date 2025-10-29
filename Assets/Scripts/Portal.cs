using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Camera m_Camera;
    public Transform m_OhterPortalTransform;
    public Portal m_MirrorPortal;
    public float m_NearCameraOffset = 0.5f;
    public List<Transform> m_ValidPositions;

    public float m_ValidDistanceOffset = 0.15f;
    public LayerMask m_ValidPortalLayerMask;
    public float m_MaxAnglePermited = 5.0f;

    public void LateUpdate()
    {
        Vector3 l_WorldPosition = Camera.main.transform.position;
        Vector3 l_LocalPosition = m_OhterPortalTransform.InverseTransformPoint(l_WorldPosition);

        m_MirrorPortal.m_Camera.transform.position = m_MirrorPortal.transform.TransformPoint(l_LocalPosition);

        Vector3 l_WorldForward = Camera.main.transform.forward;
        Vector3 l_LocalForward = m_OhterPortalTransform.InverseTransformDirection(l_WorldPosition);
        m_MirrorPortal.m_Camera.transform.forward = m_MirrorPortal.transform.TransformDirection(l_LocalForward);

        float l_DistanceToPortal=Vector3.Distance(m_MirrorPortal.transform.position,m_MirrorPortal.m_Camera.transform.position);
        m_MirrorPortal.m_Camera.nearClipPlane = l_DistanceToPortal + m_NearCameraOffset;
    }

    public bool IsValidPosition(Vector3 Position, Vector3 Normal)
    {
        gameObject.SetActive(false);
        transform.position = Position;
        transform.rotation=Quaternion.LookRotation(Normal);
        bool l_Valid = true;

        Vector3 l_CameraPosition = Camera.main.transform.position;
        for (int i = 0; i<m_ValidPositions.Count; ++i)
        {
            Vector3 l_ValidPositions = m_ValidPositions[i].position;
            Vector3 l_Direction = l_ValidPositions-l_CameraPosition;
            float l_Distance=Vector3.Distance(l_ValidPositions, l_CameraPosition);
            //l_Direction.Normalize();
            l_Direction /= l_Distance;
            Ray l_Ray = new Ray(l_CameraPosition, l_Direction);
            if(Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, l_Distance+m_ValidDistanceOffset, m_ValidPortalLayerMask.value))
            {
                if (l_RaycastHit.collider.CompareTag("DrawableWall"))
                {
                    if(Vector3.Distance(l_RaycastHit.point, l_ValidPositions) < m_ValidDistanceOffset)
                    {
                        float l_DotAngle=Vector3.Dot(l_RaycastHit.normal, m_ValidPositions[i].forward);
                        if (l_DotAngle > Mathf.Cos(m_MaxAnglePermited * Mathf.Deg2Rad))
                            l_Valid = false;
                    }
                }
                else
                    l_Valid = false;
            }
            else
                l_Valid = false;
        }
        return l_Valid;
    }
}
