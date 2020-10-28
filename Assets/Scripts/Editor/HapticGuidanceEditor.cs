using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HapticGuidance)), CanEditMultipleObjects]
public class HapticGuidanceEditor : Editor
{
    private HapticGuidance guidance;

    public SerializedProperty
        disableGuidance_Prop,
        persistent_Prop,
        //applyForceToRigidBody_Prop,
        triggerDistance_Prop,
        gradientDistance_Prop,
        customTranform_Prop,
        //spring
        model_Prop,
        magnitude_Prop,
        stiffness_Prop,
        restDistance_Prop,
        // hunt-crossley
        insideOut_Prop,
        n_Prop,
        alpha_Prop,
        // zincbath
        forceScale_Prop,
        findIntersectingVolume_Prop,

        dampRB_Prop,
        projRBDamping_Prop,
        rbDampingConst_Prop,

        dampHD_Prop,
        projHDDamping_Prop,
        hdDampingConst_Prop;


    private void OnEnable()
    {
        disableGuidance_Prop = serializedObject.FindProperty("disableGuidance");
        persistent_Prop = serializedObject.FindProperty("persistent");

        guidance = (HapticGuidance)target;
        //applyForceToRigidBody_Prop = serializedObject.FindProperty("applyForceToRigidBody");
        triggerDistance_Prop = serializedObject.FindProperty("triggerDistance");
        gradientDistance_Prop = serializedObject.FindProperty("gradientDistance");
        customTranform_Prop = serializedObject.FindProperty("customTransform");

        model_Prop = serializedObject.FindProperty("contactModel");
        magnitude_Prop = serializedObject.FindProperty("magnitude");
        stiffness_Prop = serializedObject.FindProperty("stiffness");
        restDistance_Prop = serializedObject.FindProperty("restDistance");
        n_Prop = serializedObject.FindProperty("n");
        alpha_Prop = serializedObject.FindProperty("alpha");

        forceScale_Prop = serializedObject.FindProperty("forceScale");
        findIntersectingVolume_Prop = serializedObject.FindProperty("findIntersectingVolume");


        // Damping properties
        insideOut_Prop = serializedObject.FindProperty("insideOut");
        dampRB_Prop = serializedObject.FindProperty("dampRB");
        projRBDamping_Prop = serializedObject.FindProperty("projRBDamping");
        rbDampingConst_Prop = serializedObject.FindProperty("rbDampingConst");

        dampHD_Prop = serializedObject.FindProperty("dampHD");
        projHDDamping_Prop = serializedObject.FindProperty("projHDDamping");
        hdDampingConst_Prop = serializedObject.FindProperty("hdDampingConst");
    }

    void OnDisable()
    {
        //Debug.Log("OnDisable is called");
    }

    void OnDestroy()
    {
        //Debug.Log("OnDestroy is called");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Custom styles
        GUIStyle infoText = new GUIStyle();
        infoText.fontSize = 10;
        GUIStyle titleText = new GUIStyle();
        titleText.fontSize = 15;

        // Basic settings:
        EditorGUILayout.LabelField("Haptic Guidance Settings", titleText);
        EditorGUILayout.LabelField("Note that the maximum force that can be applied is 1\nPlease properly scale the stiffness to achieve the appropriate range", infoText, GUILayout.Height(25));
        EditorGUILayout.PropertyField(disableGuidance_Prop, new GUIContent("Disable", "Enable this to disable the haptic guidance of this script."));
        EditorGUILayout.PropertyField(persistent_Prop, new GUIContent("Make Persistent", "Enable this to make the haptic effect permanent (more CPU but less weird force spikes)."));

        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.Vector3Field("Guidance force", guidance.force);
        EditorGUILayout.Slider(guidance.force.magnitude,0,1);
        EditorGUI.EndDisabledGroup(); 
        //EditorGUILayout.PropertyField(applyForceToRigidBody_Prop, new GUIContent("Apply force to rigid body instead of haptic device"));


        EditorGUILayout.PropertyField(triggerDistance_Prop);
        EditorGUILayout.PropertyField(gradientDistance_Prop);
        EditorGUILayout.PropertyField(customTranform_Prop, new GUIContent("Custom Transform Point", "If not set, the haptic manipulator position will be used."));
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Select a contact model");
        EditorGUILayout.PropertyField(model_Prop);
        HapticGuidance.CONTACT_MODEL model = (HapticGuidance.CONTACT_MODEL)model_Prop.enumValueIndex; // intValue could also be used, but this is more clear imo

        // Model specific settings
        switch (model)
        {
            case HapticGuidance.CONTACT_MODEL.constant:
                EditorGUILayout.Slider(magnitude_Prop, -1f, 1f, new GUIContent("Magnitude"));
                break;
            case HapticGuidance.CONTACT_MODEL.linearSpring:
                //EditorGUILayout.Slider(gain_Prop, -1f, 1f, new GUIContent("Gain"));
                EditorGUILayout.PropertyField(stiffness_Prop, new GUIContent("Stiffness"));
                EditorGUILayout.Slider(restDistance_Prop, -guidance.triggerDistance, guidance.triggerDistance, new GUIContent("Rest distance of spring"));
                break;
            case HapticGuidance.CONTACT_MODEL.huntCrossley:
                EditorGUILayout.PropertyField(insideOut_Prop, new GUIContent("Flip inside out"));
                EditorGUILayout.PropertyField(stiffness_Prop, new GUIContent("Stiffness"));
                EditorGUILayout.PropertyField(n_Prop, new GUIContent("n (power~=1.1-1.3 for soft tissue)"));
                EditorGUILayout.PropertyField(alpha_Prop, new GUIContent("alpha (restitutivity or something ~=0.08-0.32s/m for steel)"));
                break;
            case HapticGuidance.CONTACT_MODEL.zincBath:
                EditorGUILayout.LabelField("The force here is scaled with the intersecting volume fraction (linearly)", infoText, GUILayout.Height(25));
                EditorGUILayout.PropertyField(forceScale_Prop, new GUIContent("Force scaling"));
                EditorGUILayout.PropertyField(findIntersectingVolume_Prop, new GUIContent("findIntersectingVolume script"));

                
                break;
        }
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Damper Settings");
        

        EditorGUILayout.PropertyField(dampRB_Prop, new GUIContent("Damp rigidbody (recommended)"));
        if (dampRB_Prop.boolValue)
        {
            EditorGUILayout.PropertyField(rbDampingConst_Prop, new GUIContent("Damping constant RB"));
            EditorGUILayout.PropertyField(projRBDamping_Prop, new GUIContent("Project RB damping on force direction"));
        }
            
        EditorGUILayout.PropertyField(dampHD_Prop, new GUIContent("Damp haptic device"));
        if (dampHD_Prop.boolValue)
        {
            EditorGUILayout.PropertyField(hdDampingConst_Prop, new GUIContent("Damping constant HD"));
            EditorGUILayout.PropertyField(projHDDamping_Prop, new GUIContent("Project HD damping on force direction"));
        }

        serializedObject.ApplyModifiedProperties();

        //DrawDefaultInspector();
    }
}



