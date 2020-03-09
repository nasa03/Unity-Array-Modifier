using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace ArrayModifier
{
    [CustomEditor(typeof(ArrayModifier))]
    public class ArrayModifierEditor : Editor
    {
        private ArrayModifier _target = null;
        private Transform _targetTransform = null;
        private bool _sceneWasDirtyLastTick = false;
        private bool _targetGOWasDirtyLastTick = false;
        private List<GameObject> _duplicates = new List<GameObject>();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Refresh"))
            {
                _target.Calculate();
            }

            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();

            var fitType = (FitType)EditorGUILayout.EnumPopup(new GUIContent("Fit Type", "Determines which method to use for creating duplicates."), _target._fitType);

            var count = _target._count;
            var fitLength = _target._fitLength;
            if (_target._fitType == FitType.FixedCount)
            {
                count = EditorGUILayout.DelayedIntField(new GUIContent("Count", "Determines how many duplicates will get created."), _target._count);
            }
            else if (_target._fitType == FitType.Length)
            {
                fitLength = EditorGUILayout.DelayedFloatField(new GUIContent("Length", "Determines at which length to duplicate to."), _target._fitLength);
            }

            var constantOffset = EditorGUILayout.Vector3Field(new GUIContent("Constant Offset", "Determines the offset between each object in global space."), _target._constantOffset);
            var relativeOffset = EditorGUILayout.Vector3Field(new GUIContent("Relative Offset", "Determines the offset between each object in local space."), _target._relativeOffset);

            if (EditorGUI.EndChangeCheck())
            {
                if (fitType != _target._fitType)
                {
                    Undo.RecordObject(_target, "Changed fit type");
                    _target._fitType = fitType;
                }

                if (count != _target._count)
                {
                    Undo.RecordObject(_target, "Changed count");
                    _target._count = count;
                }

                if (fitLength != _target._fitLength)
                {
                    Undo.RecordObject(_target, "Changed fit length");
                    _target._fitLength = fitLength;
                }

                if (constantOffset != _target._constantOffset)
                {
                    Undo.RecordObject(_target, "Changed constant offset");
                    _target._constantOffset = constantOffset;
                }

                if (relativeOffset != _target._relativeOffset)
                {
                    Undo.RecordObject(_target, "Changed relative offset");
                    _target._relativeOffset = relativeOffset;
                }

                _target.Calculate();
            }
        }

        private void CheckForSceneChanges()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.isDirty && !_sceneWasDirtyLastTick)
            {
                OnSceneChanged();
            }

            _sceneWasDirtyLastTick = activeScene.isDirty;
        }

        private void CheckForTargetGameObjectChanges()
        {
            if (_target == null) return;

            var targetGameObjectIsDirty = false;

            var components = _target.gameObject.GetComponents<MonoBehaviour>();
            foreach (var component in components)
            {
                if (EditorUtility.IsDirty(component))
                {
                    targetGameObjectIsDirty = true;
                    break;
                }
            }

            if (targetGameObjectIsDirty && !_targetGOWasDirtyLastTick)
            {
                OnTargetGOChanged();
            }

            _targetGOWasDirtyLastTick = targetGameObjectIsDirty;
        }

        private void OnSceneChanged()
        {
            _target.Calculate();
            SceneView.RepaintAll();
        }

        private void OnTargetGOChanged()
        {
            _target.Calculate();
            SceneView.RepaintAll();
        }

        private void OnChangedTransform()
        {
            _target.Calculate();
            _target.transform.hasChanged = false;
        }

        private void GetDuplicates()
        {
            _duplicates = _target.GetDuplicates();
        }

        private void Awake()
        {
            _target = target as ArrayModifier;
            _targetTransform = _target.transform;
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += _target.Calculate;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= _target.Calculate;
            GetDuplicates();
        }

        private void OnSceneGUI()
        {
            if (_target == null) return;

            CheckForSceneChanges();
            CheckForTargetGameObjectChanges();

            if (_target.transform.hasChanged) OnChangedTransform();
        }

        private void OnDestroy()
        {
            if (_target != null) return;
            if (_targetTransform == null) return;
            if (Application.isPlaying) return;

            if (_duplicates == null) return;

            for (int i = _duplicates.Count - 1; i >= 0; i--)
            {
                DestroyImmediate(_duplicates[i]);
            }
        }
    }
}