//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelizeFeature : ScriptableRendererFeature
{
    // specific screen height to calculate the pixelated resolution
    [System.Serializable]
   public class CustomPassSettings 
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public int screenHeigh = 144;
    }

    [SerializeField] private CustomPassSettings settings; 
    private PixelizePass customPass;

    //initialize the custom paths calling its constructor
    public override void Create() 
    {
        customPass = new PixelizePass(settings);
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) //custom pass add in to each frame, bt not include the screen carmera
    {
#if UNITY_EDITOR
        if (renderingData.cameraData.isSceneViewCamera) return;
#endif
        renderer.EnqueuePass(customPass);
    }
}