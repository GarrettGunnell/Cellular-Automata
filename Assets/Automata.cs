using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Automata : MonoBehaviour {
    
    public ComputeShader automataCompute;
    public bool randomSeed = false;
    [Range(0.01f, 1.0f)]
    public float seedChance = 0.5f;
    public bool bake = false;

    public enum Automaton {
        Rule110 = 1,
        BriansBrain = 3,
        BelousovZhabotinsky = 5,
        Seeds = 7
    } public Automaton automaton;

    [Range(1, 500)]
    public uint V = 100;

    [Range(0.1f, 10.0f)]
    public float k1 = 1.0f;

    [Range(0.1f, 10.0f)]
    public float k2 = 1.0f;

    [Range(0.1f, 100.0f)]
    public float g = 1.0f;

    public bool capturing = false;

    private RenderTexture target;
    private int kernel, threadGroupsX, threadGroupsY, generation;
    private int width, height, frameCount;
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
        frameCount = 0;
        automataCompute.SetTexture((int)automaton - 1, "_Result", target);
        automataCompute.SetTexture((int)automaton, "_Result", target);
        automataCompute.SetInt("_Generation", generation);
        automataCompute.SetInt("_RandomSeed", randomSeed ? 1 : 0);
        automataCompute.SetFloat("_SeedChance", seedChance);
        automataCompute.SetInt("_RandSeed", Random.Range(2, 1000));

        automataCompute.SetInt("_V", (int)V);
        automataCompute.SetFloat("_K1", k1);
        automataCompute.SetFloat("_K2", k2);
        automataCompute.SetFloat("_G", g);

        if (automaton == Automaton.Rule110)
            automataCompute.Dispatch((int)automaton - 1, threadGroupsX, 1, 1);
        else
            automataCompute.Dispatch((int)automaton - 1, threadGroupsX, threadGroupsY, 1);

        if (bake) {
            for (int i = 0; i < 3000; ++i)
                automataCompute.Dispatch((int)automaton, threadGroupsX, threadGroupsY, 1);
        }
    }

    void OnDisable() {
        if (target != null) {
            target.Release();
            target = null;
        }
    }

    void Update() {
        automataCompute.SetTexture((int)automaton, "_Result", target);
        automataCompute.SetInt("_Generation", generation);

        if (timer > 0.04) {
            timer = 0;
            generation--;
            if (automaton == Automaton.Rule110)
                automataCompute.Dispatch((int)automaton, threadGroupsX, 1, 1);
            else
                automataCompute.Dispatch((int)automaton, threadGroupsX, threadGroupsY, 1);
        }

        Capture();
            
        if (timer > 1 && capturing) {
            capturing = false;
            Debug.Log("Finished Capture");
        }

        timer += Time.deltaTime;
        frameCount++;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(target, destination);
    }

    private void Capture() {
        if (capturing) {
            RenderTexture rt = new RenderTexture(width, height, 24);
            GetComponent<Camera>().targetTexture = rt;
            Texture2D screenshot = new Texture2D(506, 506, TextureFormat.RGB24, false);
            GetComponent<Camera>().Render();
            RenderTexture.active = rt;
            screenshot.ReadPixels(new Rect(0, 0, 506, 506), 0, 0);
            GetComponent<Camera>().targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            string filename = string.Format("{0}/../Recordings/{1:000000}.png", Application.dataPath, frameCount);
            System.IO.File.WriteAllBytes(filename, screenshot.EncodeToPNG());
        }
    }
}
