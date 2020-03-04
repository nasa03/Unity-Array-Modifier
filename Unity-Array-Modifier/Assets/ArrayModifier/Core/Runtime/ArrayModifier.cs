using System.Collections.Generic;
using UnityEngine;

namespace ArrayModifier
{
    public class ArrayModifier : MonoBehaviour
    {
        [HideInInspector] public FitType _fitType = FitType.FixedCount;
        [HideInInspector] public int _count = 1;
        [HideInInspector] public Vector3 _constantOffset = Vector3.right;
        [HideInInspector] public Vector3 _relativeOffset = Vector3.zero;

        private List<GameObject> _duplicates = new List<GameObject>();

        public void RemoveDuplicates()
        {
            if (_duplicates == null) return;

            foreach (var duplicate in _duplicates)
            {
                if (duplicate == null) continue;

                DestroyImmediate(duplicate);
            }

            _duplicates.Clear();
        }

        public void UpdateArrayModifier()
        {
            RemoveDuplicates();
            InstantiateDuplicates();
        }

        public void InstantiateDuplicates()
        {
            var prefab = Object.Instantiate(gameObject);

            for (int i = 1; i < _count; i++)
            {
                var duplicate = Object.Instantiate(prefab);

                var constantOffsetVector = _constantOffset * (float)i;

                var relativeOffsetVector = transform.right * _relativeOffset.x + transform.up * _relativeOffset.y + transform.forward * _relativeOffset.z;
                relativeOffsetVector *= (float)i;

                duplicate.transform.position = transform.position + constantOffsetVector + relativeOffsetVector;
                duplicate.transform.SetParent(transform, true);

                _duplicates.Add(duplicate);
            }

            DestroyImmediate(prefab);
        }

        private void OnDestroy()
        {
            RemoveDuplicates();
        }

        private void OnDisable()
        {
            RemoveDuplicates();
        }
    }
}