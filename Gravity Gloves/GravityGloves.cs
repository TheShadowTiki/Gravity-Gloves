using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityGloves : MonoBehaviour
{
    //Object to be attracted
    public GameObject target; 

    //Vertical and horizontal components of vertex of trajectory. Relative to parent object.
    public Vector2 vertexOfTrajectory = new Vector2(); 

    //Gravitational constant of acceleration
    public float g = 9.8067f; 

    //Forward direction target aligns to
    public Vector3 orientation = new Vector3(); 

    //Enables sphere of influence (SOI) when true
    public bool sphereOfInfluence = false; 

    //Outer radius of sphere of influence
    public float outerRadius = 1.0f; 

    //Inner radius of sphere of influence
    public float innerRadius = 0.5f; 

    //Outer collider of sphere of influence. Visible in inspector when SOI is enabled
    [HideInInspector] public SphereCollider outer = new SphereCollider(); 

    //Inner collider of sphere of influence. Visible in inspector when SOI is enabled
    [HideInInspector] public SphereCollider inner = new SphereCollider(); 

    //The force which acts as the influence of the SOI
    [HideInInspector] public bool force = false; 

    //Triggers on grab
    [HideInInspector] public bool trigger = false; 

    // Start is called before the first frame update
    void Start()
    {
        setUpOrient();
        outer = null;
        inner = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            updateTrajectory();
        }
        updateSOI();
        updateSOIInfluence();
    }

    void setUpOrient()
    {
        GameObject orient_object = new GameObject();
        orient_object.transform.position = this.transform.position;
        orient_object.transform.forward = orientation;
        orient_object.transform.parent = this.transform;
        orient_object.tag = "orient";
    }

    void updateSOI()
    {
        if (sphereOfInfluence && outer == null && inner == null)
        {
            outer = this.gameObject.AddComponent<SphereCollider>();
            outer.isTrigger = true;
            inner = this.gameObject.AddComponent<SphereCollider>();
            inner.isTrigger = true;
        }
        else if (sphereOfInfluence)
        {
            if (outerRadius > 0 && innerRadius > 0)
            {
                if (innerRadius < outerRadius)
                {
                    inner.radius = innerRadius;
                    outer.radius = outerRadius;
                }
                else
                {
                    outerRadius = innerRadius;
                    innerRadius = outerRadius;
                }
            }
            else
            {
                if (outerRadius < 0)
                {
                    outerRadius = 0;
                }
                else
                {
                    innerRadius = 0;
                }
            }
        }
        else if (!sphereOfInfluence)
        {
            Destroy(outer);
            outer = null;
            Destroy(inner);
            inner = null;
        }
    }

    void updateTrajectory()
    {
        target.tag = "target";
        var targetRigid = target.GetComponent<Rigidbody>();
        var direction = this.transform.position - target.transform.position;
        var vot = vertexOfTrajectory;
        var vertex = this.transform.position + new Vector3(0, vot.y, 0) + Vector3.ProjectOnPlane(-direction, new Vector3(0, 1, 0)).normalized * vot.x;

        float t = Mathf.Sqrt(2 * (vertex.y - target.transform.position.y) / g);
        var xVector = Vector3.ProjectOnPlane(vertex - target.transform.position, new Vector3(0, 1, 0));
        float x = xVector.magnitude;

        Debug.DrawRay(target.transform.position, xVector, Color.yellow);
        var upAngle = Mathf.Atan((Mathf.Pow(t, 2) * g) / x);
        var initialVelocity = (t * g) / (Mathf.Sin(upAngle));
        var finalDirection = new Vector3();
        if (target.transform.position.y >= this.transform.position.y)
        {
            float steepAngle = Vector3.Angle(new Vector3(0, -1, 0), direction);
            if (steepAngle > 45)
            {
                finalDirection = Vector3.ProjectOnPlane(this.transform.position - target.transform.position, new Vector3(0, 1, 0)).normalized;
                initialVelocity = (Mathf.Sqrt(g) * (Vector3.ProjectOnPlane(this.transform.position - target.transform.position, new Vector3(0, 1, 0)).magnitude)) / (Mathf.Sqrt((target.transform.position.y - this.transform.position.y) * 2));
            }
            else
            {
                finalDirection = direction.normalized;
                initialVelocity = 1.5f * g;
            }
        }
        else
        {
            finalDirection = (xVector.normalized + new Vector3(0, Mathf.Tan(upAngle), 0)).normalized;
        }

        Debug.DrawRay(this.transform.position, vertex - this.transform.position, Color.red);
        Debug.DrawRay(target.transform.position, direction, Color.green);
        Debug.DrawRay(target.transform.position, finalDirection * initialVelocity, Color.blue);
        Debug.DrawRay(vertex, new Vector3(0, -1, 0) * (vertex.y - target.transform.position.y), Color.cyan);

        if (trigger == true)
        {
           targetRigid.AddForce(finalDirection * initialVelocity + -targetRigid.velocity, ForceMode.VelocityChange);
           trigger = false;
        }
    }

    void updateSOIInfluence()
    {
        if (force)
        {
            Transform orient_transform = null;
            foreach (Transform child in this.transform)
            {
                if (child.tag == "orient")
                {
                    orient_transform = child;
                }
            }
            var target_rigid = target.GetComponent<Rigidbody>();
            var direction = this.transform.position - target.transform.position;
            target_rigid.AddForce(direction);
            target_rigid.AddTorque(Vector3.Cross(orient_transform.forward, target.transform.forward));
        }
    }

    public void grab()
    {
        trigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        var targ_pos = other.gameObject.transform.position;
        var pos = this.gameObject.transform.position;
        if (other.gameObject.tag == "target" && Vector3.Distance(targ_pos, pos) >= outer.radius)
        {
            force = true;
        }
        else if (other.gameObject.tag == "target")
        {
            force = false;
        }
    }
}