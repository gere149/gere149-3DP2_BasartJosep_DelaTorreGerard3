using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.Progress;
using Cursor = UnityEngine.Cursor;
public class PlayerController : MonoBehaviour
{
    Vector3 m_StartPosition;
    Quaternion m_StartRotation;
    float m_Yaw;
    float m_Pitch;
    public float m_YawSpeed;
    public float m_PitchSpeed;
    public float m_MinPitch;
    public float m_MaxPitch;
    public Transform m_PitchController;
    public bool m_UseInvertedYaw;
    public bool m_UseInvertedPitch;
    public CharacterController m_CharacterController;
    float m_VerticalSpeed = 0.0f;

    bool m_AngleLocked;
    public float m_Speed;
    public float m_JumpSpeed;
    public float m_SpeedMultiplier;
    public Camera m_Camera;

    [Header("Shoot")]
    public float m_ShootMaxDistance = 50.0f;
    public float m_CooldownBetweenShots = 0.2f;
    private float m_ShootTimer = 0f;
    //private bool m_CanShoot = true;


    [Header("Input")]
    public KeyCode m_LeftKeyCode = KeyCode.A;
    public KeyCode m_RightKeyCode = KeyCode.D;
    public KeyCode m_UpKeyCode = KeyCode.W;
    public KeyCode m_DownKeyCode = KeyCode.S;
    public KeyCode m_JumpKeyCode = KeyCode.Space;
    public KeyCode m_RunKeyCode = KeyCode.LeftShift;
    public KeyCode m_GrabKeyCode = KeyCode.E;
    public int m_BlueShootButton = 0;
    public int m_OrangeShootButton = 1;

    [Header("Animations")]
    public Animation m_Animation;
    public AnimationClip m_IdleAnimationClip;
    public AnimationClip m_ShootAnimationClip;

    [Header("Debug Input")]
    public KeyCode m_DebugLockAngleKeyCode = KeyCode.I;

    [Header("Teleport")]
    public float m_PortalDistance = 1.5f;
    Vector3 m_MovementDirection;
    public float m_MaxAngleToTeleport = 60.0f;

    [Header("Portals")]
    public Portal m_BluePortal;
    public Portal m_OrangePortal;
    public Portal m_FrameBluePortal;
    public Portal m_FrameOrangePortal;

    [Header("AttachedObject")]
    public ForceMode m_ForceMode;
    public float m_AttachMaxDistance = 10.0f;
    public float m_ThrowForce = 10.0f;
    public Transform m_GripTransform;
    Rigidbody m_AttachedObjectRigidbody;
    bool m_AttachingObject;
    Vector3 m_StartAttachingObjectPosition;
    float m_AttachingCurrentTime;
    public float m_AttachingTime = 0.4f;
    public float m_AttachingObjectRotationDistanceLerp = 2.0f;
    bool m_AttachedObject;
    public LayerMask m_ValidAttachObjectsLayerMask;


    [Header("Portal Preview and Scale")]
    public Transform m_BluePortalTransform;
    public Transform m_OrangePortalTransform;

    private float[] m_PortalScales = new float[] { 0.5f, 1f, 2f };
    private int m_CurrentScale = 1;


    void Start()
    {
        //m_ShootParticlesPool = new PoolElements();
        //m_ShootParticlesPool.Init(25, m_ShootParticles);
        PlayerController l_Player = GameManager.GetGameManager().GetPLayer();
        if (l_Player != null)
        {
            l_Player.m_CharacterController.enabled = false;
            l_Player.transform.position = transform.position;
            l_Player.transform.rotation = transform.rotation;
            l_Player.m_CharacterController.enabled = true;
            l_Player.m_StartPosition = transform.position;
            l_Player.m_StartRotation = transform.rotation;
            GameObject.Destroy(gameObject);
            return;
        }
        m_StartPosition = transform.position;
        m_StartRotation = transform.rotation;
        DontDestroyOnLoad(gameObject);
        GameManager.GetGameManager().SetPlayer(this);
        Cursor.lockState = CursorLockMode.Locked;
        //SetIdleAnimation();
    }

    void Update()
    {
        if (m_ShootTimer > 0f)
            m_ShootTimer -= Time.deltaTime;

        float l_MouseX = Input.GetAxis("Mouse X");
        float l_MouseY = Input.GetAxis("Mouse Y");

        if (Input.GetKeyDown(m_DebugLockAngleKeyCode))
            m_AngleLocked = !m_AngleLocked;

        if (!m_AngleLocked)
        {
            m_Yaw = m_Yaw + l_MouseX * m_YawSpeed * Time.deltaTime * (m_UseInvertedYaw ? -1.0f : 1.0f);
            m_Pitch = m_Pitch + l_MouseY * m_PitchSpeed * Time.deltaTime * (m_UseInvertedPitch ? -1.0f : 1.0f);
            m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);
            transform.rotation = Quaternion.Euler(0.0f, m_Yaw, 0.0f);
            m_PitchController.localRotation = Quaternion.Euler(m_Pitch, 0.0f, 0.0f);
        }

