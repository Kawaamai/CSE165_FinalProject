using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

    public Text movementText;
    public Text RGBTextR;
    public Text RGBTextG;
    public Text RGBTextB;
    public Text ResetText;
    public LineRenderer lightsaber;

	public Transform[] controllers; // only has the right hand

	// Use this for initialization
	void Start () {
        OutputLightsaberColor();	    	
        Debug.Log(lightsaber.colorGradient.colorKeys.Length);
	}
	
	// Update is called once per frame
	void Update () {

        if (OVRInput.Get(OVRInput.RawTouch.RIndexTrigger) == false) // pointing
        {
            // TODO: raycast
            // determine if hit any component
            // highlight
            // TODO: play with these offsets for the right index finger position
            Ray ray = new Ray(controllers[0].position + (controllers[0].up * .01f) + (controllers[0].right * .015f), controllers[0].forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 2f))
            {
                if (hit.collider.tag == "menu")
                {
                    Text t = hit.collider.gameObject.GetComponent<Text>();
                    highlightText(t);

                    if (OVRInput.GetDown(OVRInput.RawButton.RHandTrigger))
                    {
                        // TODO
                        if (hit.collider.gameObject.name == "MovementText")
                        {
                            if (GameManager.Inst.isHumanJoystick)
                            {
                                GameManager.Inst.isHumanJoystick = false;
                                t.text = "Movement:\nNormal";
                            }
                            else
                            {
                                GameManager.Inst.isHumanJoystick = true;
                                GameManager.Inst.SetBaseHeadPos();
                                t.text = "Movement:\nHuman Joystick";
                            }
                        }
                        else if (t.gameObject.name == "Reset")
                        {
                            // TODO:
                            Debug.Log("soemra;lkjsdf;lk");
                            GameManager.Inst.Reset();
                        }
                    }
                    if (t.gameObject.name == "RGBTextR" || t.gameObject.name == "RGBTextG" || t.gameObject.name == "RGBTextB")
                    {
                        Vector2 rtstick = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);
                        if (rtstick.y > 0)
                        {
                            // TODO: fix color. is not just start color but other oclor too. also sometimes turns clear
                            switch (t.gameObject.name)
                            {
                                case "RGBTextR":
                                    SetLightsaberColor(lightsaber.startColor + (new Color(.2f,0f,0f) * Time.deltaTime));
                                    break;
                                case "RGBTextG":
                                    SetLightsaberColor(lightsaber.startColor + (new Color(0f,.2f,0f) * Time.deltaTime));
                                    break;
                                case "RGBTextB":
                                    SetLightsaberColor(lightsaber.startColor + (new Color(0f,0f,.2f) * Time.deltaTime));
                                    break;
                            }
                        }
                        else if (rtstick.y < 0)
                        {
                            switch (t.gameObject.name)
                            {
                                case "RGBTextR":
                                    SetLightsaberColor(lightsaber.startColor - (new Color(.2f,0f,0f) * Time.deltaTime));
                                    break;
                                case "RGBTextG":
                                    SetLightsaberColor(lightsaber.startColor - (new Color(0f,.2f,0f) * Time.deltaTime));
                                    break;
                                case "RGBTextB":
                                    SetLightsaberColor(lightsaber.startColor - (new Color(0f,0f,.2f) * Time.deltaTime));
                                    break;
                            }
                        }
                        OutputLightsaberColor();
                    }
                    else
                    {
                        unhighlightText(RGBTextR);
                        unhighlightText(RGBTextG);
                        unhighlightText(RGBTextB);
                    }
                }
                else
                {
                    unhighlightText(movementText);
                    unhighlightText(ResetText);
                    unhighlightText(RGBTextR);
                    unhighlightText(RGBTextG);
                    unhighlightText(RGBTextB);
                }
            }
            else
            {
                unhighlightText(movementText);
                unhighlightText(ResetText);
                unhighlightText(RGBTextR);
                unhighlightText(RGBTextG);
                unhighlightText(RGBTextB);
            }
        }
	}

    private void highlightText(Text t)
    {
        switch (t.gameObject.name)
        {
            case "MovementText":
                t.color = Color.red;
                break;
            case "RGBTextR":
                t.color = Color.red;
                break;
            case "RGBTextG":
                t.color = Color.green;
                break;
            case "RGBTextB":
                t.color = Color.blue;
                break;
            case "Reset":
                t.color = Color.yellow;
                break;
        }
    }

    private void SetLightsaberColor(Color c)
    {
        float startalpha = lightsaber.startColor.a;
        float endalpha= lightsaber.endColor.a;
        lightsaber.startColor = new Color(c.r, c.g, c.b, startalpha);
        lightsaber.endColor = new Color(c.r, c.g, c.b, endalpha);
    }

    private void unhighlightText(Text t)
    {
        if (t.gameObject.name == "Reset")
            t.color = Color.white;
        else
            t.color = Color.black;
    }

    private void OutputLightsaberColor()
    {
        RGBTextR.text = "R:\n" + lightsaber.startColor.r;
        RGBTextG.text = "G:\n" + lightsaber.startColor.g;
        RGBTextB.text = "B:\n" + lightsaber.startColor.b;
    }
}
