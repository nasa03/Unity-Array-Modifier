using UnityEngine;
using UnityEditor;

namespace ArrayModifier
{
    [CustomEditor(typeof(ArrayModifier))]
    public class ArrayModifierEditor : Editor
    {
        private ArrayModifier _target;

        private void Awake()
        {
            _target = target as ArrayModifier;
        }

        public override void OnInspectorGUI()
        {
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
                Calculate();
            }
        }

        private void Calculate() 
        {
            for (int i = _target.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(_target.transform.GetChild(i).gameObject);
            }

            var arrayModifiers = _target.GetComponents<ArrayModifier>();

            Transform lastArrayModifierTransform = null;

            foreach (var arrayModifier in arrayModifiers)
            {
                arrayModifier.InstantiateDuplicates();

                if (lastArrayModifierTransform != null)
                {
                    arrayModifier.transform.SetParent(lastArrayModifierTransform, true);
                }

                lastArrayModifierTransform = arrayModifier.transform;
            }
        }

        private void OnSceneGUI()
        {
            if (_target.transform.hasChanged) OnChangedTransform();
        }

        private void OnChangedTransform()
        { 
            Calculate();
            _target.transform.hasChanged = false;
        }

        private void OnDestroy()
        {
            if (_target != null) return;

            _target.RemoveDuplicates();
        }
    }
}