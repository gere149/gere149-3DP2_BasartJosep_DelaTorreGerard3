using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.Progress;
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

    [Header("Text")]
    public Text m_AmmoText;
    public Text m_LifeText;
    public Text m_ShieldText;


    [Header("Shoot")]
    public float m_ShootMaxDistance = 50.0f;
    public int m_Ammo = 120;
    public int m_TotalMaxAmmo = 120;
    public int m_LoaderSize = 12;
    public int m_CurrentAmmo = 12;
    public float m_CooldownBetweenShots = 0.2f;
    private float m_ShootTimer = 0f;
    public float m_ReloadTime = 2f;
    private bool m_IsReloading = false;
    private bool m_CanShoot = true;


    [Header("Input")]
    public KeyCode m_LeftKeyCode = KeyCode.A;
    public KeyCode m_RightKeyCode = KeyCode.D;
    public KeyCode m_UpKeyCode = KeyCode.W;
    public KeyCode m_DownKeyCode = KeyCode.S;
    public KeyCode m_JumpKeyCode = KeyCode.Space;
    public KeyCode m_RunKeyCode = KeyCode.LeftShift;
    public KeyCode m_ReloadKeyCode = KeyCode.R;
    public int m_BlueShootButton = 0;
    public int m_OrangeShootButton = 0;

    [Header("Animations")]
    public Animation m_Animation;
    public AnimationClip m_IdleAnimationClip;
    public AnimationClip m_ShootAnimationClip;

    [Header("Debug Input")]
    public KeyCode m_DebugLockAngleKeyCode = KeyCode.I;

    [Header("Teleport")]
    public float m_PortalDistance = 1.5f;
    Vector3 m_MovementDirection;
    public float m_MaxAngleToTeleport = 60f;

    [Header("Portals")]
    public Portal m_BluePortals;
    public Portal m_OrangePortals;

    void Start()
    {
        /*m_ShootParticlesPool = new PoolElements();
        m_ShootParticlesPool.Init(25, m_ShootParticles);
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
        SetIdleAnimation();
        UpdateAmmoHUD();
        UpdateLifeHUD();
        UpdateShieldHUD();*/
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

        if (CanShoot() && Input.GetMouseButtonDown(m_BlueShootButton))
            Shoot(m_BluePortals);
    }
    bool CanShoot()
    {
        return true;
    }
    void Shoot(Portal _Portal)
    {
        m_CanShoot = false;
        m_ShootTimer = m_CooldownBetweenShots;
        SetShootAnimation();
        if (m_CurrentAmmo > 0)
        {
            Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
            if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_ShootMaxDistance, _Portal.m_ValidPortalLayerMask.value))
            {
                if(_Portal.IsValidPosition(l_RaycastHit.point, l_RaycastHit.normal))
                {
                    _Portal.gameObject.SetActive(true);
                }
                else
                    _Portal.gameObject.SetActive(false);
            }
            m_CurrentAmmo--;
            UpdateAmmoHUD();
        }
        m_CanShoot = true;
    }
    void SetIdleAnimation()
    {
        m_Animation.CrossFade(m_IdleAnimationClip.name);
    }
    void SetShootAnimation()
    {
        m_Animation.CrossFade(m_ShootAnimationClip.name, 0.1f);
        m_Animation.CrossFadeQueued(m_IdleAnimationClip.name, 0.0f);
    }
    public void AddAmmo(int Ammo)
    {
        m_Ammo += Ammo;
        if (m_Ammo > 120)
            m_Ammo = 120;
        UpdateAmmoHUD();
    }
    public void UpdateAmmoHUD()
    {
        if (m_AmmoText != null)
            m_AmmoText.text = m_CurrentAmmo + " / " + m_Ammo;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Portal"))
        {
            Portal l_Portal=other.GetComponent<Portal>();
            if(CanTeleport(l_Portal))
                Teleport(other.GetComponent<Portal>());
        }
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
        Vector3 l_WorldPosition= _Portal.m_MirrorPortal.transform.TransformDirection(l_NextPosition);

        Vector3 l_WorldForward=transform.forward;
        Vector3 l_LocalForward=_Portal.m_OhterPortalTransform.InverseTransformDirection(l_WorldPosition);
        l_WorldForward=_Portal.m_MirrorPortal.transform.TransformDirection(l_LocalForward);

        m_CharacterController.enabled = false;
        transform.position = l_WorldPosition;
        transform.rotation = Quaternion.LookRotation(l_WorldForward, l_LocalForward);
        m_Yaw=transform.rotation.eulerAngles.y;
        m_CharacterController.enabled = true;
    }
    public void Restart()
    {
        m_CharacterController.enabled = false;
        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;
        m_CharacterController.enabled = true;
        UpdateAmmoHUD();
    }
}