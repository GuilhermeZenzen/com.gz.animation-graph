using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;

namespace GZ.AnimationGraph.Editor
{
    [CustomEditor(typeof(StateEventsAsset))]
    public class StateEventsAssetInspector : UnityEditor.Editor
    {
        private ReorderableList _list;

        private void OnEnable()
        {
            var asset = (StateEventsAsset)target;

            for (int i = 0; i < asset.Events.Count; i++)
            {
                if (string.IsNullOrEmpty(asset.Events[i].Name))
                {
                    asset.Events[i].Name = $"Event {i}";
                }
            }

            float margin = 3;

            _list = new ReorderableList(serializedObject, serializedObject.FindProperty("Events"), true, true, true, true);
            _list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Events");
            _list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                StateEventAsset evt = asset.Events[index];
                SerializedProperty evtSp = serializedObject.FindProperty("Events").GetArrayElementAtIndex(index);

                rect = rect.AddY(margin);

                float buttonWidth = 35f;
                float nameWidth = rect.width - buttonWidth;
                float nameHeight = EditorGUI.GetPropertyHeight(evtSp.FindPropertyRelative("Name"));
                evt.IsExpanded = EditorGUI.Foldout(rect.Width(nameWidth).Height(nameHeight).AddX(10f), evt.IsExpanded, evt.Name, true);

                if (GUI.Button(rect.AddX(nameWidth).Width(buttonWidth).Height(nameHeight), "Edit"))
                {
                    RenameEditor.Open(evt.Name, newName =>
                    {
                        evtSp.FindPropertyRelative("Name").stringValue = NameValidation.ValidateName(newName, validationName => !asset.Events.Exists(evtAsset => evtAsset.Name.Equals(validationName)));
                    });
                }

                if (evt.IsExpanded)
                {
                    rect = rect.AddY(20f + margin);

                    var stateName = evtSp.FindPropertyRelative("StateName");
                    float stateNameHeight = EditorGUI.GetPropertyHeight(stateName);
                    EditorGUI.PropertyField(rect.Height(stateNameHeight), stateName);
                    rect = rect.AddY(stateNameHeight + margin);

                    var timeType = evtSp.FindPropertyRelative("TimeType");
                    float timeTypeHeight = EditorGUI.GetPropertyHeight(timeType);
                    EditorGUI.PropertyField(rect.Height(timeTypeHeight), timeType);
                    rect = rect.AddY(timeTypeHeight + margin);

                    if (evtSp.FindPropertyRelative("TimeType").enumValueIndex == (int)TimeType.Frame)
                    {
                        var frame = evtSp.FindPropertyRelative("Frame");
                        EditorGUI.PropertyField(rect.Height(EditorGUI.GetPropertyHeight(frame)), frame);
                    }
                    else
                    {
                        var normalizedTime = evtSp.FindPropertyRelative("NormalizedTime");
                        EditorGUI.PropertyField(rect.Height(EditorGUI.GetPropertyHeight(normalizedTime)), normalizedTime);
                    }
                }
            };
            _list.elementHeightCallback = index =>
            {
                StateEventAsset evt = asset.Events[index];
                SerializedProperty evtSp = serializedObject.FindProperty("Events").GetArrayElementAtIndex(index);
                float nameHeight = EditorGUI.GetPropertyHeight(evtSp.FindPropertyRelative("Name"));

                if (evt.IsExpanded)
                {
                    return nameHeight + EditorGUI.GetPropertyHeight(evtSp.FindPropertyRelative("StateName")) + EditorGUI.GetPropertyHeight(evtSp.FindPropertyRelative("TimeType")) + EditorGUI.GetPropertyHeight(evtSp.FindPropertyRelative("Frame")) + margin * 6f;
                }
                else
                {
                    return nameHeight + margin * 2f;
                }
            };
        }
        

        public override void OnInspectorGUI()
        {
            _list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
