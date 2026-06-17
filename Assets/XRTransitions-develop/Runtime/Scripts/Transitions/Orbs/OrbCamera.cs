using System;
using System.Collections;
using System.Collections.Generic;
using Scripts.Utils;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class OrbCamera : MonoBehaviour
{
    private bool _isInitialized = false;

    public float nearClipOffset = 0.05f;
    public float nearClipLimit = 0.2f;

    private TransitionManager _transitionManager;
    private Orb _orb;
    private Camera _camera;
    private Camera _mainCamera;
    private Camera.StereoscopicEye _eye;
    private Renderer _orbRenderer;
    private Transform _eyeTransform;
    private Transform _orbTransform;
    private Transform _destination;

    private static readonly int LeftRenderTexture = Shader.PropertyToID("_LeftEyeTexture");
    private static readonly int RightRenderTexture = Shader.PropertyToID("_RightEyeTexture");

    public void Initialize(Orb orb, OrbTransition transition, Camera.StereoscopicEye eye)
    {
        _transitionManager = FindObjectOfType<TransitionManager>();
        _orb = orb;
        _orbTransform = orb.transform;
        transform.parent = _orbTransform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        _destination = transition.Destination;

        _camera = GetComponent<Camera>();
        if (_camera == null)
        {
            _camera = gameObject.AddComponent<Camera>();
        }

        _mainCamera = _transitionManager.MainCamera;
        _camera.CopyFrom(_mainCamera);
        _camera.forceIntoRenderTexture = true;
        _camera.targetTexture = new RenderTexture(_mainCamera.pixelWidth, _mainCamera.pixelHeight, 24);
        _camera.aspect = _mainCamera.aspect;
        _camera.fieldOfView = _mainCamera.fieldOfView;
        _camera.projectionMatrix = _mainCamera.GetStereoProjectionMatrix(eye);
        _camera.nonJitteredProjectionMatrix = _mainCamera.GetStereoNonJitteredProjectionMatrix(eye);
        _camera.enabled = false;

        _eye = eye;
        _eyeTransform = _eye == Camera.StereoscopicEye.Left
            ? _transitionManager.LeftEyeTransform
            : _transitionManager.RightEyeTransform;
        _orbRenderer = orb.OrbRenderer;
        _orbRenderer.material.SetTexture(_eye == Camera.StereoscopicEye.Left ? LeftRenderTexture : RightRenderTexture,
            _camera.targetTexture);

        _isInitialized = true;
    }

    private void SetNearClipPlane()
    {
        // Learning resource:
        // http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
        int dot = Math.Sign(Vector3.Dot(_eyeTransform.forward, _destination.position - transform.position));

        Vector3 camSpacePos = _camera.worldToCameraMatrix.MultiplyPoint(_destination.position);
        Vector3 camSpaceNormal = _camera.worldToCameraMatrix.MultiplyVector(_eyeTransform.forward) * dot;
        float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + nearClipOffset;

        // Don't use oblique clip plane if very close to portal as it seems this can cause some visual artifacts
        if (Mathf.Abs(camSpaceDst) > nearClipLimit)
        {
            Vector4 clipPlaneCameraSpace =
                new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);
            _camera.projectionMatrix = _mainCamera.CalculateStereoObliqueMatrix(_eye, clipPlaneCameraSpace);
        }
        else
        {
            _camera.projectionMatrix = _mainCamera.GetStereoProjectionMatrix(_eye);
        }
    }

    private void OnEnable()
    {
        InputSystem.onAfterUpdate += RenderPortal;
    }

    private void OnDisable()
    {
        InputSystem.onAfterUpdate -= RenderPortal;
    }

    private void RenderPortal()
    {
        if (_isInitialized && InputState.currentUpdateType == InputUpdateType.BeforeRender && _orbRenderer.isVisible)
        {
            transform.position =
                _destination.transform.TransformPoint(_transitionManager.XROrigin.transform.InverseTransformPoint(_eyeTransform.position));
            transform.rotation = _destination.transform.TransformRotation(
                _transitionManager.XROrigin.transform.InverseTransformRotation(_eyeTransform.rotation));
            //SetNearClipPlane();
            _camera.Render();
        }
    }
}