using UnityEngine;

public class RefractionCube : MonoBehaviour
{
    public LineRenderer m_Laser;
    public float m_MaxDistance = 50.0f;
    public LayerMask m_LayerMask;
    bool m_IsReflectingLaser=false;
    bool m_AttachedObject = false;
    public Collider m_Collider;


    private void Start()
    {
        m_Laser.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (m_IsReflectingLaser)
        {
            UpdateLaser();
            m_IsReflectingLaser = false;
        }
        else
            m_Laser.gameObject.SetActive(false );
    }
    public void Reflect()
    {
        if(m_IsReflectingLaser)
            return;

        m_IsReflectingLaser = true;
        UpdateLaser();
    }

    void UpdateLaser()
    {
        m_Laser.gameObject.SetActive(true);

        float l_Distance = m_MaxDistance;
        Ray l_Ray = new Ray(m_Laser.transform.position, m_Laser.transform.forward);
        if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_MaxDistance, m_LayerMask.value, QueryTriggerInteraction.Ignore))
        {
            l_Distance = l_RaycastHit.distance;
            if (l_RaycastHit.collider.CompareTag("RefractionCube"))
                l_RaycastHit.collider.GetComponent<RefractionCube>().Reflect();
            else if (l_RaycastHit.collider.CompareTag("Player"))
            {
                PlayerController playerController = l_RaycastHit.collider.GetComponent<PlayerController>();
                if (playerController != null)
                    playerController.Kill();
            }
            else if (l_RaycastHit.collider.CompareTag("Turret"))
            {
                l_RaycastHit.collider.GetComponent<Turret>().KillTurret(l_RaycastHit.collider.gameObject);
            }
            else if (l_RaycastHit.collider.CompareTag("Button"))
            {
                PortalButton portalButton = l_RaycastHit.collider.GetComponent<PortalButton>();
                portalButton.OnTriggerEnter(m_Collider);
            }
        }
        Vector3 l_Position = new Vector3(0.0f, 0.0f, l_Distance);
        m_Laser.SetPosition(1, l_Position);
    }
    
    public void SetAttachedObject(bool AttachedObject)
    {
        m_AttachedObject = AttachedObject;
    }
}