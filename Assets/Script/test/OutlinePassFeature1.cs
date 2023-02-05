using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


[Serializable]
public class FilterSettings1
{
    public RenderQueueType renderQueueType; //透明还是不透明，Unity定义的enum
    public LayerMask layerMask; //渲染目标的Layer
    [Range(1, 32)] public int renderingLayerMask; //我想要指定的RenderingLayerMask

    public FilterSettings1()
    {
        renderQueueType = RenderQueueType.Opaque; //默认不透明
        layerMask = -1; //默认渲染所有层
        renderingLayerMask = 32; //默认渲染32
    }
}

/// <summary>
/// 自定义Feature，实现后会自动在RenderFeature面板上可供添加
/// </summary>
public class OutlinePassFeature1 : ScriptableRendererFeature
{
    public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques; //和官方的一样用来表示什么时候插入Pass，默认在渲染完不透明物体后
    public FilterSettings1 filterSettings; //上面的一些自定义过滤设置
    public Material material; //我想用的新的渲染指定物体的材质
    public int[] passes; //我想指定的几个Pass的Index

    [Space(10)] //下面三个是和Unity一样的深度设置
    public bool overrideDepthState = false;

    public CompareFunction depthCompareFunction = CompareFunction.LessEqual;
    public bool enableWrite = true;

    List<OutlineRenderPass1> m_ScriptablePasses = new List<OutlineRenderPass1>(2);

    /// <summary>
    /// 最重要的方法，用来生成RenderPass
    /// </summary>
    public override void Create()
    {
        if (passes == null) return;
        m_ScriptablePasses.Clear();
        //根据Shader的Pass数生成多个RenderPass
        for (int i = 0; i < passes.Length; i++)
        {
            var scriptablePass = new OutlineRenderPass1(name, Event, filterSettings);
            scriptablePass.overrideMaterial = material;
            scriptablePass.overrideMaterialPassIndex = passes[i];

            if (overrideDepthState)
                scriptablePass.SetDepthState(enableWrite, depthCompareFunction);

            m_ScriptablePasses.Add(scriptablePass);
        }
    }

    //添加Pass到渲染队列
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (passes == null) return;
        foreach (var pass in m_ScriptablePasses)
        {
            renderer.EnqueuePass(pass);
        }
    }
}