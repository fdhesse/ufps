using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class MainUI : vp_UIControl
{

    Transform attackButton;

    private Toggle autoShoot;
    private Toggle delayShoot;
    private Toggle rightMode;

	// Use this for initialization
	protected override void Start () {
        base.Start();

        attackButton = transform.FindChild("MainPanel/AnchorBottomRight/AttackButton");

        var buttons = transform.FindChild("Canvas/Options");
        if (buttons)
        {
            var button = buttons.GetChild(0);
            autoShoot = button.GetComponent<Toggle>();

            button = buttons.GetChild(1);
            delayShoot = button.GetComponent<Toggle>();

            button = buttons.GetChild(2);
            rightMode = button.GetComponent<Toggle>();

            autoShoot.onValueChanged.AddListener(delegate(bool enabled)
            {
                //if (m_Character)
                //  m_Character.GetComponent<CustomSettings>().AutoShoot = enabled;
                attackButton.gameObject.SetActive(!enabled);
                vp_GlobalEvent<bool>.Send("AutoShoot", enabled);
            });

            delayShoot.onValueChanged.AddListener(delegate(bool enabled)
            {
                vp_GlobalEvent<bool>.Send("DelayShoot", enabled);
            });
        }
	}
	
	// Update is called once per frame
    protected override void Update()
    {
        base.Update();
	}

    protected override void TouchesBegan(vp_Touch touch)
    {
        base.TouchesBegan(touch);

        if (rightMode && rightMode.isOn && touch.Position.x > Screen.width / 2 && attackButton)
            attackButton.position = m_Camera.ScreenToWorldPoint(touch.Position);
    }
}
