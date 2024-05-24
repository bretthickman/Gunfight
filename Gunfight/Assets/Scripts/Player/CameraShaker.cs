using System.Collections;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    private Transform cameraTransform;

    private GameObject target;

    public float shakeDuration = 0.1f;

    void Start()
    {
        target = GameObject.Find("LocalGamePlayer");
        cameraTransform = GetComponent<Transform>();
    }

    public void ShootCameraShake(float velocity)
    {
        float shakeIntensity = Mathf.Clamp01(velocity / 20f);
        StartCoroutine(ShootShake(shakeDuration,
        target.GetComponent<PlayerController>().weaponInfo.range / 15.0f * shakeIntensity));
    }

    public void HurtCameraShake(float velocity)
    {
        float shakeIntensity = Mathf.Clamp01(velocity / 20f);
        StartCoroutine(HurtShake(shakeDuration, 0.35f * shakeIntensity));
    }

    public void SteleCameraShake(float velocity)
    {
        float shakeIntensity = Mathf.Clamp01(velocity / 20f);
        StartCoroutine(SteleShake(shakeDuration, 0.35f * shakeIntensity));
    }

    private IEnumerator ShootShake(float duration, float magnitude)
    {
        Vector3 originalPosition = cameraTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // float x = Random.Range(-1f, 1f) * magnitude;
            // float y = Random.Range(-1f, 1f) * magnitude;
            float x =
                (-target.transform.up.x + Random.Range(-0.25f, 0.25f)) *
                magnitude;
            float y =
                (-target.transform.up.y + Random.Range(-0.25f, 0.25f)) *
                magnitude;

            cameraTransform.localPosition =
                new Vector3(x, y, cameraTransform.localPosition.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        cameraTransform.localPosition = originalPosition;
    }

    private IEnumerator HurtShake(float duration, float magnitude)
    {
        Vector3 originalPosition = cameraTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            cameraTransform.localPosition =
                new Vector3(x, y, cameraTransform.localPosition.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        cameraTransform.localPosition = originalPosition;
    }

    private IEnumerator SteleShake(float duration, float magnitude)
    {
        Vector3 originalPosition = cameraTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            cameraTransform.localPosition =
                new Vector3(x, y, cameraTransform.localPosition.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        cameraTransform.localPosition = originalPosition;
    }
}
