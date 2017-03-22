using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Basic.UnityPlayerPrefs;
using UnityEngine;

public class VariableModifierGUI : MonoBehaviour
{
    public vp_FPController FPController;
    public vp_FPInputMobile InputMobile;
    public vp_FPWeaponHandler WeaponHandler;

    public float MinWidthCaption = 100;
    public float MinWidthValue = 70;
    public float MinheightField = 30;

    public float GUIScale = 2;

    private string _presetName = "P1";

    private bool _display;
    private int _selected;

    private List<string> _savedPresetNames;

    #region Values to save

    // vp_FPController
    private float _fpControllerAcceleration,
        _fpControllerDamping,
        _fpControllerBackwardSpeed,
        _fpControllerAirSpeed;

    //vp_FPInputMobile
    private float _sensitivityX,
        _sensitivityY,
        _smoothStep,
        _smoothWeight;

    // vp_FPWeaponShooter
    // Projectile
    private float _projectileFiringRate,
        _projectileTapFiringRate,
        _projectileSpread,
        _projectileMaxSpreadWhenMoving,
        _projectileMaxSpreadWhenRotate,
        _projectileMaxSpreadRof;

    // Motion
    private float _motionPositionRecoilX,
        _motionPositionRecoilY,
        _motionPositionRecoilZ,
        _motionRotationRecoilX,
        _motionRotationRecoilY,
        _motionRotationRecoilZ,
        _motionCameraPositionRecoil,
        _motionCameraRotationRecoil;

    // vp_FPWeaponReloader
    private float _reloadDuration;

    #endregion

    void Start()
    {
        CheckExistingPresets();
        vp_GlobalEvent.Register("EditValues", EditValues);
    }

    void EditValues()
    {
        _display = !_display;
    }

    void CheckExistingPresets()
    {
        _savedPresetNames = new List<string>();
        var existingPresetNumber = 0;
        while (PlayerPrefs.HasKey("TuningPreset" + existingPresetNumber))
            _savedPresetNames.Add(PlayerPrefs.GetString("TuningPreset" + existingPresetNumber++));
    }

    private void OnGUI()
    {
        var oldMat = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(1, 1, 1) * GUIScale);

        GUILayout.BeginHorizontal();
        if (!_display) return;

        GUILayout.Space(120);
        DisplaySavedPresets();
        GUILayout.EndHorizontal();
        GUILayout.Space(25);

        GUILayout.BeginHorizontal();

        _selected = GUILayout.Toolbar(_selected, new [] {
            "vp_FPController",
            "vp_FPInputMobile",
            "vp_FPWeaponShooter",
            "vp_FPWeaponReloader"},
            GUILayout.Height(50));
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();

        if (_selected == 0)
            FPControllerGUI();
        if (_selected == 1)
            FPInputMobileGUI();
        if (_selected == 2)
            FPWeaponShooterGUI();
        if (_selected == 3)
            FPWeaponReloaderGUI();
        
        GUILayout.BeginHorizontal();
        _presetName = GUILayout.TextField(_presetName);
        bool init = GUILayout.Button("Init");
        if (init)
            Init();
        if (GUILayout.Button("Save"))
            Save(_presetName);

        if (GUILayout.Button("ToLobby"))
        {


        }

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();

        if (GUI.changed && !init)
            UpdateValues();

        if (init)
            Debug.Log(_fpControllerAcceleration);

