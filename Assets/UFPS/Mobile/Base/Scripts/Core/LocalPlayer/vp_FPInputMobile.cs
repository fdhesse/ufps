/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPInputMobile.cs
//	© Opsive. All Rights Reserved.
//	https://twitter.com/Opsive
//	http://www.opsive.com
//
//	description:	This extends vp_FPInput in order to override the GetButton
//					methods and allow support for touch controls for vp_UI system
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

public class vp_FPInputMobile : vp_FPInput
{
    private bool delayShoot;
    private bool autoShoot;
    private float delayTime = 0;

	protected vp_FPCamera m_FPCamera = null;
	public vp_FPCamera FPCamera
	{
		get
		{
			if (m_FPCamera == null)
				m_FPCamera = transform.root.GetComponentInChildren<vp_FPCamera>();
			return m_FPCamera;
		}
	}


    protected override void OnEnable()
    {
        base.OnEnable();
        vp_GlobalEvent<bool>.Register("DelayShoot", DelayShoot);
        vp_GlobalEvent<bool>.Register("AutoShoot", AutoShoot);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        vp_GlobalEvent<bool>.Unregister("DelayShoot", DelayShoot);
        vp_GlobalEvent<bool>.Unregister("AutoShoot", AutoShoot);
    }

    private void AutoShoot(bool auto)
    {
        autoShoot = auto;
    }

    private void DelayShoot(bool delay)
    {
        delayShoot = delay;
    }

	/// <summary>
	/// 
	/// </summary>
	protected override void Start()
	{
		
		FPCamera.SetRotation(FPCamera.Transform.eulerAngles, false, true);
		FPPlayer.Zoom.MinPause = .25f;
		FPPlayer.Zoom.MinDuration = .25f;
		
		MouseCursorBlocksMouseLook = false;

	}


