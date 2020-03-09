using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArrayModifier
{
    [ExecuteInEditMode]
    public class ArrayModifier : MonoBehaviour
    {
        [HideInInspector] public FitType _fitType = FitType.FixedCount;
        [HideInInspector] public int _count = 1;
        [HideInInspector] public Vector3 _constantOffset = Vector3.right;
        [HideInInspector] public Vector3 _relativeOffset = Vector3.zero;
        [HideInInspector] public float _fitLength = 5f;

        private bool _isPrefab = false;
        private Vector3 _size = Vector3.one;
        private Collider _collider = null;
        private Collider2D _collider2D = null;
        private MeshRenderer _meshRenderer = null;
        private SpriteRenderer _spriteRenderer = null;

        private List<GameObject> _duplicates = new List<GameObject>();
        private float _accumulatedLength = 0f;

        private const float MIN_SIZE = 0.02f;
        private const int MAX_COUNT = 1024;
        private const float MIN_MAGNITUDE = 0.01f;

        public List<GameObject> GetDuplicates() 
        {
            return _duplicates;
        }

        public void RemoveDuplicates()
        {
            foreach (var arrayModifier in GetComponents<ArrayModifier>())
            {
                var duplicates = arrayModifier.GetDuplicates();
                if (duplicates == null) continue;

                for (int i = duplicates.Count - 1; i >= 0; i--)
                {
                    if (duplicates[i] == null) continue;

                    try
                    {
                        DestroyImmediate(duplicates[i]);
                        duplicates.RemoveAt(i);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Index = " + i);
                        Debug.LogError(e.Message);
                        Debug.LogWarning("Array modifier prefabs are not supported for the time being.");
                        _isPrefab = true;
                        break;
                    }
                }
            }
        }

        public void InstantiateDuplicates(int arrayModifierIndex)
        {
            if (_isPrefab) return;
            if (transform.localScale.x == 0f || transform.localScale.y == 0f || transform.localScale.z == 0f) return;

                var prefab = Object.Instantiate(gameObject);

            Vector3 previousDuplicationPosition = transform.position;
            var relativeOffsetInWorldSpace = transform.TransformVector(_relativeOffset);
            Vector3 translation = _constantOffset + 
                new Vector3(relativeOffsetInWorldSpace.x * _size.x / transform.localScale.x, 
                            relativeOffsetInWorldSpace.y * _size.y / transform.localScale.y, 
                            relativeOffsetInWorldSpace.z * _size.z / transform.localScale.z);

            for (int i = 0; i < _count - 1 || _fitType == FitType.Length; i++)
            {
                if (_duplicates.Count >= MAX_COUNT) return;
                if (translation.magnitude <= MIN_MAGNITUDE) return;

                if (_fitType == FitType.Length && _accumulatedLength + translation.magnitude >= _fitLength) break;

                var duplicateGameObject = Object.Instantiate(prefab);
                duplicateGameObject.hideFlags = HideFlags.HideInHierarchy;

                duplicateGameObject.transform.position = previousDuplicationPosition;
                duplicateGameObject.transform.Translate(translation, Space.World);

                if (_fitType == FitType.Length)
                {
                    var distance = Vector3.Distance(duplicateGameObject.transform.position, previousDuplicationPosition);
                    _accumulatedLength += distance;
                }

                // Destroy all the array modifiers up to arrayModifierIndex
                for (int j = 0; j <= arrayModifierIndex; j++)
                {
                    var arrayModifierToDestroy = duplicateGameObject.GetComponents<ArrayModifier>().FirstOrDefault();
                    if (arrayModifierToDestroy == default(ArrayModifier)) break;
                    DestroyImmediate(arrayModifierToDestroy);
                }

                // Repeat this process on the duplicate if an array modifier still exists
                // on the duplicate
                var arrayModifier = duplicateGameObject.GetComponents<ArrayModifier>().FirstOrDefault();
                if (arrayModifier != default(ArrayModifier))
                {
                    arrayModifier.Calculate();
                }

                previousDuplicationPosition = duplicateGameObject.transform.position;

                _duplicates.Add(duplicateGameObject);
            }

            DestroyImmediate(prefab);
        }

        public void Calculate()
        {
            StartCoroutine(CalculateCoroutine());
        }

        private IEnumerator CalculateCoroutine() 
        {
            if (_relativeOffset.sqrMagnitude + _constantOffset.sqrMagnitude <= 0f) yield break;

            GetComponents();
            DetermineSize();

            if (_fitType == FitType.Length && (_size.magnitude <= MIN_SIZE || transform.localScale.magnitude <= MIN_SIZE)) yield break;

            RemoveDuplicates();
            _accumulatedLength = 0f;

            var arrayModifiers = GetComponents<ArrayModifier>();
            for (int i = 0; i < arrayModifiers.Length; i++)
            {
                arrayModifiers[i].InstantiateDuplicates(i);
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

        private void OnDestroy()
        {
            RemoveDuplicates();
        }
    }
}