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

        private bool _isPrefab = false;
        private Vector3 _size = Vector3.one;
        private Collider _collider = null;
        private Collider2D _collider2D = null;
        private MeshRenderer _meshRenderer = null;
        private SpriteRenderer _spriteRenderer = null;

        private List<GameObject> _duplicates = new List<GameObject>();

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

            var prefab = Object.Instantiate(gameObject);
            GameObject previousDuplicate = null;
            
            for (int i = 0; i < _count - 1; i++)
            {
                var duplicateGameObject = Object.Instantiate(prefab);
                duplicateGameObject.hideFlags = HideFlags.HideInHierarchy;

                // Set position
                if (previousDuplicate == null)
                {
                    duplicateGameObject.transform.position = transform.position;
                }
                else 
                {
                    duplicateGameObject.transform.position = previousDuplicate.transform.position;
                }

                duplicateGameObject.transform.Translate(_constantOffset, Space.World);
                duplicateGameObject.transform.Translate(new Vector3(_relativeOffset.x * _size.x, _relativeOffset.y * _size.y, _relativeOffset.z * _size.z), Space.Self);

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

                previousDuplicate = duplicateGameObject;
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
            GetComponents();
            DetermineSize();
            RemoveDuplicates();

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