using System.Collections;
using UnityEngine;
using StarterAssets;

public class ShrinkZone : MonoBehaviour
{
    [Header("Shrink-Einstellungen")]
    [Range(0.1f, 1f)]
    public float shrinkFactor = 0.5f;

    [Range(0.1f, 1f)]
    public float speedFactor = 0.5f;

    [Header("Transition")]
    public float transitionDuration = 1.0f;

    public string playerTag = "Player";

    private Vector3 _originalScale;
    private float   _originalMoveSpeed;
    private float   _originalSprintSpeed;

    private FirstPersonController _fpc;
    private Coroutine _activeCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        _fpc = other.GetComponent<FirstPersonController>();
        if (_fpc == null) return;

        _originalScale       = other.transform.localScale;
        _originalMoveSpeed   = _fpc.MoveSpeed;
        _originalSprintSpeed = _fpc.SprintSpeed;

        if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
        _activeCoroutine = StartCoroutine(TransitionTo(
            other.transform,
            _originalScale * shrinkFactor,
            _originalMoveSpeed   * speedFactor,
            _originalSprintSpeed * speedFactor
        ));
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (_fpc == null) return;

        if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
        _activeCoroutine = StartCoroutine(TransitionTo(
            other.transform,
            _originalScale,
            _originalMoveSpeed,
            _originalSprintSpeed
        ));
    }

    private IEnumerator TransitionTo(
        Transform playerTransform,
        Vector3 targetScale,
        float targetMoveSpeed,
        float targetSprintSpeed)
    {
        Vector3 startScale       = playerTransform.localScale;
        float   startMoveSpeed   = _fpc.MoveSpeed;
        float   startSprintSpeed = _fpc.SprintSpeed;

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);

            playerTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            _fpc.MoveSpeed             = Mathf.Lerp(startMoveSpeed,   targetMoveSpeed,   t);
            _fpc.SprintSpeed           = Mathf.Lerp(startSprintSpeed, targetSprintSpeed, t);

            yield return null;
        }

        // Zielwerte exakt setzen am Ende
        playerTransform.localScale = targetScale;
        _fpc.MoveSpeed             = targetMoveSpeed;
        _fpc.SprintSpeed           = targetSprintSpeed;
    }
}