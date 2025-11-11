using System.Collections;
using System.Net.NetworkInformation;
using UnityEngine;

public class PortalGate : MonoBehaviour
{
    public Animation m_Animation;
    public AnimationClip m_OpenAnimation;
    public AnimationClip m_CloseAnimation;
    public AnimationClip m_OpenedAnimationClip;
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
        m_Animation.Play(m_OpenedAnimationClip.name);
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
            m_Animation.Play(m_OpenAnimation.name);
            StartCoroutine(SetState(m_OpenAnimation.length, TState.OPENED));
        }
    }
    public void Close()
    {
        if (m_State == TState.OPENED)
        {
            m_State = TState.CLOSE;
            m_Animation.Play(m_CloseAnimation.name);
            StartCoroutine(SetState(m_CloseAnimation.length, TState.CLOSED));
        }
    }
    IEnumerator SetState(float AnimationTime, TState State)
    {
        yield return new WaitForSeconds(AnimationTime);
        m_State=State;
    }
}