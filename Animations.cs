using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Code to animate the meshes. Because unity doesn't allow vertex animation other than through blendshapes
/// the animation will be done through manipulating the blendshape range through code.
/// </summary>
public class Animations : MonoBehaviour
{
    
    public List<SkinnedMeshRenderer> _SkinnedMeshRends; // get the blendshapes
    List<int> blendsInSkin = new List<int>();
    List<int> AssBodyBlends = new List<int>();
    bool isSwitch = false;
    public int val, val2, val3, val4, val5, val6, val7, val8, val9; // values to animate between
    float valConv, valConv2, valConv3, valConv4, valConv5, valConv6, valConv7, valConv8, valConv9;
    Renderer assRend;
    string rendName = "_EMissionIntensity"; // name of shader property

    void Awake()
    {
        foreach (SkinnedMeshRenderer skin in _SkinnedMeshRends) // get the blendshapes
        {
            blendsInSkin.Add(skin.sharedMesh.blendShapeCount);
            assRend = _SkinnedMeshRends[0].gameObject.GetComponent<Renderer>();
        }
    }

    // Use leantween tweening engine to transition between two values with different smoothing curves applied
    void Start()
    {
        LeanTween.value(this.gameObject, 0f, 100f, 3f).setOnUpdate( (valConv1) => { val = (int)valConv1; } )
            .setLoopPingPong().setEase(LeanTweenType.easeInOutSine);
        LeanTween.value(this.gameObject, 50f, 70f, 5f).setOnUpdate((valConv2) => { val2 = (int)valConv2; })
            .setLoopPingPong().setEase(LeanTweenType.easeInOutSine);
        LeanTween.value(this.gameObject, 20f, 50f, 3f).setOnUpdate((valConv3) => { val3 = (int)valConv3; })
            .setLoopPingPong().setEase(LeanTweenType.easeInOutSine);
        LeanTween.value(this.gameObject, 10f, 70f, 3f).setOnUpdate((valConv4) => { val4 = (int)valConv4; })
            .setLoopPingPong().setEase(LeanTweenType.easeInOutSine);
        LeanTween.value(this.gameObject, 30f, 100f, 2f).setOnUpdate((valConv5) => { val5 = (int)valConv5; })
            .setLoopPingPong().setEase(LeanTweenType.easeInOutSine);
        LeanTween.value(this.gameObject, 0f, 40f, 2f).setOnUpdate((valConv6) => { val6 = (int)valConv6; })
            .setLoopPingPong().setEase(LeanTweenType.easeInOutSine);
        LeanTween.value(this.gameObject, 0f, 100f, 1.5f).setOnUpdate((valConv7) => { val7 = (int)valConv7; })
            .setLoopPingPong().setEase(LeanTweenType.easeOutQuart);
        LeanTween.value(this.gameObject, 0f, 100f, 1.5f).setOnUpdate((valConv8) => { val8 = (int)valConv8; })
            .setLoopPingPong().setEase(LeanTweenType.easeOutSine);
        LeanTween.value(this.gameObject, 0f, 100f, 1f).setOnUpdate((valConv9) => { val9 = (int)valConv9; })
            .setLoopPingPong().setEase(LeanTweenType.linear);

        StartCoroutine(animateBlendshapes());
    }

    

    IEnumerator animateBlendshapes() // funtion to control the different animations
    {        

        while (true)
        {
            // Ass Balloons
            for (int i = 0; i < blendsInSkin[0]; i++)
            {               
                _SkinnedMeshRends[0].SetBlendShapeWeight(i, val);
                assRend.material.SetFloat(rendName, val5 * 0.01f);
            }
            // Ass Head Hair
                _SkinnedMeshRends[1].SetBlendShapeWeight(0, val2);
                _SkinnedMeshRends[1].SetBlendShapeWeight(1, val3);
            // HardShell Body
                _SkinnedMeshRends[2].SetBlendShapeWeight(0, val6);
            // HighGrav Head
                _SkinnedMeshRends[4].SetBlendShapeWeight(0, val);
                _SkinnedMeshRends[4].SetBlendShapeWeight(1, val2);
            // HighGrav Arms
                _SkinnedMeshRends[5].SetBlendShapeWeight(0, val5);
                _SkinnedMeshRends[7].SetBlendShapeWeight(0, val5);
            // HighGrav Legs
                _SkinnedMeshRends[6].SetBlendShapeWeight(0, val5);
                _SkinnedMeshRends[8].SetBlendShapeWeight(0, val5);
            // Tendrils
                _SkinnedMeshRends[9].SetBlendShapeWeight(0, val8);
                _SkinnedMeshRends[10].SetBlendShapeWeight(0, val9);
                _SkinnedMeshRends[11].SetBlendShapeWeight(0, val8);
                _SkinnedMeshRends[12].SetBlendShapeWeight(0, val7);
                _SkinnedMeshRends[13].SetBlendShapeWeight(0, val8);
                _SkinnedMeshRends[14].SetBlendShapeWeight(0, val7);


            yield return new WaitForSeconds(0.01f);
        } 
    }
}
