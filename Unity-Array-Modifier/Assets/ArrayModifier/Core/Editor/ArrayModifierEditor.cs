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

            _target._fitType = (FitType)EditorGUILayout.EnumPopup("Fit Type", _target._fitType);

            if (_target._fitType == FitType.FixedCount)
            {
                _target._count = EditorGUILayout.DelayedIntField(_target._count);
            }

            _target._constantOffset = EditorGUILayout.Vector3Field("Constant Offset", _target._constantOffset);

            var valuesHaveBeenChanged = EditorGUI.EndChangeCheck();
            if (valuesHaveBeenChanged)
            {
                UpdateArrayModifier();
            }
        }

        private void UpdateArrayModifier()
        {
            if (_target._isDuplicate) return;

            _target.RemoveDuplicates();
            _target.InstantiateDuplicates();
        }
    }
}