using UnityEngine;
using System.Linq;
using System.Collections.Generic;

using MathUtils;

[RequireComponent(typeof(vp_FPController))]
[RequireComponent(typeof(vp_WeaponHandler))]
public class vp_FPAccuracyController : MonoBehaviour
{
    #region public inspector properties

    public Camera WeaponCamera;
    public Material mat;

    #endregion

    #region Accuracy properties

    public float CurrentAccuracy { get; set; }
    public float MinAccuracy { get; set; }
    public float MaxAccuracy { get; set; }

    #endregion

    #region Aim Helper properties

    public AnimationCurve AimHelperCurve;

    // ADS snap
    public float ADSSnapDistance = 3;
    public float ADSSnapSpeed = 2;

    // Magnetism lock
    public float MagnetismLockDistance = .5f;
    public float MagnetismDuration = 3;


    //the help_aim effect distance  -- add by hxh
    public float AimMaxDistance = 50.0f;

    public float AimHelperCurrentFriction { get; private set; }
    public GameObject ClosestEnemy { get; private set; }

	//the block( wall, door or sth ) stop help_aim  -- add by hxh
    public List<string> BlockLayerMask = new List<string>();

    #endregion

    #region UFPS parameters

    private vp_FPPlayerEventHandler m_FPPlayer;

    private vp_FPWeaponHandler _weaponHandler;
    private vp_FPController _fpController;

    private vp_FPPlayerEventHandler FPPlayer
    {
        get
        {
            if (m_FPPlayer == null)
                m_FPPlayer = GameObject.FindObjectOfType(typeof(vp_FPPlayerEventHandler)) as vp_FPPlayerEventHandler;
            return m_FPPlayer;
        }
    }

    #endregion

    #region Private properties

    private GameObject _minCone, _currentCone, _maxCone;
    private bool _displayCones;
    private Vector3 _lastFpControllerForward;

    #endregion

    #region C/DTOR - Initialization

    void Awake()
    {
        _weaponHandler = GetComponent<vp_FPWeaponHandler>();
        _fpController = GetComponent<vp_FPController>();

        CreateAccuracyCones();
    }

    #endregion

    #region Accuracy cone management

    void DisplayCones(bool display)
    {
        _currentCone.GetComponent<MeshRenderer>().enabled = display;
        _minCone.GetComponent<MeshRenderer>().enabled = display;
        _maxCone.GetComponent<MeshRenderer>().enabled = display;
    }

    void CreateAccuracyCones()
    {
        var matred = new Material(mat);
        matred.SetColor("_EmissionColor", new Color(1, 0, 0, .5f));
        var matgreen = new Material(mat);
        matgreen.SetColor("_EmissionColor", new Color(0, 1, 0, 1));
        matgreen.color = new Color(0, 1, 0, 1);
        var matorange = new Material(mat);
        matorange.SetColor("_EmissionColor", new Color(1, 0.4f, 0, .5f));

        _currentCone = ConeUtils.CreateCone("current_accuracy", 0, matorange);
        _minCone = ConeUtils.CreateCone("min_accuracy", 0, matgreen);
        _maxCone = ConeUtils.CreateCone("max_accuracy", 0, matred);

        var p = WeaponCamera.transform.position + WeaponCamera.transform.forward * WeaponCamera.nearClipPlane;

        _minCone.transform.parent = WeaponCamera.transform;
        _minCone.transform.position = p;
        _minCone.transform.localRotation = Quaternion.identity;

        _currentCone.transform.parent = WeaponCamera.transform;
        _currentCone.transform.position = p + WeaponCamera.transform.forward * .01f;
        _currentCone.transform.localRotation = Quaternion.identity;

        _maxCone.transform.parent = WeaponCamera.transform;
        _maxCone.transform.position = p + WeaponCamera.transform.forward * .02f;
        _maxCone.transform.localRotation = Quaternion.identity;

        DisplayCones(_displayCones);
    }

