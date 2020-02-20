using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Renegadeware {
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Shapes2DColorFromPalette))]
    public class Shapes2DColorFromPaletteInspector : Editor {
        SerializedProperty mPalette;

        SerializedProperty mPaletteIndex;
        SerializedProperty mBrightness;
        SerializedProperty mAlpha;

        SerializedProperty mPaletteIndex2;
        SerializedProperty mBrightness2;
        SerializedProperty mAlpha2;

        SerializedProperty mPaletteIndexOutline;
        SerializedProperty mBrightnessOutline;
        SerializedProperty mAlphaOutline;

        void OnDisable() {
            Undo.undoRedoPerformed -= ApplyColor;
        }

        void OnEnable() {
            mPalette = serializedObject.FindProperty("_palette");

            mPaletteIndex = serializedObject.FindProperty("_index");
            mBrightness = serializedObject.FindProperty("_brightness");
            mAlpha = serializedObject.FindProperty("_alpha");

            mPaletteIndex2 = serializedObject.FindProperty("_index2");
            mBrightness2 = serializedObject.FindProperty("_brightness2");
            mAlpha2 = serializedObject.FindProperty("_alpha2");

            mPaletteIndexOutline = serializedObject.FindProperty("_indexOutline");
            mBrightnessOutline = serializedObject.FindProperty("_brightnessOutline");
            mAlphaOutline = serializedObject.FindProperty("_alphaOutline");

            ApplyColor();

            Undo.undoRedoPerformed += ApplyColor;
        }

        void ApplyColor() {
            //refresh
            var dats = serializedObject.targetObjects;
            for(int i = 0; i < dats.Length; i++) {
                var dat = (Shapes2DColorFromPalette)dats[i];
                dat.ApplyColor();
            }
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            //var dat = target as ColorFromPaletteBase;

            serializedObject.Update();

            EditorGUILayout.PropertyField(mPalette);

            M8.ColorPalette palette = mPalette.objectReferenceValue ? (M8.ColorPalette)mPalette.objectReferenceValue : null;

            DoEdit(palette, mPaletteIndex, mBrightness, mAlpha);

            DoEdit(palette, mPaletteIndex2, mBrightness2, mAlpha2);

            DoEdit(palette, mPaletteIndexOutline, mBrightnessOutline, mAlphaOutline);

            if(serializedObject.ApplyModifiedProperties())
                ApplyColor();
        }

        void DoEdit(M8.ColorPalette palette, SerializedProperty ind, SerializedProperty brightness, SerializedProperty alpha) {
            //Palette Edit
            EditorGUILayout.BeginVertical(GUI.skin.box);

            if(palette)
                EditorGUILayout.IntSlider(ind, 0, palette.count - 1);
            else
                EditorGUILayout.PropertyField(ind);

            //Settings
            EditorGUILayout.Slider(brightness, 0f, 2f);

            EditorGUILayout.Slider(alpha, 0f, 1f);

            EditorGUILayout.EndVertical();
        }
    }
}