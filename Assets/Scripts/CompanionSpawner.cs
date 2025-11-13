using UnityEngine;

public class CompanionSpawner : MonoBehaviour
{
    public GameObject m_CompanionCubePrefab;
    public Transform m_SpawnerTransform;
    private bool m_InRange;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && m_InRange)
        {
            Spawn();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            m_InRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            m_InRange = false;
    }
    void Spawn()
    {
        GameObject l_GameObject =GameObject.Instantiate(m_CompanionCubePrefab);
        l_GameObject.transform.position=m_SpawnerTransform.position;
        l_GameObject.transform.rotation=m_SpawnerTransform.rotation;
    }
}