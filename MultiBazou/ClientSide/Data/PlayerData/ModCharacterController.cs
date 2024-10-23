using System;
using UnityEngine;

namespace MultiBazou.ClientSide.Data.PlayerData
{
    public class ModCharacterController : MonoBehaviour
    {
        public float moveSpeed = 36f;
        public float acceleration = 50f;
        public float deceleration = 50f;
        public float arrivalThreshold = 0.1f;

        private Vector3 _velocity = Vector3.zero;
        private Vector3 _targetVelocity = Vector3.zero;
        private Vector3 _targetPosition;
        private bool _isMovingToTarget;
        
        private Quaternion _targetRotation;
        private bool _isRotationLocked;
        
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void UpdatePlayer()
        {
            if (_isMovingToTarget)
            {
                Vector3 directionToTarget = (_targetPosition - transform.position).normalized;
                float distanceToTarget = Vector3.Distance(transform.position, _targetPosition);

                if (distanceToTarget <= arrivalThreshold)
                {
                    _velocity = Vector3.zero;
                    _targetVelocity = Vector3.zero;
                    _isMovingToTarget = false;
                    return;
                }

                _targetVelocity = directionToTarget * moveSpeed;
            }

            _velocity = Vector3.Lerp(_velocity, _targetVelocity, (_targetVelocity.magnitude > 0.1f) ? acceleration * Time.deltaTime : deceleration * Time.deltaTime);

            transform.position = Vector3.Lerp(transform.position, transform.position + _velocity * Time.deltaTime, Time.deltaTime * 10f);
            
            if (_isRotationLocked)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, Time.deltaTime * 10f);
            }
            
            float verticalSpeed = Vector3.Dot(transform.forward, _velocity);
            float horizontalSpeed = Vector3.Dot(transform.right, _velocity);

            _animator.SetFloat("Vertical", Mathf.Lerp(_animator.GetFloat("Vertical"), verticalSpeed, Time.deltaTime * 10f));
            _animator.SetFloat("Horizontal", Mathf.Lerp(_animator.GetFloat("Horizontal"), horizontalSpeed, Time.deltaTime * 10f));
        }

        public void MoveToPosition(Vector3 position)
        {
            _targetPosition = position;
            _isMovingToTarget = true;
        }
        public void RotateToRotation( Quaternion rotation)
        {
            _targetRotation = rotation;
            _isRotationLocked = true;
        }
    }
}