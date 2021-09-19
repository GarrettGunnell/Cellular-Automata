using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Automata : MonoBehaviour {
    
    public ComputeShader automataCompute;

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
        }
    }

    void Update() {
        automataCompute.SetTexture(0, "_Result", target);
        automataCompute.SetInt("_Generation", generation);
        automataCompute.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        if (generation > 0)
            generation--;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(target, destination);
    }
}
