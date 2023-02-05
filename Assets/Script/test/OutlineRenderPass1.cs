using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace UnityEngine.Rendering.Universal
{
    public class OutlineRenderPass1 : ScriptableRenderPass
    {
        //for debugger
        private string m_ProfilerTag;
        private ProfilingSampler m_ProfilingSampler;
        private RenderStateBlock m_RenderStateBlock;

        private RenderQueueType m_renderQueueType;
        private FilteringSettings m_FilteringSettings;

        public Material overrideMaterial { get; set; }
        public int overrideMaterialPassIndex { get; set; }

        private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>()
        {
            new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("UniversalForwardOnly"),
            new ShaderTagId("LightweightForward")
        };

        //Pass的构造方法，参数都由Feature传入
        public OutlineRenderPass1(string profilerTag, RenderPassEvent renderPassEvent, FilterSettings1 filterSettings)
        {
            base.profilingSampler = new ProfilingSampler(nameof(OutlineRenderPass1));
            m_ProfilerTag = profilerTag;
            m_ProfilingSampler = new ProfilingSampler(profilerTag);

            this.renderPassEvent = renderPassEvent;
            m_renderQueueType = filterSettings.renderQueueType;
            RenderQueueRange renderQueueRange = (filterSettings.renderQueueType == RenderQueueType.Transparent)
                ? RenderQueueRange.transparent
                : RenderQueueRange.opaque;
            uint renderingLayerMask = (uint)1 << filterSettings.renderingLayerMask - 1;
            m_FilteringSettings = new FilteringSettings(renderQueueRange, filterSettings.layerMask, renderingLayerMask);

            m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
        }

        public void SetDepthState(bool writeEnabled, CompareFunction function = CompareFunction.Less)
        {
            m_RenderStateBlock.mask |= RenderStateMask.Depth;
            m_RenderStateBlock.depthState = new DepthState(writeEnabled, function);
        }

        /// <summary>
        /// 最重要的方法，用来定义CommandBuffer并执行
        /// </summary>
        /// <param name="context"></param>
        /// <param name="renderingData"></param>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            SortingCriteria sortingCriteria = (m_renderQueueType == RenderQueueType.Transparent)
                ? SortingCriteria.CommonTransparent
                : renderingData.cameraData.defaultOpaqueSortFlags;

            var drawingSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortingCriteria);
            drawingSettings.overrideMaterial = overrideMaterial;
            drawingSettings.overrideMaterialPassIndex = overrideMaterialPassIndex;
            //这里不需要所以没有直接写CommandBuffer，在下面Feature的AddRenderPasses加入了渲染队列，底层还是CB
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings);
        }
    }
}