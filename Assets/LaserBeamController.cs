using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeamController : MonoBehaviour
{
    [SerializeField] float lifeTime = 2f;
    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    public void ActivateBeam()
    {
        StartCoroutine(ActivateBeamRoutine());
    }

    private IEnumerator ActivateBeamRoutine()
    {
        while (true)
        {
            yield return StartCoroutine(AnimateBeamCoroutine());
        }
    }

    private IEnumerator AnimateBeamCoroutine()
    {
        Vector3 beamOrigin = transform.position;
        Vector3 beamStart = GetRandomPointOnCube();
        Vector3 beamEnd = GetRandomPointOnCube();
        
        Vector3 direction = (beamEnd - transform.position).normalized;
        Vector3 outsidePoint = transform.position + direction * 2f;
        
        float startTime = Time.time;
        
        while (Time.time < startTime + Random.Range(lifeTime, lifeTime * 3))
        {
            float t = (Time.time - startTime) / lifeTime;
            _lineRenderer.SetPosition(0, beamOrigin);
            // _lineRenderer.SetPosition(1, Vector3.Lerp(beamStart, beamEnd, t));
            Vector3 tDirection = Vector3.Lerp(beamStart, beamEnd, t) - outsidePoint;
            Vector3 cubePointOnT = Physics.Raycast(outsidePoint, tDirection, out RaycastHit hit, 2f) ? hit.point : Vector3.zero;
            // Debug.DrawRay(outsidePoint, tDirection, Color.blue, 0.1f);
            _lineRenderer.SetPosition(1, cubePointOnT);
            yield return null;
        }
    }

    private Vector3 GetRandomPointOnCube()
    {
        Vector3 center = transform.position;

        Vector3 randomPointOutsideCubeInSphericalCoordinates = Random.insideUnitSphere * transform.localScale.x * 2f;
    
        // Cast a ray from the surface point in a random direction
        Vector3 direction = center - randomPointOutsideCubeInSphericalCoordinates;
        Ray ray = new Ray(randomPointOutsideCubeInSphericalCoordinates, direction);
    
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            // Get the point on the surface of the cube that was hit
            Vector3 pointOnCube = hit.point;
            
            // Debug.DrawRay(randomPointOutsideCubeInSphericalCoordinates, direction, Color.red, lifeTime);
            return pointOnCube;
        }

        return hit.point;
    }
}
