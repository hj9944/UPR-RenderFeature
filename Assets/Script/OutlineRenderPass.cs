using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Rendering.Universal
{
	public class OutlineRenderPass : ScriptableRenderPass
	{
        //标签名，用于续帧调试器中显示缓冲区名称
        const string CommandBufferTag = "Outline Render Pass";
        // 用于后处理的材质
        public Material m_Material;
        // 材质中第几个pass来渲染
        public int m_PassID;
        // 颜色渲染标识符
        RenderTargetIdentifier m_ColorAttachment;
        // 过滤设置
        private RenderQueueRange m_renderQueueRange; //渲染队列的范围
        private FilteringSettings m_FilteringSettings; //渲染过滤设置
        private RenderStateBlock m_RenderStateBlock; //渲染状态的设置，比如深度测试，开启混合等等
        private SortingCriteria m_SortingCriterial; //渲染物体的渲染顺序，从前往后，还是从后往前
        // 轮廓线颜色
        private Color m_OutlineColor;
        // 轮廓线宽度
        private float m_OutlineWidth;
        // 多Pass渲染需要不同的ShaderTagId，应该是这样吧？懒得验证了。
        private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>()
        {
            new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("UniversalForward"),
        };
        // 设置渲染参数
        public void Setup(Material Material, int passID,
            RenderPassEvent renderPassEvent, FilterSettings filterSettings)
        {
            //this.m_ColorAttachment = _ColorAttachment;
            this.m_Material = Material;
            this.m_PassID = passID;
            this.m_OutlineColor = filterSettings.outlineColor;
            this.m_OutlineWidth = filterSettings.outlineWidth;
            // 渲染时机
            this.renderPassEvent = renderPassEvent;
            // 过滤范围，处于哪些渲染队列的可以被渲染
            this.m_renderQueueRange =
                filterSettings.renderQueue == RenderQueue.Geometry?
                RenderQueueRange.opaque: RenderQueueRange.transparent;
            // render layer，这里基本上不靠这个过滤，靠物体的object layer过滤
            uint renderingLayerMask = (uint)1 << filterSettings.renderingLayerMask - 1;
            // 整合一下，得到一个全新的参数
            this.m_FilteringSettings = new FilteringSettings(m_renderQueueRange, filterSettings.layerMask, renderingLayerMask);
            // 渲染状态的设置，比如是否写入深度呀，是否开启混合呀，等等
            this.m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
            // 排序，不多哔哔
            this.m_SortingCriterial =
                filterSettings.renderQueue == RenderQueue.Geometry ?
                SortingCriteria.CommonOpaque : SortingCriteria.CommonTransparent;
        }
        // 重新设置渲染状态
        public void SetDepthState(bool writeEnable, CompareFunction function = CompareFunction.Less)
        {
            m_RenderStateBlock.mask |= RenderStateMask.Depth;
            m_RenderStateBlock.depthState = new DepthState(writeEnable, function);
        }
        /// <summary>
        /// URP会自动调用该执行方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="renderingData"></param>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            m_Material.SetColor("_OutlineColor", m_OutlineColor);
            m_Material.SetFloat("_OutlineWidth", m_OutlineWidth);
            var cmd = CommandBufferPool.Get(CommandBufferTag);
            var drawingSettings = CreateDrawingSettings
                (m_ShaderTagIdList, ref renderingData, m_SortingCriterial);
            drawingSettings.overrideMaterial = m_Material;
            drawingSettings.overrideMaterialPassIndex = m_PassID;
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
} 