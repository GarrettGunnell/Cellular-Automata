using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Automata : MonoBehaviour {
    
    public ComputeShader automataCompute;
    public bool randomSeed = false;
    [Range(0.01f, 1.0f)]
    public float seedChance = 0.5f;

    public bool capturing = false;

    private RenderTexture target;
    private int kernel, threadGroupsX, threadGroupsY, generation;
    private int width, height;
    private float timer;

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
        timer = 0.0f;
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

        if (timer > 0.01 && generation > 0) {
            timer = 0;
            generation--;
        }

        if (timer > 1) {
            capturing = false;
            Debug.Log("Finished Capture");
        }

        timer += Time.deltaTime;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(target, destination);
    }
}
