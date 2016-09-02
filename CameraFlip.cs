using UnityEngine;
using System.Collections;

/// <summary>
/// This class creates a custom projection for the reflection camera so that the position of
/// the centre of projection can be controlled
/// </summary>
[ExecuteInEditMode]
public class CameraFlip : MonoBehaviour
{
    public Camera cam; // get the reflection camera
    public Transform mirrorplane;
    public Matrix4x4 originalProjection;

    // control the dimensions of the projection
    public float left = -0.7F;
    public float right = 0.7F;
    public float top = 0.7F;
    public float bottom = -0.7F;
    public float anOffset = -10F; // zooms the FOV in and out
    public float anOffset2 = 2F;


    void LateUpdate()
    {
        Matrix4x4 m = PerspectiveOffCenter(left, right, bottom, top, cam.nearClipPlane, cam.farClipPlane, anOffset2, anOffset);
        cam.projectionMatrix = m; // pass new projection matrix into reflection camera
    }
    
    // matrix code from Unity documentation example edited with variables to control code from the inspector
    static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far, float o2, float o)
    {
        float x = 2F * near / (right - left);
        float y = 2F * near / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(o2 * far * near) / (far - near);
        float e = o;
        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x;
        m[0, 1] = 0;
        m[0, 2] = a;
        m[0, 3] = 0;
        m[1, 0] = 0;
        m[1, 1] = y;
        m[1, 2] = b;
        m[1, 3] = 0;
        m[2, 0] = 0;
        m[2, 1] = 0;
        m[2, 2] = c;
        m[2, 3] = d;
        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = e;
        m[3, 3] = 0;
        return m;
    }
}
