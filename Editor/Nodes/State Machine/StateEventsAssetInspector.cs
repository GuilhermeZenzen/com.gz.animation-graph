using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using GZ.Tools.UnityUtility.Editor;
using GZ.UnityUtility;

namespace GZ.AnimationGraph.Editor
{
    using static CustomStyles;
    using static ReorderableListStyle;

    [CustomEditor(typeof(StateEventsCollectionAsset))]
    public class StateEventsAssetInspector : UnityEditor.Editor
    {
        private StateEventsCollectionAsset _asset;

        private ReorderableList _list;
        private ReorderableListItemsDrawer _listDrawer;

        private List<ReorderableListDrawer<StateEventAsset>> _stateEventsLists = new List<ReorderableListDrawer<StateEventAsset>>();

        private void OnEnable()
        {
            _asset = (StateEventsCollectionAsset)target;
            
            for (int i = 0; i < _asset.StatesEvents.Count; i++)
            {
                CreateStateEventList(_asset.StatesEvents[i]);

                if (string.IsNullOrEmpty(_asset.StatesEvents[i].Name))
                {
                    _asset.StatesEvents[i].Name = $"State {i}";
                }

                for (int j = 0; j < _asset.StatesEvents[i].Events.Count; j++)
                {
                    if (string.IsNullOrEmpty(_asset.StatesEvents[i].Events[j].Name))
                    {
                        _asset.StatesEvents[i].Events[j].Name = $"Event {j}";
                    }
                }
            }

            _listDrawer = new ReorderableListItemsDrawer();
            _list = new ReorderableList(_asset.StatesEvents, typeof(StateEventsAsset), true, true, true, true);
            _list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "States");
            _list.onAddCallback = list =>
            {
                var stateEvents = new StateEventsAsset() { Name = ValidateStateName("New State") };
                _asset.StatesEvents.Add(stateEvents);
                CreateStateEventList(stateEvents);
            };
            _list.onReorderCallbackWithDetails = (list, from, to) =>
            {
                _asset.StatesEvents.Move(from, to);
                _stateEventsLists.Move(from, to);
            };
            _list.onRemoveCallback = list =>
            {
                _asset.StatesEvents.RemoveAt(list.index);
                _stateEventsLists.RemoveAt(list.index);
            };
            _list.elementHeightCallback = index =>
            {
                if (!_listDrawer.StartDrawing(index)) { return _listDrawer.Height; }

                _listDrawer.Space(TOP_PADDING);

                _listDrawer.Draw(EditorGUI.GetPropertyHeight(SerializedPropertyType.String, new GUIContent("Name")), rect =>
                {
                    GUIContent nameLabel = new GUIContent(_asset.StatesEvents[index].Name);
                    float nameLabelWidth = Mathf.Min(rect.width - EDIT_NAME_BUTTON_WIDTH - INTERNAL_HORIZONTAL_MARGIN - FOLDOUT_MARGIN, GUI.skin.label.CalcSize(nameLabel).x);

                    _asset.StatesEvents[index].IsExpanded = EditorGUI.Foldout(rect.AddX(FOLDOUT_MARGIN).Width(nameLabelWidth), _asset.StatesEvents[index].IsExpanded, nameLabel, true);

                    if (GUI.Button(rect.AddX(rect.width - EDIT_NAME_BUTTON_WIDTH).Width(EDIT_NAME_BUTTON_WIDTH), "Edit"))
                    {
                        RenameEditor.Open(_asset.StatesEvents[index].Name, newName => _asset.StatesEvents[index].Name = ValidateStateName(newName));
                    }
                }, TOP_PADDING);

                if (_asset.StatesEvents[index].IsExpanded)
                {
                    _stateEventsLists[index].Drawer.CanDraw = false;
                    _listDrawer.Draw(_stateEventsLists[index].List.GetHeight(), rect =>
                    {
                        _stateEventsLists[index].List.DoList(rect);
                    }, TOP_PADDING);
                    _stateEventsLists[index].Drawer.CanDraw = true;
                }

                return _listDrawer.Height;
            };
            _list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                _listDrawer.Display(rect, index);
            };
        }

        private void CreateStateEventList(StateEventsAsset events)
        {
            var listDrawer = new ReorderableListDrawer<StateEventAsset>("Events", events.Events, true);
            listDrawer.List.onAddCallback = list =>
            {
                listDrawer.Source.Add(new StateEventAsset() { Name = ValidateEventName("New Event", events.Events) });
            };
            listDrawer.List.elementHeightCallback = index =>
            {
                if (!listDrawer.Drawer.StartDrawing(index)) { return listDrawer.Drawer.Height; }

                listDrawer.Drawer.Space(TOP_PADDING);

                listDrawer.Drawer.Draw(EditorGUI.GetPropertyHeight(SerializedPropertyType.String, new GUIContent("Name")), rect =>
                {
                    GUIContent nameLabel = new GUIContent(listDrawer.Source[index].Name);
                    float nameLabelWidth = Mathf.Min(rect.width - EDIT_NAME_BUTTON_WIDTH - INTERNAL_HORIZONTAL_MARGIN - FOLDOUT_MARGIN, GUI.skin.label.CalcSize(nameLabel).x);

                    listDrawer.Source[index].IsExpanded = EditorGUI.Foldout(rect.AddX(FOLDOUT_MARGIN).Width(nameLabelWidth), listDrawer.Source[index].IsExpanded, listDrawer.Source[index].Name, true);

                    if (GUI.Button(rect.AddX(rect.width - EDIT_NAME_BUTTON_WIDTH).Width(EDIT_NAME_BUTTON_WIDTH), "Edit"))
                    {
                        RenameEditor.Open(listDrawer.Source[index].Name, newName => listDrawer.Source[index].Name = ValidateEventName(newName, listDrawer.Source));
                    }
                }, INTERNAL_VERTICAL_MARGIN);

                if (listDrawer.Source[index].IsExpanded)
                {
                    listDrawer.Drawer.Draw(EditorGUI.GetPropertyHeight(SerializedPropertyType.Float, new GUIContent("Normalized Time")), rect =>
                    {
                        listDrawer.Source[index].NormalizedTime = EditorGUI.FloatField(rect, "Normalized Time", listDrawer.Source[index].NormalizedTime);
                    }, TOP_PADDING);
                }

                return listDrawer.Drawer.Height;
            };

            _stateEventsLists.Add(listDrawer);
        }

        private string ValidateEventName(string name, List<StateEventAsset> events) => NameValidation.ValidateName(name, validationName => !events.Exists(evtAsset => evtAsset.Name.Equals(validationName)));

        private string ValidateStateName(string name) => NameValidation.ValidateName(name, validationName => !_asset.StatesEvents.Exists(state => state.Name.Equals(validationName)));

        public override void OnInspectorGUI()
        {
            _list.DoLayoutList();

            EditorUtility.SetDirty(target);
        }
    }
}
