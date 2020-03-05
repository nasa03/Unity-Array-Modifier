using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ArrayModifier
{
    [CustomEditor(typeof(ArrayModifier))]
    public class ArrayModifierEditor : Editor
    {
        private ArrayModifier _target;
        private bool _sceneWasDirtyLastTick = false;
        private bool _targetGOWasDirtyLastTick = false;

        private void Awake()
        {
            _target = target as ArrayModifier;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Refresh"))
            {
                _target.Calculate();
            }

            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();

            _target._fitType = (FitType)EditorGUILayout.EnumPopup(new GUIContent("Fit Type", "Determines which method to use for creating duplicates."), _target._fitType);

            if (_target._fitType == FitType.FixedCount)
            {
                _target._count = EditorGUILayout.DelayedIntField(new GUIContent("Count", "Determines how many duplicates will get created."), _target._count);
            }

            _target._constantOffset = EditorGUILayout.Vector3Field(new GUIContent("Constant Offset", "Determines the offset between each object in global space."), _target._constantOffset);
            _target._relativeOffset = EditorGUILayout.Vector3Field(new GUIContent("Relative Offset", "Determines the offset between each object in local space."), _target._relativeOffset);

            var valuesHaveBeenChanged = EditorGUI.EndChangeCheck();
            if (valuesHaveBeenChanged)
            {
                _target.Calculate();
            }
        }

        private void OnSceneGUI()
        {
            if (_target == null) return;

            CheckForSceneChanges();
            CheckForTargetGameObjectChanges();

            if (_target.transform.hasChanged) OnChangedTransform();
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

            var targetGOIsDirty = false;

            var components = _target.gameObject.GetComponents<MonoBehaviour>();
            foreach (var component in components)
            {
                if (EditorUtility.IsDirty(component))
                {
                    targetGOIsDirty = true;
                    break;
                }
            }

            if (targetGOIsDirty && !_targetGOWasDirtyLastTick)
            {
                OnTargetGOChanged();
            }

            _targetGOWasDirtyLastTick = targetGOIsDirty;
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

        private void OnDestroy()
        {
            if (_target != null) return;

            _target.RemoveDuplicates();
        }
    }
}