        Vector3 l_Movement = Vector3.zero;
        float l_YawPiRadins = m_Yaw * Mathf.Deg2Rad;
        float l_Yaw90PiRadians = (m_Yaw + 90) * Mathf.Deg2Rad;
        Vector3 l_ForwardDirection = new Vector3(Mathf.Sin(l_YawPiRadins), 0.0f, Mathf.Cos(l_YawPiRadins));
        Vector3 l_RightDirection = new Vector3(Mathf.Sin(l_Yaw90PiRadians), 0.0f, Mathf.Cos(l_Yaw90PiRadians));


        if (Input.GetKey(m_RightKeyCode))
            l_Movement = l_RightDirection;
        else if (Input.GetKey(m_LeftKeyCode))
            l_Movement = -l_RightDirection;

        if (Input.GetKey(m_UpKeyCode))
            l_Movement += l_ForwardDirection;
        else if (Input.GetKey(m_DownKeyCode))
            l_Movement -= l_ForwardDirection;

        float l_SpeedMultiplier = 1.0f;

        if (Input.GetKey(m_RunKeyCode))
            l_SpeedMultiplier = m_SpeedMultiplier;

        l_Movement.Normalize();
        m_MovementDirection = l_Movement;
        l_Movement *= m_Speed * l_SpeedMultiplier * Time.deltaTime;

        m_VerticalSpeed += Physics.gravity.y * Time.deltaTime;
        l_Movement.y = m_VerticalSpeed * Time.deltaTime;

        CollisionFlags l_CollisionFlags = m_CharacterController.Move(l_Movement);
        if (m_VerticalSpeed < 0.0f && (l_CollisionFlags & CollisionFlags.Below) != 0) //si estoy cyendoo colisiono con el suelo
        {
            m_VerticalSpeed = 0.0f;
            if (Input.GetKeyDown(m_JumpKeyCode))
                m_VerticalSpeed = m_JumpSpeed;

        }
        else if (m_VerticalSpeed > 0.0f && (l_CollisionFlags & CollisionFlags.Above) != 0) //si estyoy subiendo y colision con un techo  
            m_VerticalSpeed = 0.0f;

        PortalScale(m_BluePortal, m_BlueShootButton);
        PortalScale(m_OrangePortal, m_OrangeShootButton);

        if (CanShoot() && Input.GetMouseButton(m_BlueShootButton))
                Shoot(m_BluePortal);
        else if (Input.GetMouseButtonUp(m_BlueShootButton))
            Shoot(m_BluePortal);

        if (CanShoot() && Input.GetMouseButton(m_OrangeShootButton))
                Shoot(m_OrangePortal);
        else if (Input.GetMouseButtonUp(m_OrangeShootButton))
            Shoot(m_OrangePortal);


        if (CanAttachObject())
            AttachObject();

