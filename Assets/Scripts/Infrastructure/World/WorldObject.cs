using System;
using System.Linq;
using Reflex.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Infrastructure.World
{
    public class WorldObject : MonoBehaviour, IWorldObject
    {
        [SerializeField]
        private string id;

        public Guid Id => Guid.Parse(id);
        public GameObject Value => gameObject;

        [Inject]
        private void Construct(IWorldService worldService)
        {
            worldService.Register(this);
        }

        [Button]
        private void GenerateId()
        {
            id = Guid.NewGuid().ToString();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                return;
            }

            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            WorldObject[] allWorldObjects = FindObjectsOfType<WorldObject>(true);
            WorldObject duplicate = allWorldObjects.FirstOrDefault(obj => obj != this && obj.id == id);

            if (duplicate != null)
            {
                Debug.LogError($"Duplicate WorldObject ID detected! Object '{gameObject.name}' has the same ID as '{duplicate.gameObject.name}'. Please generate a new ID.", this);
            }
        }
#endif
    }
}
