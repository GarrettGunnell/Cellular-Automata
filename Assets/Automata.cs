using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Automata : MonoBehaviour {
    
    public ComputeShader automataCompute;
    public bool randomSeed = false;
    [Range(0.01f, 1.0f)]
    public float seedChance = 0.5f;

    private RenderTexture target;
    private int kernel, threadGroupsX, threadGroupsY, generation;
    private int width, height;
    
    void Awake() {
        threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        width = Screen.width;
        height = Screen.height;
    }

    void OnEnable() {
        if (target == null) {
            target = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create();

            automataCompute.SetInt("_Width", width);
            automataCompute.SetInt("_Height", height);
        }

        generation = height - 2;
        automataCompute.SetTexture(0, "_Result", target);
        automataCompute.SetInt("_Generation", generation);
        automataCompute.SetInt("_RandomSeed", randomSeed ? 1 : 0);
        automataCompute.SetFloat("_SeedChance", seedChance);
        automataCompute.SetInt("_RandSeed", Random.Range(2, 1000));
        automataCompute.Dispatch(0, threadGroupsX, 1, 1);
    }

    void OnDisable() {
        if (target != null) {
            target.Release();
            target = null;
        }
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
