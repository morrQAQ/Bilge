using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public class Rebinding : EditorWindow
{
    [MenuItem("Tools/Animation Rebind")]
    static void Init()
    {
        GetWindow<Rebinding>().Show();
    }

    private AnimationClip originalClip;
    private GameObject targetObject;
    private string newClipName = "ModifiedAnimation";

    void OnGUI()
    {
        GUILayout.Label("Animation Rebind", EditorStyles.boldLabel);
        originalClip = (AnimationClip)EditorGUILayout.ObjectField("Original Clip", originalClip, typeof(AnimationClip), true);
        targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);
        newClipName = EditorGUILayout.TextField("New Clip Name", newClipName);

        if (GUILayout.Button("Modify Animation Bindings and Save As New Clip"))
        {
            ModifyAnimationBindingsAndSaveNewClip();
        }
    }

    void ModifyAnimationBindingsAndSaveNewClip()
    {
        if (originalClip == null || targetObject == null)
        {
            Debug.LogWarning("Original clip or target object is not set.");
            return;
        }

        var newClip = new AnimationClip { name = newClipName };

        var newCurveBindings = AnimationUtility.GetCurveBindings(originalClip)
            .Select(binding => new
            {
                OriginalBinding = binding,
                Curve = AnimationUtility.GetEditorCurve(originalClip, binding),
                NewBinding = CreateNewBinding(binding, targetObject)
            });

        foreach (var item in newCurveBindings)
        {
            AnimationUtility.SetEditorCurve(newClip, item.NewBinding, item.Curve);
        }

        SaveNewClip(newClip);
    }

    EditorCurveBinding CreateNewBinding(EditorCurveBinding binding, GameObject targetObject)
    {
        return new EditorCurveBinding
        {
            path = AnimationUtility.CalculateTransformPath(targetObject.transform, targetObject.transform.root),
            propertyName = binding.propertyName,
            type = binding.type
        };
    }

    void SaveNewClip(AnimationClip newClip)
    {
        var path = AssetDatabase.GetAssetPath(originalClip);
        var newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), $"{newClipName}.anim");
        AssetDatabase.CreateAsset(newClip, newPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"Animation binding modification completed. New clip saved at: {newPath}");
    }
}
