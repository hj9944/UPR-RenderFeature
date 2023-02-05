using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Rendering.Universal
{
	[Serializable] // 参数面板设置
	public class FilterSettings
	{
		// 轮廓线颜色
		public Color outlineColor = Color.blue;
		// 轮廓线宽度
		[Range(0, 0.1f)] public float outlineWidth = 0.04f;
		// 过滤用的渲染队列
		public RenderQueue renderQueue = RenderQueue.Geometry;
		// 过滤用的物体层
		public LayerMask layerMask = -1;
		// 过滤用的渲染层
		[Range(1, 32)] public int renderingLayerMask;
	}
	public class OutlineRenderFeature : ScriptableRendererFeature
	{
		// 渲染时机
		public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingSkybox;
		// 参数面板设置实例化
		public FilterSettings filterSettings;
		// 渲染用shader
		public Shader shader;
		// 材质
		private Material m_Material;
		// 使用第几个pass的参数列表
		public int[] passes;
		[Space(10)]
		// 是否重新写入渲染状态
		public bool overrideDepthState = false;
		// 深度比较参数
		public CompareFunction depthCompareFunction = CompareFunction.LessEqual;
		// 是否开启深度写入
		public bool enableWrite = true;
		// 我们一共要渲染几个Pass
		List<ScriptableRenderPass> m_ScriptablePasses = new List<ScriptableRenderPass>();

		// 将Pass添加到渲染列表
		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			if (passes==null || shader==null)
			{
				return;
			}
			foreach (OutlineRenderPass item in m_ScriptablePasses)
			{
				var cameraColorTarget = renderer.cameraColorTarget;
				renderer.EnqueuePass(item);
			}
		}

		// 创建Passes
		public override void Create()
		{
			if (shader == null || passes == null)
				return;
			// 创建材质
			if (m_Material == null)
				m_Material = CoreUtils.CreateEngineMaterial(shader);

			m_ScriptablePasses.Clear();
			for (int i = 0; i < passes.Length; i++)
			{
				// Pass参数的初始化
				var outlineRenderPass = new OutlineRenderPass();
				outlineRenderPass.Setup(m_Material, passes[i], passEvent, filterSettings);
				if (overrideDepthState)
				{
					outlineRenderPass.SetDepthState(enableWrite, depthCompareFunction);
				}
				m_ScriptablePasses.Add(outlineRenderPass);
			}
		}
	}
} 