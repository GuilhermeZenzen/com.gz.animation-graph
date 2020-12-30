using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using GZ.Tools.UnityUtility.Editor;
using GZ.UnityUtility;
using GZ.UnityUtility.Editor;

namespace GZ.AnimationGraph.Editor
{
    using static CustomStyles;
    using static ReorderableListStyle;

    [CustomEditor(typeof(StateEventsCollectionAsset))]
    public class StateEventsAssetInspector : UnityEditor.Editor
    {
        private const string STATE_EVENTS = nameof(StateEventsCollectionAsset.StatesEvents);

        private StateEventsCollectionAsset _asset;
        
        private AdvancedReorderableList<StateEventListItem> _list;


        private void OnEnable()
        {
            _asset = (StateEventsCollectionAsset)target;
            
            _list = new AdvancedReorderableList<StateEventListItem>(serializedObject, serializedObject.FindProperty(STATE_EVENTS), true, true, true);
            _list.OnDrawHeaderCallback = rect => EditorGUI.LabelField(rect, "States");
            _list.OnAddCallback = list =>
            {
                list.serializedProperty.AddArrayElement().FindPropertyRelative(nameof(StateEventsAsset.Name)).stringValue = ValidateStateName("New State");
            };
            _list.OnDrawCallback = (list, prop, data, index) =>
            {
                list.Space(TOP_PADDING);

                list.Draw(EditorGUI.GetPropertyHeight(SerializedPropertyType.String, new GUIContent("Name")), rect =>
                {
                    SerializedProperty nameProp = prop.FindPropertyRelative(nameof(StateEventsAsset.Name));
                    DrawEditableNameFoldout(rect, nameProp.stringValue, newName => nameProp.stringValue = ValidateStateName(newName), prop.isExpanded, newExpandedState => prop.isExpanded = newExpandedState);
                });

                if (prop.isExpanded)
                {
                    list.Space(TOP_PADDING);

                    list.Draw(data.List.GetHeight(), rect => data.List.DoList(rect));
                }

                list.Space(BOTTOM_PADDING);
            };
        }

        public class StateEventListItem : IAdvancedReorderableListItemData<StateEventListItem>
        {
            public AdvancedReorderableList List;

            public StateEventListItem()
            {
                List = new AdvancedReorderableList(null, null, true, true, true);
                List.OnAddCallback = list =>
                {
                    List.Elements.AddArrayElement().FindPropertyRelative(nameof(StateEventAsset.Name)).stringValue = ValidateEventName("New Event", List.Elements);
                };
                List.OnDrawCallback = (list, prop, index) =>
                {
                    list.Space(TOP_PADDING);

                    list.Draw(EditorGUI.GetPropertyHeight(SerializedPropertyType.String, new GUIContent("Name")), rect =>
                    {
                        SerializedProperty nameProp = prop.FindPropertyRelative(nameof(StateEventAsset.Name));
                        DrawEditableNameFoldout(rect, nameProp.stringValue, newName => nameProp.stringValue = ValidateEventName(newName, list.Elements), prop.isExpanded, newExpandedState => prop.isExpanded = newExpandedState);
                    });

                    if (prop.isExpanded)
                    {
                        list.Space(TOP_PADDING);

                        var typeProp = prop.FindPropertyRelative(nameof(StateEventAsset.Type));

                        list.Draw(typeProp, INTERNAL_VERTICAL_MARGIN);

                        if ((EventType)typeProp.enumValueIndex == EventType.Trigger)
                        {
                            list.Draw(nameof(StateEventAsset.TriggerTime));
                        }
                        else
                        {
                            list.Draw(nameof(StateEventAsset.StartTime), INTERNAL_VERTICAL_MARGIN);

                            list.Draw(nameof(StateEventAsset.EndTime));
                        }
                    }

                    list.Space(BOTTOM_PADDING);
                };
            }

            public void Refresh(AdvancedReorderableListItem<StateEventListItem> item)
            {
                List.Elements = item.Property.FindPropertyRelative(nameof(StateEventsAsset.Events));
            }
        }

        private void CreateStateEventList(StateEventsAsset events)
        {
            var listDrawer = new ReorderableListDrawer<StateEventAsset>("Events", events.Events, true);
            listDrawer.List.onAddCallback = list =>
            {
                //listDrawer.Source.Add(new StateEventAsset() { Name = ValidateEventName("New Event", events.Events) });
            };
            listDrawer.List.elementHeightCallback = index =>
            {
                if (!listDrawer.Drawer.StartDrawing(index)) { return listDrawer.Drawer.Height; }

                listDrawer.Drawer.Space(TOP_PADDING);

                listDrawer.Drawer.Draw(EditorGUI.GetPropertyHeight(SerializedPropertyType.String, new GUIContent("Name")), rect =>
                {
                    GUIContent nameLabel = new GUIContent(listDrawer.Source[index].Name);
                    float nameLabelWidth = Mathf.Min(rect.width - EDIT_BUTTON_WIDTH - INTERNAL_HORIZONTAL_MARGIN - FOLDOUT_MARGIN, GUI.skin.label.CalcSize(nameLabel).x);

                    //listDrawer.Source[index].IsExpanded = EditorGUI.Foldout(rect.AddX(FOLDOUT_MARGIN).Width(nameLabelWidth), listDrawer.Source[index].IsExpanded, listDrawer.Source[index].Name, true);

                    if (GUI.Button(rect.AddX(rect.width - EDIT_BUTTON_WIDTH).Width(EDIT_BUTTON_WIDTH), "Edit"))
                    {
                        //RenameEditor.Open(listDrawer.Source[index].Name, newName => listDrawer.Source[index].Name = ValidateEventName(newName, listDrawer.Source));
                    }
                }, INTERNAL_VERTICAL_MARGIN);

                //if (listDrawer.Source[index].IsExpanded)
                //{
                //    listDrawer.Drawer.Draw(EditorGUI.GetPropertyHeight(SerializedPropertyType.Float, new GUIContent("Normalized Time")), rect =>
                //    {
                //        listDrawer.Source[index].NormalizedTime = EditorGUI.FloatField(rect, "Normalized Time", listDrawer.Source[index].NormalizedTime);
                //    }, TOP_PADDING);
                //}

                return listDrawer.Drawer.Height;
            };

            //_stateEventsLists.Add(listDrawer);
        }

        private static string ValidateEventName(string name, SerializedProperty events) => NameValidation.ValidateName(name, validationName => !events.Exists(evtAsset => evtAsset.FindPropertyRelative(nameof(StateEventAsset.Name)).stringValue.Equals(validationName)));

        private string ValidateStateName(string name) => NameValidation.ValidateName(name, validationName => !_asset.StatesEvents.Exists(state => state.Name.Equals(validationName)));

        public override void OnInspectorGUI()
        {
            _list.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
