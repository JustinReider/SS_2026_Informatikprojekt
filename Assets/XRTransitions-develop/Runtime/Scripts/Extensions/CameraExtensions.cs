using System;
using UnityEngine;

namespace Scripts.Utils
{
    public static class CameraExtensions
    {
        public static Matrix4x4 CalculateStereoObliqueMatrix(this Camera camera, Camera.StereoscopicEye eye, Vector4 clipPlane)
        {
            Matrix4x4 projection = camera.GetStereoProjectionMatrix(eye);
            Matrix4x4 obliqueMatrix = projection;
            Vector4 q = projection.inverse * new Vector4(
                Math.Sign(clipPlane.x),
                Math.Sign(clipPlane.y),
                1.0f,
                1.0f
            );
            Vector4 c = clipPlane * (2.0F / (Vector4.Dot (clipPlane, q)));
            obliqueMatrix[2] = c.x - projection[3];
            obliqueMatrix[6] = c.y - projection[7];
            obliqueMatrix[10] = c.z - projection[11];
            obliqueMatrix[14] = c.w - projection[15];
            return obliqueMatrix;
        }
    }
}