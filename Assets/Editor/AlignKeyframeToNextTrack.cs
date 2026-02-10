using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class SelectiveStaircaseTool : EditorWindow
{
    private AnimationClip targetClip;
    private List<string> selectedPaths = new List<string>();
    private Vector2 scrollPos;
    
    [MenuItem("Window/Animation/Selective Staircase Tool")]
    public static void ShowWindow()
    {
        GetWindow<SelectiveStaircaseTool>("Selective Staircase");
    }
    
    void OnEnable()
    {
        if (targetClip != null)
        {
            UpdatePathList();
        }
    }
    
    void OnGUI()
    {
        GUILayout.Label("Selective Staircase Alignment", EditorStyles.boldLabel);
        
        AnimationClip newClip = (AnimationClip)EditorGUILayout.ObjectField(
            "Animation Clip", 
            targetClip, 
            typeof(AnimationClip), 
            false
        );
        
        if (newClip != targetClip)
        {
            targetClip = newClip;
            UpdatePathList();
        }
        
        if (targetClip == null)
        {
            EditorGUILayout.HelpBox("Please select an Animation Clip", MessageType.Info);
            return;
        }
        
        EditorGUILayout.Space();
        GUILayout.Label("Select Tracks to Process:");
        
        // 显示轨道选择列表
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
        
        EditorCurveBinding[] allBindings = AnimationUtility.GetCurveBindings(targetClip);
        var uniquePaths = allBindings.Select(b => b.path).Distinct().ToList();
        
        foreach (string path in uniquePaths)
        {
            bool isSelected = selectedPaths.Contains(path);
            bool newSelection = EditorGUILayout.ToggleLeft(path, isSelected);
            
            if (newSelection && !isSelected)
            {
                selectedPaths.Add(path);
            }
            else if (!newSelection && isSelected)
            {
                selectedPaths.Remove(path);
            }
        }
        
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Select All", GUILayout.Width(100)))
        {
            selectedPaths = uniquePaths.ToList();
        }
        
        if (GUILayout.Button("Deselect All", GUILayout.Width(100)))
        {
            selectedPaths.Clear();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Apply Staircase to Selected Tracks", GUILayout.Height(40)))
        {
            ApplyStaircaseToSelected();
        }
    }
    
    void UpdatePathList()
    {
        selectedPaths.Clear();
        if (targetClip != null)
        {
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(targetClip);
            selectedPaths = bindings.Select(b => b.path).Distinct().ToList();
        }
    }
    
    void ApplyStaircaseToSelected()
    {
        if (targetClip == null || selectedPaths.Count == 0) return;
        
        Undo.RegisterCompleteObjectUndo(targetClip, "Selective Staircase");
        
        // 收集选定轨道的所有曲线
        var trackCurves = new Dictionary<string, List<EditorCurveBinding>>();
        
        foreach (var binding in AnimationUtility.GetCurveBindings(targetClip))
        {
            if (selectedPaths.Contains(binding.path))
            {
                string trackKey = binding.path;
                if (!trackCurves.ContainsKey(trackKey))
                {
                    trackCurves[trackKey] = new List<EditorCurveBinding>();
                }
                trackCurves[trackKey].Add(binding);
            }
        }
        
        // 为每个轨道找到第一个和最后一个关键帧时间
        var trackTimes = new Dictionary<string, (float first, float last)>();
        
        foreach (var track in trackCurves)
        {
            float firstTime = float.MaxValue;
            float lastTime = float.MinValue;
            
            foreach (var binding in track.Value)
            {
                var curve = AnimationUtility.GetEditorCurve(targetClip, binding);
                if (curve != null && curve.keys.Length > 0)
                {
                    firstTime = Mathf.Min(firstTime, curve.keys[0].time);
                    lastTime = Mathf.Max(lastTime, curve.keys[curve.keys.Length - 1].time);
                }
            }
            
            if (firstTime < float.MaxValue && lastTime > float.MinValue)
            {
                trackTimes[track.Key] = (firstTime, lastTime);
            }
        }
        
        // 对轨道排序
        var sortedTracks = trackCurves.Keys.OrderBy(k => k).ToList();
        
        // 应用楼梯式对齐
        for (int i = 1; i < sortedTracks.Count; i++)
        {
            string currentTrack = sortedTracks[i];
            string prevTrack = sortedTracks[i - 1];
            
            if (trackTimes.ContainsKey(prevTrack) && trackTimes.ContainsKey(currentTrack))
            {
                float prevLastTime = trackTimes[prevTrack].last;
                float currentFirstTime = trackTimes[currentTrack].first;
                float timeDifference = currentFirstTime - trackTimes[currentTrack].first;
                
                // 移动当前轨道的关键帧
                foreach (var binding in trackCurves[currentTrack])
                {
                    var curve = AnimationUtility.GetEditorCurve(targetClip, binding);
                    if (curve != null)
                    {
                        // 计算偏移量
                        float offset = prevLastTime - currentFirstTime;
                        
                        // 移动所有关键帧
                        Keyframe[] keys = curve.keys;
                        for (int j = 0; j < keys.Length; j++)
                        {
                            keys[j].time += offset;
                        }
                        
                        curve.keys = keys;
                        AnimationUtility.SetEditorCurve(targetClip, binding, curve);
                    }
                }
            }
        }
        
        Debug.Log($"Applied staircase pattern to {sortedTracks.Count} selected tracks");
        EditorUtility.SetDirty(targetClip);
    }
}