        if (m_AttachedObjectRigidbody != null)
            UpdateAttachedObject();
    }
    void PortalScale(Portal _Portal, int _ShootButton)
    {
        if (Input.GetMouseButton(_ShootButton) && _Portal.gameObject.activeSelf)
        {
            float scroll = Input.mouseScrollDelta.y;
            if (scroll > 0f)
            {
                m_CurrentScale = (m_CurrentScale + 1) % m_PortalScales.Length;
            }
            else if (scroll < 0f)
            {
                m_CurrentScale--;
                if (m_CurrentScale < 0)
                    m_CurrentScale = m_PortalScales.Length - 1;
            }

            float l_NewScale = m_PortalScales[m_CurrentScale];
            _Portal.transform.localScale = Vector3.one * l_NewScale;
            _Portal.m_MirrorPortal.transform.localScale = Vector3.one * l_NewScale;
        }
    }
    bool CanAttachObject()
    {
        return true;
    }
    bool CanShoot()
    {
        return m_AttachedObjectRigidbody ==null;
    }
    void Shoot(Portal _Portal)
    {
        //m_CanShoot = false;
        Debug.Log("Shoot");
        m_ShootTimer = m_CooldownBetweenShots;
        //SetShootAnimation();
        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_ShootMaxDistance, _Portal.m_ValidPortalLayerMask.value, QueryTriggerInteraction.Ignore))
        {
            if (l_RaycastHit.collider.CompareTag("DrawableWall"))
            {
                if (_Portal.IsValidPosition(l_RaycastHit.point, l_RaycastHit.normal))
                {
                    _Portal.gameObject.SetActive(true);
                }
                else
                    _Portal.gameObject.SetActive(false);
            }
        }
        //m_CanShoot = true;
    }
    public void Kill()
    {
        GameManager.GetGameManager().RestartLevel();
    }
    void SetIdleAnimation()
    {
        m_Animation.CrossFade(m_IdleAnimationClip.name);
    }
    void SetShootAnimation()
    {
        //m_Animation.CrossFade(m_ShootAnimationClip.name, 0.1f);
        //m_Animation.CrossFadeQueued(m_IdleAnimationClip.name, 0.0f);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Portal"))
        {
            Portal l_Portal=other.GetComponent<Portal>();
            if(CanTeleport(l_Portal))
                Teleport(other.GetComponent<Portal>());
        }
        else if (other.CompareTag("DeadZone"))
            Kill();
    }
    bool CanTeleport(Portal _Portal)
    {
        float l_DotValue=Vector3.Dot(_Portal.transform.forward, -m_MovementDirection);
        return l_DotValue > Mathf.Cos(m_MaxAngleToTeleport * Mathf.Deg2Rad);
    }
    void Teleport(Portal _Portal)
    {
        Vector3 l_NextPosition = transform.position + m_MovementDirection * m_PortalDistance;
        Vector3 l_localPosition= _Portal.m_OhterPortalTransform.InverseTransformPoint(l_NextPosition);
        Vector3 l_WorldPosition= _Portal.m_MirrorPortal.transform.TransformPoint(l_localPosition);

        Vector3 l_WorldForward=transform.forward;
        Vector3 l_LocalForward=_Portal.m_OhterPortalTransform.InverseTransformDirection(l_WorldForward);
        l_WorldForward=_Portal.m_MirrorPortal.transform.TransformDirection(l_LocalForward);

        m_CharacterController.enabled = false;
        transform.position = l_WorldPosition;
        transform.rotation = Quaternion.LookRotation(l_WorldForward);
        m_Yaw=transform.rotation.eulerAngles.y;
        m_CharacterController.enabled = true;
    }
    public void Restart()
    {
        m_CharacterController.enabled = false;
        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;
        m_CharacterController.enabled = true;
    }

   
    //--------------------------------------AttachObjects ------------------------------------------------
    void AttachObject()
    {
        if (Input.GetKeyDown(m_GrabKeyCode))
        {
            Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
            if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_AttachMaxDistance, m_ValidAttachObjectsLayerMask.value, QueryTriggerInteraction.Ignore))
            {
                if (l_RaycastHit.collider.CompareTag("Cube"))
                    AttachObject(l_RaycastHit.rigidbody);
                else if (l_RaycastHit.collider.CompareTag("Turret"))
                    AttachObject(l_RaycastHit.rigidbody);
            }
        }
    }
    void AttachObject(Rigidbody _Rigidbody)
    {
        m_AttachingObject = true;
        m_AttachedObjectRigidbody= _Rigidbody;
        if (m_AttachedObjectRigidbody.GetComponent<CompanionCube>())
        {
            m_AttachedObjectRigidbody.GetComponent<CompanionCube>().SetAttachedObject(true);
        }
        m_StartAttachingObjectPosition =_Rigidbody.transform.position;
        m_AttachingCurrentTime = 0.0f;
        m_AttachedObject = false;
    }
    void UpdateAttachedObject()
    {
        if (m_AttachingObject)
        {
            m_AttachingCurrentTime += Time.deltaTime;
            float l_Pct = Mathf.Min(1.0f,m_AttachingCurrentTime / m_AttachingTime);
            Vector3 l_Position= Vector3.Lerp(m_StartAttachingObjectPosition, m_GripTransform.position, l_Pct);
            float l_Distance = Vector3.Distance(l_Position, m_GripTransform.position);
            float l_RotationPct = 1.0f - Mathf.Min(1.0f, l_Distance / m_AttachingObjectRotationDistanceLerp);
            Quaternion l_Rotation = Quaternion.Lerp(transform.rotation, m_GripTransform.rotation, l_RotationPct);
            m_AttachedObjectRigidbody.MovePosition(l_Position);
            m_AttachedObjectRigidbody.MoveRotation(l_Rotation);
            if (l_Pct == 1.0f)
            {
                m_AttachingObject = false;
                m_AttachedObject = true; ;
                m_AttachedObjectRigidbody.transform.SetParent(m_GripTransform);
                m_AttachedObjectRigidbody.transform.localPosition = Vector3.zero;
                m_AttachedObjectRigidbody.transform.localRotation=Quaternion.identity;
                m_AttachedObjectRigidbody.isKinematic = true;
            }
        }
        if (Input.GetMouseButtonUp(0))
            ThrowObject(m_ThrowForce);
        else if (Input.GetMouseButtonUp(1))
            ThrowObject(0.0f);
    }
    void ThrowObject(float Force)
    {
        m_AttachedObjectRigidbody.isKinematic=false;
        m_AttachedObjectRigidbody.AddForce(m_PitchController.forward * Force, m_ForceMode);
        m_AttachedObjectRigidbody.transform.SetParent(null);
        m_AttachingObject = false;
        m_AttachedObject = false;
        m_AttachedObjectRigidbody.GetComponent<CompanionCube>().SetAttachedObject(false);
        m_AttachedObjectRigidbody = null;
    }
}