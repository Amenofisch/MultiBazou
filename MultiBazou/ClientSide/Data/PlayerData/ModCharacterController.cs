using System;
using UnityEngine;

namespace MultiBazou.ClientSide.Data.PlayerData
{
    public class ModCharacterController : MonoBehaviour
    {
        public float moveSpeed = 50f;
        public float acceleration = 25f;
        public float deceleration = 35f;
        public float arrivalThreshold = 0.02f;

        private Vector3 _velocity = Vector3.zero;
        private Vector3 _targetVelocity = Vector3.zero;
        private Vector3 _targetPosition;
        private bool _isMovingToTarget;
        
        private Quaternion _targetRotation;
        
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void UpdatePlayer()
        {
            if (_isMovingToTarget)
            {
                var directionToTarget = (_targetPosition - transform.position).normalized;
                var distanceToTarget = Vector3.Distance(transform.position, _targetPosition);

                if (distanceToTarget <= arrivalThreshold)
                {
                    _velocity = Vector3.zero;
                    _targetVelocity = Vector3.zero;
                    _isMovingToTarget = false;
                    return;
                }

                _targetVelocity = directionToTarget * moveSpeed;
            }

            _velocity = Vector3.Lerp(_velocity, _targetVelocity, _targetVelocity.magnitude > 0.1f ? acceleration * Time.deltaTime : deceleration * Time.deltaTime);

            transform.position = Vector3.Lerp(transform.position, transform.position + _velocity * Time.deltaTime, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, Time.deltaTime * 10f);
            
            var verticalSpeed = Vector3.Dot(transform.forward, _velocity);
            var horizontalSpeed = Vector3.Dot(transform.right, _velocity);

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
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }
    }
}