    void UpdateAccuracyValuesAndCones()
    {
        MinAccuracy = _weaponHandler.CurrentShooter.ProjectileSpread;

        var moveAccuracy = Mathf.Lerp(0,
                                      _weaponHandler.CurrentShooter.MaxProjectileSpreadMove,
                                      _fpController.Velocity.magnitude / _fpController.MaxPlayerVelocity);

        var rotationAngle = Vector3.Angle(_fpController.transform.forward, _lastFpControllerForward);
        var rotateAccuracy = Mathf.Lerp(0,
                                        _weaponHandler.CurrentShooter.MaxProjectileSpreadRotate,
                                        rotationAngle / _fpController.MaxPlayerRotationAngle);

        var lfdt = _weaponHandler.CurrentShooter.LastFiresDeltaTime;
        var rofAccuracy = lfdt.Sum(f => Mathf.Lerp(0, _weaponHandler.CurrentShooter.MaxProjectileSpreadROF, 1 - f / _weaponHandler.CurrentShooter.MaxROFAffectTime));

        CurrentAccuracy = MinAccuracy + moveAccuracy + rotateAccuracy + rofAccuracy;

        MaxAccuracy = MinAccuracy +
                      _weaponHandler.CurrentShooter.MaxProjectileSpreadMove +
                      _weaponHandler.CurrentShooter.MaxProjectileSpreadRotate +
                      _weaponHandler.CurrentShooter.MaxProjectileSpreadROF * _weaponHandler.CurrentShooter.MaxROFAffectTime / _weaponHandler.CurrentShooter.ProjectileFiringRate;

        if (_displayCones)
        {
            _currentCone.UpdateCone(CurrentAccuracy);
            _minCone.UpdateCone(MinAccuracy);
            _maxCone.UpdateCone(MaxAccuracy);
        }
        _lastFpControllerForward = _fpController.transform.forward;
    }

    #endregion 

    #region UFPS listener registration

    /// <summary>
    /// registers this component with the event handler (if any)
    /// </summary>
    protected virtual void OnEnable()
    {
        if (FPPlayer != null)
            FPPlayer.Register(this);
    }

    /// <summary>
    /// unregisters this component from the event handler (if any)
    /// </summary>
    protected virtual void OnDisable()
    {
        if (FPPlayer != null)
            FPPlayer.Unregister(this);
    }

    #endregion

    #region Update loops

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            _displayCones = !_displayCones;
            DisplayCones(_displayCones);
        }

        //Initialize the current friction
        AimHelperCurrentFriction = 1;

        if (null == _weaponHandler || null == _weaponHandler.CurrentShooter) return;

        UpdateAccuracyValuesAndCones();
        ComputeAimHelperFriction();
    }

    #endregion

    #region Aim helper methods

/*	code from tamaya
    void ComputeAimHelperFriction()
    {
        var ray = new Ray(transform.position, transform.forward);
        var forwardAH = vp_AimHelper.AimHelpers.Where(ah => ray.IsPointForward(ah.transform.position));

        if (!forwardAH.Any()) return;

        ClosestEnemy = forwardAH.OrderByDescending(ah => ray.DistanceToLine(ah.transform.position)).First().gameObject;
        AimHelperCurrentFriction = AimHelperCurve.Evaluate(ray.DistanceToLine(ClosestEnemy.transform.position));
    }
*/

// changed by hxh
    void ComputeAimHelperFriction()
    {
        AimHelperCurrentFriction = AimHelperCurve.Evaluate(float.MaxValue);

        if (WeaponCamera == null)
        {            
            return;
        }
        ClosestEnemy = null;
		
		// the ray from camera direction
        var ray = new Ray(WeaponCamera.transform.position, WeaponCamera.transform.forward);
        
        var forwardAH = vp_AimHelper.AimHelpers.Where(ah => ray.IsPointForward(ah.transform.position));
        
        if (!forwardAH.Any()) return;
		// get the block mask	-- add by hxh
        int mask = 0;
        for( int i = 0; i < BlockLayerMask.Count; ++i )
        {
            int mask1 = LayerMask.NameToLayer(BlockLayerMask[i]);
            mask |= (1 << mask1);
        }
		
		// fix the multi enemys in scene -- add by hxh
        var rankedList = forwardAH.OrderByDescending(ah => (-1.0f * ray.DistanceToLine(ah.transform.position)));
        int count = rankedList.Count();
        for (int i = 0; i < rankedList.Count(); ++i)
        {
            vp_AimHelper helper = rankedList.ElementAt(i);
            if (helper != null)
            {
                Vector3 dir = Vector3.Normalize( helper.transform.position - WeaponCamera.transform.position );
                Ray testRay = new Ray(WeaponCamera.transform.position, dir);
                float length = Vector3.Distance(WeaponCamera.transform.position, helper.transform.position);
                // out the aim distance -- add by hxh
				if (length > AimMaxDistance)
                {
                    continue;
                }
                RaycastHit objGroup = new RaycastHit();


				//ignore the enemys which are behind -- add by hxh
                if (Physics.Raycast(testRay, out objGroup, length, mask))
                {
                    continue;
                }

                ClosestEnemy = helper.gameObject;
                break;
            }
        }

        if( ClosestEnemy == null )
        {            
            return;
        }
        
        float Dis = ray.DistanceToLine(ClosestEnemy.transform.position);
        AimHelperCurrentFriction = AimHelperCurve.Evaluate(Dis);

    }

    #endregion
}