	/// <summary>
	/// Handles interaction with the game world
	/// </summary>
	protected override void InputInteract()
	{
	
		if(!FPPlayer.CanInteract.Get())
			return;
			
		bool interact = vp_GlobalEventReturn<bool>.Send("SimulateTouchWithMouse") ? vp_Input.GetButtonAny("Interact") : vp_Input.GetButtonUp("Interact");
		
		if(interact /*|| UnityEngine.Input.GetButtonDown("Interact")*/)
			FPPlayer.Interact.TryStart();
		else
			FPPlayer.Interact.TryStop();
	
	}
	
	
	/// <summary>
	/// broadcasts a message to any listening components telling
	/// them to go into 'attack' mode. vp_FPShooter uses this to
	/// repeatedly fire the current weapon while the fire button
	/// is being pressed, but it could also be used by, for example,
	/// an animation script to make the player model loop an 'attack
	/// stance' animation.
	/// </summary>
	protected override void InputAttack()
	{

		// TIP: uncomment this to prevent player from attacking while running
		//if (FPPlayer.Run.Active)
		//	return;

		// if mouse cursor is visible, an extra click is needed
		// before we can attack
		
		if(FPPlayer.Reload.Active)
			return;

        if (autoShoot)
            return;

        if (vp_Input.GetButtonAny("Attack") || vp_Input.GetAxisRaw("RightTrigger") > 0.5f		// fire using the right gamepad trigger
            )
        {
            delayTime += Time.deltaTime;
            if (!delayShoot || delayTime > 1)
                FPPlayer.Attack.TryStart();
        }
        else
        {
            FPPlayer.Attack.TryStop();
            delayTime = 0;
        }
			
	}
	
	
	/// <summary>
	/// ask controller to jump when button is pressed (the current
	/// controller preset determines jump force).
	/// NOTE: if its 'MotorJumpForceHold' is non-zero, this
	/// also makes the controller accumulate jump force until
	/// button release.
	/// </summary>
	protected override void InputJump()
	{

		// TIP: to find out what determines if 'Jump.TryStart'
		// succeeds and where it is hooked up, search the project
		// for 'CanStart_Jump'

		if (vp_Input.GetButtonAny("Jump") || UnityEngine.Input.GetButton("Jump"))
			FPPlayer.Jump.TryStart();
		else
			FPPlayer.Jump.Stop();

	}
	
	
	/// <summary>
	/// when the reload button is pressed, broadcasts a message
	/// to any listening components asking them to reload
	/// NOTE: reload may not succeed due to ammo status etc.
	/// </summary>
	protected override void InputReload()
	{

		if (vp_Input.GetButtonAny("Reload")|| UnityEngine.Input.GetButton("Fire3"))
			FPPlayer.Reload.TryStart();
		
	}
	
	
	/// <summary>
	/// disallow zoom if these conditions are met
	/// </summary>
	protected virtual bool CanStart_Zoom()
	{
	
		if(!FPPlayer.CurrentWeaponWielded.Get())
			return false;
			
		if(FPPlayer.Reload.Active)
			return false;
		
		// we can only zoom with weapons that carry ammo
		if(FPPlayer.CurrentWeaponMaxAmmoCount.Get() == 0)
			return false;
			
		return true;
	
	}
	
	
	/// <summary>
	/// stop zooming if reload starts
	/// </summary>
	protected virtual void OnStart_Reload()
	{
	
		FPPlayer.Zoom.Stop();
	
	}
	
	
	/// <summary>
	/// zoom in using the zoom modifier key(s)
	/// </summary>
	protected override void InputCamera()
	{
	
		bool zoom = vp_GlobalEventReturn<bool>.Send("SimulateTouchWithMouse") ? vp_Input.GetButtonAny("Zoom") : vp_Input.GetButtonDown("Zoom");
		
		if(!zoom)
			return;
			
		if(Time.time < FPPlayer.Zoom.NextAllowedStartTime)
			return;
		
		if(FPPlayer.Zoom.Active)
			FPPlayer.Zoom.TryStop();
		else
			FPPlayer.Zoom.TryStart();

	}
	
	
	/// <summary>
	/// tell the player to enable or disable the 'Run' state.
	/// NOTE: since running is a state, it's not sent to the
	/// controller code (which doesn't know the state names).
	/// instead, the player class is responsible for feeding the
	/// 'Run' state to every affected component.
	/// </summary>
	protected override void InputRun()
	{

		if (vp_Input.GetButtonAny("Run") || vp_Input.GetAxisRaw("LeftTrigger") > 0.5f		// sprint using the left gamepad trigger
            )

        {
			if(vp_GlobalEventReturn<bool>.Send("SimulateTouchWithMouse"))
				FPPlayer.InputMoveVector.Set(new Vector2(0, 1));
			else
				FPPlayer.InputMoveVector.Set(new Vector2(vp_Input.GetAxisRaw("Horizontal"), vp_Input.GetAxisRaw("Vertical")));
			FPPlayer.Run.TryStart();
		}
		else FPPlayer.Run.TryStop();

	}
	
	
	/// <summary>
	/// toggles the game's pause state on / off
	/// </summary>
	protected override void UpdatePause()
	{

		if(vp_Input.GetButtonAny("Pause"))
			FPPlayer.Pause.Set(!FPPlayer.Pause.Get());

	}
	
	
	/// <summary>
	/// this method handles toggling between mouse pointer and
	/// firing modes. it can be used to deal with screen regions
	/// for button menus, inventory panels et cetera.
	/// NOTE: if your game supports multiple screen resolutions,
	/// make sure your 'MouseCursorZones' are always adapted to
	/// the current resolution. see 'vp_FPSDemo1.Start' for one
	/// example of how to this
	/// </summary>
	protected override void UpdateCursorLock()
	{

		if (vp_GlobalEventReturn<bool>.Send("SimulateTouchWithMouse"))
			return;

		// store the current mouse position as GUI coordinates
		m_MousePos.x = Input.mousePosition.x;
		m_MousePos.y = (Screen.height - Input.mousePosition.y);

		// uncomment this line to print the current mouse position
		//Debug.Log("X: " + (int)m_MousePos.x + ", Y:" + (int)m_MousePos.y);

		// if 'ForceCursor' is active, the cursor will always be visible
		// across the whole screen and firing will be disabled
		if (MouseCursorForced)
		{
			if (vp_Utility.LockCursor)
				vp_Utility.LockCursor = false;
			return;
		}

		// see if any of the mouse buttons are being held down
		if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
		{

			// if we have defined mouse cursor zones, check to see if the
			// mouse cursor is inside any of them
			if (MouseCursorZones.Length > 0)
			{
				foreach (Rect r in MouseCursorZones)
				{
					if (r.Contains(m_MousePos))
					{
						// mouse is being held down inside a mouse cursor zone, so make
						// sure the cursor is not locked and don't lock it this frame
						if (vp_Utility.LockCursor)
							vp_Utility.LockCursor = false;
						goto DontLock;
					}
				}
			}

			// no zones prevent firing the current weapon. hide mouse cursor
			// and lock it at the center of the screen
			if (!vp_Utility.LockCursor)
				vp_Utility.LockCursor = true;

		}

	DontLock:

		// if user presses 'ENTER', toggle mouse cursor on / off
		if (vp_Input.GetButtonUp("Accept1")
			|| vp_Input.GetButtonUp("Accept2")
			|| vp_Input.GetButtonUp("Menu")
			)
		{
#if UNITY_EDITOR && UNITY_5
			if (Input.GetKeyUp(KeyCode.Escape))
				vp_Utility.LockCursor = false;
			else
#endif
				vp_Utility.LockCursor = !vp_Utility.LockCursor;
		}
	}
	
}

