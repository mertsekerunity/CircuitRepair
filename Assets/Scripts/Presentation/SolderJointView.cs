using System;
using UnityEngine;

namespace CircuitRepair.Presentation
{
    public sealed class SolderJointView : MonoBehaviour
    {
        [SerializeField] private GameObject jointVisual;

        private void Awake()
        {
            ValidateReferences();
            jointVisual.SetActive(false);
        }

        public void SetConnected(bool isConnected)
        {
            jointVisual.SetActive(isConnected);
        }

        private void ValidateReferences()
        {
            if (jointVisual == null)
            {
                throw new InvalidOperationException($"{nameof(jointVisual)} is not assigned in the Inspector.");
            }
        }
    }
}