using System.Collections;
using UnityEngine;

public class LaserBeamController : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    private Vector3 _beamStart;
    private Vector3 _beamEnd;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        
        _beamStart = GetRandomPointOnCube();
        _beamEnd = GetRandomPointOnCube();
    }

    public void ActivateBeam(float duration)
    {
        _lineRenderer.enabled = true;
        StartCoroutine(AnimateBeamCoroutine(duration));
    }

    private IEnumerator AnimateBeamCoroutine(float duration)
    {
        Vector3 transformPosition = transform.position;

        Vector3 direction = (_beamEnd - transformPosition).normalized;
        Vector3 outsidePoint = transformPosition + direction * 2f;
        
        float startTime = Time.time;
        
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            _lineRenderer.SetPosition(0, transformPosition);
            Vector3 tDirection = Vector3.Lerp(_beamStart, _beamEnd, t) - outsidePoint;
            Vector3 cubePointOnT = Physics.Raycast(outsidePoint, tDirection, out RaycastHit hit, 2f) ? hit.point : Vector3.zero;
            _lineRenderer.SetPosition(1, cubePointOnT);
            yield return null;
        }
    }

    private Vector3 GetRandomPointOnCube()
    {
        Vector3 center = transform.position;

        Vector3 randomPointOutsideCubeInSphericalCoordinates = Random.insideUnitSphere * (transform.localScale.x * 2f);
    
        // Cast a ray from the surface point in a random direction
        Vector3 direction = center - randomPointOutsideCubeInSphericalCoordinates;
        Ray ray = new Ray(randomPointOutsideCubeInSphericalCoordinates, direction);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f)) return hit.point;
        
        // Get the point on the surface of the cube that was hit
        Vector3 pointOnCube = hit.point;
            
        // Debug.DrawRay(randomPointOutsideCubeInSphericalCoordinates, direction, Color.red, lifeTime);
        return pointOnCube;

    }

    public void DeactivateBeam()
    {
        StopAllCoroutines();
        _lineRenderer.SetPosition(0, Vector3.zero);
        _lineRenderer.SetPosition(1, Vector3.zero);
        _lineRenderer.enabled = false;
    }
}
