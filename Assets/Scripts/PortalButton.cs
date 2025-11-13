using UnityEngine;
using UnityEngine.Events;

public class PortalButton : MonoBehaviour
{
    public UnityEvent m_Opening;
    public UnityEvent m_Closing;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cube") || other.CompareTag("RefractionCube"))
            m_Opening.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Cube"))
            m_Closing.Invoke();
    }
}