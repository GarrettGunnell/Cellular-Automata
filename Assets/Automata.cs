using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Automata : MonoBehaviour {
    
    public ComputeShader automataCompute;
    public bool randomSeed = false;

    private RenderTexture target;
    private int kernel, threadGroupsX, threadGroupsY, generation;
    
    void Awake() {
        threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        generation = Screen.height - 2;
    }

    void OnEnable() {
        if (target == null) {
            target = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create();

            automataCompute.SetInt("_Width", Screen.width);
            automataCompute.SetInt("_Height", Screen.height);
        }

        automataCompute.SetTexture(0, "_Result", target);
        automataCompute.SetInt("_Generation", generation);
        automataCompute.SetInt("_RandomSeed", randomSeed ? 1 : 0);
        automataCompute.Dispatch(0, threadGroupsX, 1, 1);
    }

    void Update() {
        automataCompute.SetTexture(1, "_Result", target);
        automataCompute.SetInt("_Generation", generation);
        automataCompute.Dispatch(1, threadGroupsX, 1, 1);

        if (generation > 0)
            generation--;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(target, destination);
    }
}