        GUI.matrix = oldMat;


    }

    void Init()
    {
        Debug.Log("Initializing preset with current weapon values");

        _fpControllerAcceleration = FPController.MotorAcceleration;
        _fpControllerDamping = FPController.MotorDamping;
        _fpControllerBackwardSpeed = FPController.MotorBackwardsSpeed;
        _fpControllerAirSpeed = FPController.MotorAirSpeed;

        _sensitivityX = InputMobile.MouseLookSensitivity.x;
        _sensitivityY = InputMobile.MouseLookSensitivity.y;
        _smoothStep = InputMobile.MouseLookSmoothSteps;
        _smoothWeight = InputMobile.MouseLookSmoothWeight;

        var vpfpweaponshooter = WeaponHandler.CurrentShooter as vp_FPWeaponShooter;

        _projectileFiringRate = WeaponHandler.CurrentShooter.ProjectileFiringRate;
        if (null != vpfpweaponshooter)
        {
            _projectileTapFiringRate = vpfpweaponshooter.ProjectileTapFiringRate;
            _projectileSpread = vpfpweaponshooter.ProjectileSpread;
            _projectileMaxSpreadWhenMoving = vpfpweaponshooter.MaxProjectileSpreadMove;
            _projectileMaxSpreadWhenRotate = vpfpweaponshooter.MaxProjectileSpreadRotate;
            _projectileMaxSpreadRof = vpfpweaponshooter.MaxProjectileSpreadROF;

            _motionPositionRecoilX = vpfpweaponshooter.MotionPositionRecoil.x;
            _motionPositionRecoilY = vpfpweaponshooter.MotionPositionRecoil.y;
            _motionPositionRecoilZ = vpfpweaponshooter.MotionPositionRecoil.z;

            _motionRotationRecoilX = vpfpweaponshooter.MotionRotationRecoil.x;
            _motionRotationRecoilY = vpfpweaponshooter.MotionRotationRecoil.y;
            _motionRotationRecoilZ = vpfpweaponshooter.MotionRotationRecoil.z;

            _motionCameraPositionRecoil = vpfpweaponshooter.MotionPositionRecoilCameraFactor;
            _motionCameraRotationRecoil = vpfpweaponshooter.MotionRotationRecoilCameraFactor;
        }
        else
        {
            Debug.LogWarningFormat("Current selected weapon {0} cannot be casted to vp_FPWeaponShooter", 
                WeaponHandler.CurrentShooter.name);
        }
        var vpfpreloader = WeaponHandler.CurrentShooter.GetComponent<vp_FPWeaponReloader>();
        if (null != vpfpreloader)
            _reloadDuration = vpfpreloader.ReloadDuration;
        else
        {
            Debug.LogWarningFormat("Current weapon reloader cannot be found on component {0}", WeaponHandler.CurrentShooter.name);
        }
    }

    void UpdateValues()
    {
        FPController.MotorAcceleration = _fpControllerAcceleration;
        FPController.MotorDamping = _fpControllerDamping;
        FPController.MotorBackwardsSpeed = _fpControllerBackwardSpeed;
        FPController.MotorAirSpeed = _fpControllerAirSpeed;

        InputMobile.MouseLookSensitivity = new Vector2(_sensitivityX, _sensitivityY);
        InputMobile.MouseLookSmoothSteps = (int)_smoothStep;
        InputMobile.MouseLookSmoothWeight = _smoothWeight;

        var vpfpweaponshooter = WeaponHandler.CurrentShooter as vp_FPWeaponShooter;

        WeaponHandler.CurrentShooter.ProjectileFiringRate = _projectileFiringRate;
        if (null != vpfpweaponshooter)
        {
            vpfpweaponshooter.ProjectileTapFiringRate = _projectileTapFiringRate;
            vpfpweaponshooter.ProjectileSpread = _projectileSpread;
            vpfpweaponshooter.MaxProjectileSpreadMove = _projectileMaxSpreadWhenMoving;
            vpfpweaponshooter.MaxProjectileSpreadRotate = _projectileMaxSpreadWhenRotate;
            vpfpweaponshooter.MaxProjectileSpreadROF = _projectileMaxSpreadRof;

            vpfpweaponshooter.MotionPositionRecoil = new Vector3(_motionPositionRecoilX, _motionPositionRecoilY,
                _motionPositionRecoilZ);
            vpfpweaponshooter.MotionRotationRecoil = new Vector3(_motionRotationRecoilX, _motionRotationRecoilY,
                _motionRotationRecoilZ);
            vpfpweaponshooter.MotionPositionRecoilCameraFactor = _motionCameraPositionRecoil;
            vpfpweaponshooter.MotionRotationRecoilCameraFactor = _motionCameraRotationRecoil;
        }
        else
        {
            Debug.LogWarningFormat("Current selected weapon {0} cannot be casted to vp_FPWeaponShooter", WeaponHandler.CurrentShooter.name);
        }
        var vpfpreloader = WeaponHandler.CurrentShooter.GetComponent<vp_FPWeaponReloader>();
        if (null != vpfpreloader)
            vpfpreloader.ReloadDuration = _reloadDuration;
        else
        {
            Debug.LogWarningFormat("Current weapon reloader cannot be found on component {0}", WeaponHandler.CurrentShooter.name);
        }
    }

    void DisplaySavedPresets()
    {
        foreach (var presetName in _savedPresetNames)
            if (GUILayout.Button(presetName, GUILayout.Width(50), GUILayout.Height(50)))
                Load(presetName);
    }

    void FPControllerGUI()
    {
        HorizontalTitle("Motor");

        _fpControllerAcceleration = GUIValuesHelper.FloatField(new GUIContent("Acceleration"), _fpControllerAcceleration, GUILayout.Width(MinWidthValue));
        _fpControllerDamping = GUIValuesHelper.FloatField(new GUIContent("Damping"), _fpControllerDamping, GUILayout.Width(MinWidthValue));
        _fpControllerBackwardSpeed = GUIValuesHelper.FloatField(new GUIContent("BackwardSpeed"), _fpControllerBackwardSpeed, GUILayout.Width(MinWidthValue));
        _fpControllerAirSpeed = GUIValuesHelper.FloatField(new GUIContent("AirSpeed"), _fpControllerAirSpeed, GUILayout.Width(MinWidthValue));
    }

    void FPInputMobileGUI()
    {
        HorizontalTitle("Touch look");

        _sensitivityX = GUIValuesHelper.FloatField(new GUIContent("SensitivityX"), _sensitivityX, GUILayout.Width(MinWidthValue));
        _sensitivityY = GUIValuesHelper.FloatField(new GUIContent("SensitivityY"), _sensitivityY, GUILayout.Width(MinWidthValue));
        _smoothStep = GUIValuesHelper.FloatField(new GUIContent("SmoothStep"), _smoothStep, GUILayout.Width(MinWidthValue));
        _smoothWeight = GUIValuesHelper.FloatField(new GUIContent("SmoothWeight"), _smoothWeight, GUILayout.Width(MinWidthValue));
    }

    void FPWeaponShooterGUI()
    {
        HorizontalTitle("Projectile");

        _projectileFiringRate = GUIValuesHelper.FloatField(new GUIContent("ProjectileFiringRate"), _projectileFiringRate, GUILayout.Width(MinWidthValue));
        _projectileTapFiringRate = GUIValuesHelper.FloatField(new GUIContent("ProjectileTapFiringRate"), _projectileTapFiringRate, GUILayout.Width(MinWidthValue));
        _projectileSpread = GUIValuesHelper.FloatField(new GUIContent("ProjectileSpread"), _projectileSpread, GUILayout.Width(MinWidthValue));
        _projectileMaxSpreadWhenMoving = GUIValuesHelper.FloatField(new GUIContent("ProjectileMaxSpreadWhenMoving"), _projectileMaxSpreadWhenMoving, GUILayout.Width(MinWidthValue));
        _projectileMaxSpreadWhenRotate = GUIValuesHelper.FloatField(new GUIContent("ProjectileMaxSpreadWhenRotate"), _projectileMaxSpreadWhenRotate, GUILayout.Width(MinWidthValue));
        _projectileMaxSpreadRof = GUIValuesHelper.FloatField(new GUIContent("ProjectileMaxSpreadRof"), _projectileMaxSpreadRof, GUILayout.Width(MinWidthValue));

        _motionPositionRecoilX = GUIValuesHelper.FloatField(new GUIContent("MotionPositionRecoilX"), _motionPositionRecoilX, GUILayout.Width(MinWidthValue));
        _motionPositionRecoilY = GUIValuesHelper.FloatField(new GUIContent("MotionPositionRecoilY"), _motionPositionRecoilY, GUILayout.Width(MinWidthValue));
        _motionPositionRecoilZ = GUIValuesHelper.FloatField(new GUIContent("MotionPositionRecoilZ"), _motionPositionRecoilZ, GUILayout.Width(MinWidthValue));
        _motionRotationRecoilX = GUIValuesHelper.FloatField(new GUIContent("MotionRotationRecoilX"), _motionRotationRecoilX, GUILayout.Width(MinWidthValue));
        _motionRotationRecoilY = GUIValuesHelper.FloatField(new GUIContent("MotionRotationRecoilY"), _motionRotationRecoilY, GUILayout.Width(MinWidthValue));
        _motionRotationRecoilZ = GUIValuesHelper.FloatField(new GUIContent("MotionRotationRecoilZ"), _motionRotationRecoilZ, GUILayout.Width(MinWidthValue));
        _motionCameraPositionRecoil = GUIValuesHelper.FloatField(new GUIContent("MotionCameraPositionRecoil"), _motionCameraPositionRecoil, GUILayout.Width(MinWidthValue));
        _motionCameraRotationRecoil = GUIValuesHelper.FloatField(new GUIContent("MotionCameraRotationRecoil"), _motionCameraRotationRecoil, GUILayout.Width(MinWidthValue));
    }

    void FPWeaponReloaderGUI()
    {
        HorizontalTitle("Reload");

        _reloadDuration = GUIValuesHelper.FloatField(new GUIContent("ReloadDuration"), _reloadDuration, GUILayout.Width(MinWidthValue));
    }

    void HorizontalTitle(string title)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(title);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    void Save(string presetName)
    {
        int presetNumber = 0;
        while (PlayerPrefs.HasKey("TuningPreset" + presetNumber) &&
               PlayerPrefs.GetString("TuningPreset" + presetNumber) != presetName)
                presetNumber++;

        Debug.LogFormat("Saving preset with name {0} and number {1}", presetName, presetNumber);

        PlayerPrefs.SetString("TuningPreset" + presetNumber, presetName);

        PlayerPrefs.SetFloat(presetName + "_fpControllerAcceleration", _fpControllerAcceleration);
        PlayerPrefs.SetFloat(presetName + "_fpControllerDamping", _fpControllerDamping);
        PlayerPrefs.SetFloat(presetName + "_fpControllerBackwardSpeed", _fpControllerBackwardSpeed);
        PlayerPrefs.SetFloat(presetName + "_fpControllerAirSpeed", _fpControllerAirSpeed);

        PlayerPrefs.SetFloat(presetName + "_sensitivityX", _sensitivityX);
        PlayerPrefs.SetFloat(presetName + "_sensitivityY", _sensitivityY);
        PlayerPrefs.SetFloat(presetName + "_smoothStep", _smoothStep);
        PlayerPrefs.SetFloat(presetName + "_smoothWeight", _smoothWeight);

        PlayerPrefs.SetFloat(presetName + "_projectileFiringRate", _projectileFiringRate);
        PlayerPrefs.SetFloat(presetName + "_projectileTapFiringRate", _projectileTapFiringRate);
        PlayerPrefs.SetFloat(presetName + "_projectileSpread", _projectileSpread);
        PlayerPrefs.SetFloat(presetName + "_projectileMaxSpreadWhenMoving", _projectileMaxSpreadWhenMoving);
        PlayerPrefs.SetFloat(presetName + "_projectileMaxSpreadWhenRotate", _projectileMaxSpreadWhenRotate);
        PlayerPrefs.SetFloat(presetName + "_projectileMaxSpreadRof", _projectileMaxSpreadRof);

        PlayerPrefs.SetFloat(presetName + "_motionPositionRecoilX", _motionPositionRecoilX);
        PlayerPrefs.SetFloat(presetName + "_motionPositionRecoilY", _motionPositionRecoilY);
        PlayerPrefs.SetFloat(presetName + "_motionPositionRecoilZ", _motionPositionRecoilZ);
        PlayerPrefs.SetFloat(presetName + "_motionRotationRecoilX", _motionRotationRecoilX);
        PlayerPrefs.SetFloat(presetName + "_motionRotationRecoilY", _motionRotationRecoilY);
        PlayerPrefs.SetFloat(presetName + "_motionRotationRecoilZ", _motionRotationRecoilZ);
        PlayerPrefs.SetFloat(presetName + "_motionCameraPositionRecoil", _motionCameraPositionRecoil);
        PlayerPrefs.SetFloat(presetName + "_motionCameraRotationRecoil", _motionCameraRotationRecoil);

        PlayerPrefs.SetFloat(presetName + "_reloadDuration", _reloadDuration);

        PlayerPrefs.Save();
        CheckExistingPresets();
    }

    void Load(string presetName)
    {
        Debug.Log("Loading preset " + presetName);

        _presetName = presetName;

        _fpControllerAcceleration = PlayerPrefs.GetFloat(presetName + "_fpControllerAcceleration");
        _fpControllerDamping = PlayerPrefs.GetFloat(presetName + "_fpControllerDamping");
        _fpControllerBackwardSpeed = PlayerPrefs.GetFloat(presetName + "_fpControllerBackwardSpeed");
        _fpControllerAirSpeed = PlayerPrefs.GetFloat(presetName + "_fpControllerAirSpeed");

        _sensitivityX = PlayerPrefs.GetFloat(presetName + "_sensitivityX");
        _sensitivityY = PlayerPrefs.GetFloat(presetName + "_sensitivityY");
        _smoothStep = PlayerPrefs.GetFloat(presetName + "_smoothStep");
        _smoothWeight = PlayerPrefs.GetFloat(presetName + "_smoothWeight");

        _projectileFiringRate = PlayerPrefs.GetFloat(presetName + "_projectileFiringRate");
        _projectileTapFiringRate = PlayerPrefs.GetFloat(presetName + "_projectileTapFiringRate");
        _projectileSpread = PlayerPrefs.GetFloat(presetName + "_projectileSpread");
        _projectileMaxSpreadWhenMoving = PlayerPrefs.GetFloat(presetName + "_projectileMaxSpreadWhenMoving");
        _projectileMaxSpreadWhenRotate = PlayerPrefs.GetFloat(presetName + "_projectileMaxSpreadWhenRotate");
        _projectileMaxSpreadRof = PlayerPrefs.GetFloat(presetName + "_projectileMaxSpreadRof");
        
        _motionPositionRecoilX = PlayerPrefs.GetFloat(presetName + "_motionPositionRecoilX");
        _motionPositionRecoilY = PlayerPrefs.GetFloat(presetName + "_motionPositionRecoilY");
        _motionPositionRecoilZ = PlayerPrefs.GetFloat(presetName + "_motionPositionRecoilZ");
        _motionRotationRecoilX = PlayerPrefs.GetFloat(presetName + "_motionRotationRecoilX");
        _motionRotationRecoilY = PlayerPrefs.GetFloat(presetName + "_motionRotationRecoilY");
        _motionRotationRecoilZ = PlayerPrefs.GetFloat(presetName + "_motionRotationRecoilZ");
        _motionCameraPositionRecoil = PlayerPrefs.GetFloat(presetName + "_motionCameraPositionRecoil");
        _motionCameraRotationRecoil = PlayerPrefs.GetFloat(presetName + "_motionCameraRotationRecoil");
        
        _reloadDuration = PlayerPrefs.GetFloat(presetName + "_reloadDuration");
    }
}
