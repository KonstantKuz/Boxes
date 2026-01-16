using System;
using ObservableCollections;
using R3;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Interactable.BoxesInteraction.Components
{
    [RequireComponent(typeof(BoxesStorage))]
    public class BoxesStorageStateEvent : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent onIsEmpty;

        [SerializeField]
        private UnityEvent onIsFull;

        private BoxesStorage boxesStorage;
        private int storageCapacity;
        private IDisposable stateSubscription;

        private void Awake()
        {
            boxesStorage = GetComponent<BoxesStorage>();
        }

        private void OnEnable()
        {
            storageCapacity = boxesStorage.Boxes.Count;
            stateSubscription = boxesStorage.Boxes.ObserveCountChanged().Subscribe(OnCountChanged);
        }

        private void OnCountChanged(int count)
        {
            if (count == storageCapacity)
            {
                onIsFull.Invoke();
            }

            if (count == 0)
            {
                onIsEmpty.Invoke();
            }
        }

        private void OnDisable()
        {
            stateSubscription?.Dispose();
        }
    }
}
