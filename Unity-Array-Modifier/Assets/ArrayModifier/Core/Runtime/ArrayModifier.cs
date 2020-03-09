using System.Collections;
using System.Linq;
using UnityEngine;

namespace ArrayModifier
{
    public class ArrayModifier : MonoBehaviour
    {
        [HideInInspector] public FitType _fitType = FitType.FixedCount;
        [HideInInspector] public int _count = 1;
        [HideInInspector] public Vector3 _constantOffset = Vector3.right;
        [HideInInspector] public Vector3 _relativeOffset = Vector3.zero;

        private bool _isPrefab = false;
        private Vector3 _size = Vector3.one;
        private Collider _collider = null;
        private Collider2D _collider2D = null;
        private MeshRenderer _meshRenderer = null;
        private SpriteRenderer _spriteRenderer = null;

        public void RemoveDuplicates()
        {
            var duplicates = transform.GetComponentsInChildren<Duplicate>();
            if (duplicates == null) return;

            foreach (var duplicate in duplicates)
            {
                if (duplicate == null) continue;

                try
                {
                    DestroyImmediate(duplicate.gameObject);
                }
                catch
                {
                    Debug.LogWarning("Array modifier prefabs are not supported for the time being.");
                    _isPrefab = true;
                    break;
                }
            }
        }

        public void InstantiateDuplicates()
        {
            if (_isPrefab) return;

            var prefab = Object.Instantiate(gameObject);

            for (int i = 1; i < _count; i++)
            {
                var duplicate = Object.Instantiate(prefab);

                var constantOffsetVector = _constantOffset * (float)i;
                var relativeOffsetVector = transform.TransformVector(new Vector3(_relativeOffset.x * _size.x, _relativeOffset.y * _size.y, _relativeOffset.z * _size.z) * (float)i);

                duplicate.transform.position = transform.position + constantOffsetVector + relativeOffsetVector;
                duplicate.transform.SetParent(transform, true); // TODO: Remove the parent relationship, since the local scale of the 
                                                                // parent shouldn't affect the object being duplicated

                duplicate.AddComponent<Duplicate>();
            }

            DestroyImmediate(prefab);
        }

        public void Calculate()
        {
            StartCoroutine(CalculateCoroutine());
        }

        private IEnumerator CalculateCoroutine() 
        {
            GetComponents();
            DetermineSize();
            RemoveDuplicates();

            var arrayModifiers = GetComponents<ArrayModifier>();

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

            foreach (var arrayModifier in arrayModifiers)
            {
                if (arrayModifier == null) continue;

                var childArrayModifiers = arrayModifier.GetComponentsInChildren<ArrayModifier>()
                    .Where(a => a.gameObject != this.gameObject);

                foreach (var childArrayModifier in childArrayModifiers)
                {
                    childArrayModifier.gameObject.hideFlags = HideFlags.NotEditable | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                    DestroyImmediate(childArrayModifier);
                }

                yield return null;
            }

            yield return null;
        }

        private void GetComponents() 
        {
            if (_meshRenderer == null) GetComponent<MeshRenderer>();
            if (_collider == null) GetComponent<Collider>();
            if (_spriteRenderer == null) GetComponent<SpriteRenderer>();
            if (_collider2D == null) GetComponent<Collider2D>();
        }

        private void DetermineSize() 
        {
            if (_meshRenderer != null)
            {
                _size = _meshRenderer.bounds.size;
            }
            else if (_spriteRenderer != null)
            {
                _size = _spriteRenderer.bounds.size;
            }
            else if (_collider != null)
            {
                _size = _collider.bounds.size;
            }
            else if (_collider2D != null)
            {
                _size = _collider2D.bounds.size;
            }
            else
            {
                _size = transform.localScale;
            }
        }
    }
}