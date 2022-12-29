using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PointAttach : MonoBehaviour
{
    // A ray will be emitted out from the z direction of the pointDirection object
    public Transform pointDirection;
    public Transform cameraTransform;
    public GameObject particles;
    public Material rayMaterial;
    public float rayDistance = 10f;
    public float rayRadius = 0.5f;
    public float forceSensitivity = 1.25f;

    public enum Hand
    {
        Left,
        right
    }
    public Hand HandMode
    {
        get { return hand; }
        set
        {
            hand = value;
        }
    }
    [SerializeField]
    private Hand hand;

    public enum Mode
    {
        OutlineAll,
        OutlineVisible,
        OutlineHidden,
        OutlineAndSilhouette,
        SilhouetteOnly
    }
    public Mode OutlineMode
    {
        get { return outlineMode; }
        set
        {
            outlineMode = value;
        }
    }
    [SerializeField]
    private Mode outlineMode;

    public Color outlineColor = Color.white;
    public float outlineWidth = 10f;

    private LineRenderer line;

    private Vector3 handAngVelocity;

    private bool isPressingGrip = false;

    private GameObject buffer;
    private GameObject currentObj;
    private GameObject prevObj;

    private void Start()
    {
        line = this.gameObject.AddComponent<LineRenderer>();
        line.material = rayMaterial;
        line.startWidth = 0.02f;
        line.endWidth = 0.02f;
    }

    void Update()
    {
        if(hand == Hand.right)
        {
            handAngVelocity = SteamVR_Actions.default_Pose[SteamVR_Input_Sources.RightHand].angularVelocity;
            isPressingGrip = SteamVR_Actions.default_GrabGrip[SteamVR_Input_Sources.RightHand].state;
            
            if (handAngVelocity.x > forceSensitivity || handAngVelocity.x < -forceSensitivity)
            {
                this.GetComponent<GravityGloves>().grab();

                if (hand == Hand.right)
                {
                    SteamVR_Actions.default_Haptic[SteamVR_Input_Sources.RightHand].Execute(0f, 1f, 150f, 0.5f);
                }
                else
                {
                    SteamVR_Actions.default_Haptic[SteamVR_Input_Sources.LeftHand].Execute(0f, 1f, 150f, 0.5f);
                }
            }
        }
        else
        {
            handAngVelocity = SteamVR_Actions.default_Pose[SteamVR_Input_Sources.LeftHand].angularVelocity;
            isPressingGrip = SteamVR_Actions.default_GrabGrip[SteamVR_Input_Sources.LeftHand].state;
            
            if (handAngVelocity.x > forceSensitivity || handAngVelocity.x < -forceSensitivity)
            {
                this.GetComponent<GravityGloves>().grab();

                if (hand == Hand.right)
                {
                    SteamVR_Actions.default_Haptic[SteamVR_Input_Sources.RightHand].Execute(0f, 1f, 150f, 0.5f);
                }
                else
                {
                    SteamVR_Actions.default_Haptic[SteamVR_Input_Sources.LeftHand].Execute(0f, 1f, 150f, 0.5f);
                }
            }
        }

        RaycastHit hit;
        Ray detectRay = new Ray(pointDirection.position, pointDirection.transform.forward);
        Debug.DrawRay(pointDirection.position, pointDirection.forward * rayDistance, Color.red);
        
        line.SetPosition(0, pointDirection.transform.position);

        prevObj = currentObj;

        if (Physics.Raycast(detectRay, out hit, rayDistance))
        {
            line.enabled = true;
            line.SetPosition(1, hit.point);
            if (hit.collider.tag == "target")
            {
                hit.collider.gameObject.AddComponent<Outline>();
                hit.collider.gameObject.GetComponent<Outline>().OutlineMode = (Outline.Mode)outlineMode;
                hit.collider.gameObject.GetComponent<Outline>().OutlineWidth = outlineWidth;
                hit.collider.gameObject.GetComponent<Outline>().OutlineColor = outlineColor;
                currentObj = hit.collider.gameObject;

                particles.SetActive(true);
                particles.transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);

                if (isPressingGrip)
                {
                    this.GetComponent<GravityGloves>().target = hit.collider.gameObject;
                    buffer = hit.collider.gameObject;

                    if(buffer == null)
                    {
                        buffer.GetComponent<GravityGloves>().target.AddComponent<Outline>();

                        buffer.GetComponent<GravityGloves>().target.GetComponent<Outline>().OutlineMode = (Outline.Mode)outlineMode;
                        buffer.GetComponent<GravityGloves>().target.GetComponent<Outline>().OutlineWidth = outlineWidth;
                        buffer.GetComponent<GravityGloves>().target.GetComponent<Outline>().OutlineColor = outlineColor;
                    }
                }
                else
                    buffer = null;                
            }
            else
            {
                particles.SetActive(false);
                this.GetComponent<GravityGloves>().target = null;
                currentObj = null;
            }
        }
        else
        {
            line.enabled = false;
            particles.SetActive(false);
            this.GetComponent<GravityGloves>().target = null;
            currentObj = null;
        }
   
       if (prevObj != currentObj && !isPressingGrip)
            Destroy(prevObj.GetComponent<Outline>());

       if (isPressingGrip)
       {
            this.GetComponent<GravityGloves>().target = buffer;
            buffer.GetComponent<GravityGloves>().target.AddComponent<Outline>();
            buffer.GetComponent<GravityGloves>().target.GetComponent<Outline>().OutlineMode = (Outline.Mode)outlineMode;
            buffer.GetComponent<GravityGloves>().target.GetComponent<Outline>().OutlineWidth = outlineWidth;
            buffer.GetComponent<GravityGloves>().target.GetComponent<Outline>().OutlineColor = outlineColor;
       }
       else
       {
            Destroy(buffer.GetComponent<Outline>());
            buffer = null;
       }
    }
}