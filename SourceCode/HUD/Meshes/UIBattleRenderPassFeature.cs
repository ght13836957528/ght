using RenderFeature;using UW.HUD;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UIBattleRenderPassFeature : UWRenderFeature<UIBattleRenderPassFeature>
{
    public class UIBattleHudPass : ScriptableRenderPass
    {
        string m_ProfilerTag = "BattleUI";

        static public HUDRenderMesh HpMesh = null;
        static public HUDRenderMesh JumpWorldMesh = null;

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescripor)
        {
            RenderTextureDescriptor descriptor = cameraTextureDescripor;
            descriptor.msaaSamples = 1;
            descriptor.depthBufferBits = 0;
        }
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {

            if (HpMesh == null && JumpWorldMesh == null)
                return;
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
            if (HpMesh != null && HpMesh.RealMesh != null && HpMesh.RealMaterial != null)
                cmd.DrawMesh(HpMesh.RealMesh, Matrix4x4.identity, HpMesh.RealMaterial);
            if (JumpWorldMesh != null && JumpWorldMesh.RealMesh != null && JumpWorldMesh.RealMaterial != null)
                cmd.DrawMesh(JumpWorldMesh.RealMesh, Matrix4x4.identity, JumpWorldMesh.RealMaterial);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }

        public static void ClearMesh()
        {
            HpMesh = null;
            JumpWorldMesh = null;
        }
    }

    UIBattleHudPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        base.Create();
        m_ScriptablePass = new UIBattleHudPass();

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void UWAddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
	    renderer.EnqueuePass(m_ScriptablePass);
    }
}


