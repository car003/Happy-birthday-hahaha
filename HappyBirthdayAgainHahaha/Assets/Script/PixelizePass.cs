//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelizePass : ScriptableRenderPass
{
    private PixelizeFeature.CustomPassSettings settings;

    private RenderTargetIdentifier colorBuffer, pixelBuffer;        //to declare render target identifiers for the camera's color texture and the pixelated buffer
    private int pixelBufferID = Shader.PropertyToID("_PixelBuffer");//pixelated impage to a temporary render texture each frame, we need this id to issue the operation     
                                                                    //initialized the shader.propertyToID to declare the int

    private Material material; //to hold the pixelation material 
    private int pixelScreenHeigh, pixelScreenWidth; //pixelated screen h & W

    public PixelizePass(PixelizeFeature.CustomPassSettings settings) 
    {
        this.settings = settings;
        this.renderPassEvent = settings.renderPassEvent; 
        if (material == null) material = CoreUtils.CreateEngineMaterial("Hidden/Pixelize");
    }

    //public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    //{
    //    throw new System.NotImplementedException();
    //}

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        //assign the camera's color target to the color buffer 
        colorBuffer = renderingData.cameraData.renderer.cameraColorTarget;
        //extract its descriptor into a separate variable
        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor; 

        //to calculate our target width using the camera's aspect ratio
        pixelScreenHeigh = settings.screenHeigh;
        pixelScreenWidth = (int)(pixelScreenHeigh * renderingData.cameraData.camera.aspect + 0.5f); 

        //pixelated screen resolution
        material.SetVector("_BlockCount", new Vector2(pixelScreenWidth, pixelScreenHeigh));
        //block size interm of screen space uv coordinates
        material.SetVector("_BlockSize", new Vector2(1.0f / pixelScreenWidth, 1.0f / pixelScreenHeigh));
        //avoid calculation it per pixel in the shader 
        material.SetVector("_HalfBlockSize", new Vector2(0.5f / pixelScreenWidth, 0.5f / pixelScreenHeigh)); 

        descriptor.height = pixelScreenHeigh;
        descriptor.width = pixelScreenWidth;

        //filtermode.point avoid blurry in upscale
        cmd.GetTemporaryRT(pixelBufferID, descriptor, FilterMode.Point); 
        pixelBuffer = new RenderTargetIdentifier(pixelBufferID);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        //get command buffer from the pool
        CommandBuffer cmd = CommandBufferPool.Get(); 
        using (new ProfilingScope(cmd, new ProfilingSampler("Pixelize Pass")))
        {
            //copy camera's color target the color butter to pixel buffer special material
            Blit(cmd, colorBuffer, pixelBuffer, material);
            //copy the pixel buffer into the color buffer -> the result render to the screen, not apply material twice
            Blit(cmd, pixelBuffer, colorBuffer);
        }

        //sciptable render context to excute the cmd buffer
        context.ExecuteCommandBuffer(cmd);
        //release it back into the cmd pool to avoid any leaks on each frame
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        //release the render texture we requested
        if (cmd == null) throw new System.ArgumentNullException("cmd");
        //release the temporary render texture with the id stored into pixelcuffer id
        cmd.ReleaseTemporaryRT(pixelBufferID);
    }
}
