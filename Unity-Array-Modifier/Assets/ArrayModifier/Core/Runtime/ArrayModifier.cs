using System.Collections.Generic;
using UnityEngine;

namespace ArrayModifier
{
    public class ArrayModifier : MonoBehaviour
    {
        [HideInInspector] public FitType _fitType = FitType.FixedCount;
        [HideInInspector] public int _count = 1;
        [HideInInspector] public Vector3 _constantOffset = Vector3.one;

        public bool _isDuplicate = false;
        [HideInInspector] public List<GameObject> _duplicates = new List<GameObject>();

        private void OnDestroy()
        {
            RemoveDuplicates();
        }

        public void RemoveDuplicates()
        {
            if (_isDuplicate) return;

            foreach (var duplicate in _duplicates)
            {
                DestroyImmediate(duplicate);
            }

            _duplicates.Clear();
        }

        public void InstantiateDuplicates()
        {
            if (_isDuplicate) return;

            for (int i = 1; i < _count; i++)
            {
                var duplicate = Object.Instantiate(gameObject);

                var arrayModifier = duplicate.GetComponent<ArrayModifier>();
                arrayModifier.RemoveDuplicates();
                DestroyImmediate(arrayModifier);

                duplicate.transform.position = transform.position + _constantOffset * (float)i;
                duplicate.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable | HideFlags.HideInInspector;
                duplicate.transform.SetParent(transform, true);
                
                _duplicates.Add(duplicate);
            }
        }
    }
}