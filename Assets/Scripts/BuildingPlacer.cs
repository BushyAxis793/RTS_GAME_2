using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BuildingPlacer : MonoBehaviour
{

    Vector3 startpoint;

    new Renderer renderer;

    RaycastHit rayHit;
    NavMeshHit navHit;
    NavMeshPath navPath;

    [SerializeField]
    Color enableColor = Color.blue, disableColor = Color.red;
    [SerializeField]
    Vector3 positionOffset, boxOffset, boxSize;
    [SerializeField]
    float maxNavMeshDistance = 1;
    [SerializeField]
    LayerMask boxLayerMask = 1;



    private void Awake()
    {
        startpoint = transform.position;
        renderer = GetComponentInChildren<Renderer>(true);
        navPath = new NavMeshPath();
    }

    private void Update()
    {
        renderer.sharedMaterial.color = CanBuildHere() ? enableColor : disableColor;
    }

    public bool CanBuildHere()
    {

        if (!Physics.CheckBox(transform.position + boxOffset, boxSize/2,transform.rotation,boxLayerMask))
        {

            if (NavMesh.SamplePosition(transform.position, out navHit, maxNavMeshDistance, NavMesh.AllAreas))
            {
                if (NavMesh.CalculatePath(startpoint, transform.position, NavMesh.AllAreas, navPath))
                {
                    for (int i = 0; i < navPath.corners.Length-1; i++)
                    {
                        Debug.DrawLine(navPath.corners[i], navPath.corners[i + 1], Color.cyan);
                    }
                    return navPath.status == NavMeshPathStatus.PathComplete;
                }

            }
        }



        return false;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position + positionOffset;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + boxOffset, boxSize);
    }
}
