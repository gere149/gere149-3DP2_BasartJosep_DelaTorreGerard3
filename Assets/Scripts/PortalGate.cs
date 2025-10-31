using System.Collections;
using System.Net.NetworkInformation;
using UnityEngine;

public class PortalGate : MonoBehaviour
{
    public Animation m_Animation;
    public AnimationClip m_OpenAnimation;
    public AnimationClip m_CloseAnimation;
    public AnimationClip m_OpenAnimationClip;
    public AnimationClip m_ClosedAnimaionClip;

    public bool m_IsOpened;

    public enum TState
    {
        OPENED,
        CLOSED,
        OPEN,
        CLOSE

    }
    TState m_State;

    private void Start()
    {
        if (m_IsOpened)
            SetOpenedState();
        else
            SetClosedState();
    }
    void SetOpenedState()
    {
        m_State = TState.OPENED;
        m_Animation.Play(m_OpenAnimationClip.name);
    }
    void SetClosedState()
    {
        m_State = TState.CLOSED;
        m_Animation.Play(m_ClosedAnimaionClip.name);
    }
    public void Open()
    {
        if (m_State == TState.CLOSED)
        {
            m_State=TState.OPEN;
            m_Animation.Play(m_OpenAnimationClip.name);
            StartCoroutine(SetState(m_ClosedAnimaionClip.length, TState.CLOSED));
        }
    }
    IEnumerator SetState(float AnimationTime, TState State)
    {
        yield return new WaitForSeconds(AnimationTime);
        m_State=State;
    }
}