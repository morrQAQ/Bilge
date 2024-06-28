using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public class KeyframeRe : EditorWindow
{
    [MenuItem("Tools/Keyframe Reducer")]
    static void Init()
    {
        KeyframeRe window = GetWindow<KeyframeRe>();
        window.Show();
    }

    private AnimationClip originalClip;
    private float positionThreshold = 0.1f;
    private float rotationThreshold = 0.01f;
    private float scaleThreshold = 0.1f;
    private float otherThreshold = 0.01f;
    private string newClipName = "OptimizedAnimation";

    void OnGUI()
    {
        GUILayout.Label("Keyframe Reducer", EditorStyles.boldLabel);
        originalClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", originalClip, typeof(AnimationClip), true);
        positionThreshold = EditorGUILayout.FloatField("Position Threshold", positionThreshold);
        rotationThreshold = EditorGUILayout.FloatField("Rotation Threshold", rotationThreshold);
        scaleThreshold = EditorGUILayout.FloatField("Scale Threshold", scaleThreshold);
        otherThreshold = EditorGUILayout.FloatField("Other Properties Threshold", otherThreshold);
        newClipName = EditorGUILayout.TextField("New Clip Name", newClipName);

        if (GUILayout.Button("Reduce Keyframes and Save As New Clip"))
        {
            ReduceKeyframesAndSaveNewClip();
        }
    }

    void ReduceKeyframesAndSaveNewClip()
    {
        if (originalClip == null)
        {
            Debug.LogWarning("No animation clip selected.");
            return;
        }

        var newClip = new AnimationClip { name = newClipName };

        ReduceKeyframesForAllCurves(originalClip, newClip);

        var path = AssetDatabase.GetAssetPath(originalClip);
        var newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), $"{newClipName}.anim");
        AssetDatabase.CreateAsset(newClip, newPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"Keyframe reduction completed. New clip saved at: {newPath}");
    }

    void ReduceKeyframesForAllCurves(AnimationClip originalClip, AnimationClip newClip)
    {
        var curveBindings = AnimationUtility.GetCurveBindings(originalClip);
        foreach (var binding in curveBindings)
        {
            var curve = AnimationUtility.GetEditorCurve(originalClip, binding);
            float threshold = GetThresholdForProperty(binding.propertyName);
            var newKeys = ReduceKeyframes(curve.keys, threshold).ToArray();
            var newCurve = new AnimationCurve(newKeys);
            AnimationUtility.SetEditorCurve(newClip, binding, newCurve);
        }
    }

    float GetThresholdForProperty(string propertyName) => propertyName.ToLower() switch
    {
        string p when p.Contains("position") => positionThreshold,
        string p when p.Contains("rotation") => rotationThreshold,
        string p when p.Contains("scale") => scaleThreshold,
        _ => otherThreshold
    };

    IEnumerable<Keyframe> ReduceKeyframes(Keyframe[] keys, float threshold)
    {
        Keyframe? lastKey = null;
        foreach (var key in keys)
        {
            if (lastKey.HasValue && Mathf.Abs(key.value - lastKey.Value.value) < threshold)
            {
                continue;
            }
            lastKey = key;
            yield return key;
        }
    }
}
