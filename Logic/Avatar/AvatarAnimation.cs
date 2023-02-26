using UnityEngine;

namespace NekoNeko.Avatar
{
    public class AvatarAnimation : MonoBehaviour
    {
        private int[] StateParams;
        private int IsIdle = Animator.StringToHash("IsIdle");
        private int IsMoving = Animator.StringToHash("IsMoving");
        private int IsWalking = Animator.StringToHash("IsWalking");
        private int IsRunning = Animator.StringToHash("IsRunning");
        private int IsSprinting = Animator.StringToHash("IsSprinting");

        private int HasMoveInput = Animator.StringToHash("HasMoveInput");
        private int InputSpeed = Animator.StringToHash("InputSpeed");
        private int MoveSpeed = Animator.StringToHash("MoveSpeed");
        private int MoveBlend = Animator.StringToHash("MoveBlend");
        private int SmoothedMoveBlend = Animator.StringToHash("SmoothedMoveBlend");
        private int MoveDirectionDot = Animator.StringToHash("MoveDirectionDot");
        private int MoveDirectionCross = Animator.StringToHash("MoveDirectionCross");
        private int UseRootMotion = Animator.StringToHash("UseRootMotion");
        private int Foot = Animator.StringToHash("Foot");

        [SerializeField] private Animator _animator;
        [SerializeField] private float _moveBlendSmoothSpeed = 10f;
        [SerializeField] private float _walkMoveBlend = 0f;
        [SerializeField] private float _runMoveBlend = 1f;
        [SerializeField] private float _sprintMoveBlend = 2f;
        [SerializeField] private Transform _leftFoot;
        [SerializeField] private Transform _rightFoot;
        [SerializeField][Range(0f, 1f)] private float _leftFootNormalizedTime = 0f;
        [SerializeField][Range(0f, 1f)] private float _rightFootNormalizedTime = 0.5f;

        private float _smoothedMoveBlend = 0f;
        private float _footNormalizedTimeCycle;
        private bool _isLeftFootOddCycle;

        public AvatarData Data { get; set; }
        public AvatarInput Input { get; set; }

        private void Awake()
        {
            StateParams = new int[] { IsIdle, IsMoving, IsWalking, IsRunning, IsSprinting };
            _footNormalizedTimeCycle = Mathf.Abs(_rightFootNormalizedTime - _leftFootNormalizedTime);
            if (_leftFootNormalizedTime < _rightFootNormalizedTime) _isLeftFootOddCycle = true;
            else _isLeftFootOddCycle = false;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_animator == null) return;
            EvaluateLocomotionState(Data.MovementState, deltaTime);
            _animator.SetBool(HasMoveInput, Data.HasMovementInput);
            _animator.SetFloat(MoveSpeed, Data.LastNonZeroMoveSpeed);
            _animator.SetFloat(InputSpeed, Data.LastNonZeroInputSpeed);
            _animator.SetFloat(MoveDirectionDot, Data.MoveDirectionDot);
            _animator.SetFloat(MoveDirectionCross, Data.MoveDirectionCross.y);
        }

        #region State

        private void EvaluateLocomotionState(AvatarData.MovementStateType state, float deltaTime)
        {
            ClearStateParams();
            switch (state)
            {
                case AvatarData.MovementStateType.Idle:
                    _animator.SetBool(IsIdle, true);
                    _animator.SetFloat(SmoothedMoveBlend, _smoothedMoveBlend);
                    break;
                case AvatarData.MovementStateType.Run:
                    _animator.SetBool(IsMoving, true);
                    _animator.SetBool(IsRunning, true);

                    _smoothedMoveBlend = Mathf.Lerp(_smoothedMoveBlend, _runMoveBlend, deltaTime * _moveBlendSmoothSpeed);
                    _animator.SetFloat(MoveBlend, _runMoveBlend);
                    _animator.SetFloat(SmoothedMoveBlend, _smoothedMoveBlend);
                    EvaluateFootPosition();
                    break;
                case AvatarData.MovementStateType.Sprint:
                    _animator.SetBool(IsMoving, true);
                    _animator.SetBool(IsSprinting, true);

                    _smoothedMoveBlend = Mathf.Lerp(_smoothedMoveBlend, _sprintMoveBlend, deltaTime * _moveBlendSmoothSpeed);
                    _animator.SetFloat(MoveBlend, _sprintMoveBlend);
                    _animator.SetFloat(SmoothedMoveBlend, _smoothedMoveBlend);
                    EvaluateFootPosition();
                    break;
                case AvatarData.MovementStateType.Walk:
                    _animator.SetBool(IsMoving, true);
                    _animator.SetBool(IsWalking, true);
                    _animator.SetBool(UseRootMotion, true);

                    _smoothedMoveBlend = Mathf.Lerp(_smoothedMoveBlend, _walkMoveBlend, deltaTime * _moveBlendSmoothSpeed);
                    _animator.SetFloat(MoveBlend, _walkMoveBlend);
                    _animator.SetFloat(SmoothedMoveBlend, _smoothedMoveBlend);
                    EvaluateFootPosition();
                    break;
                case AvatarData.MovementStateType.RootMotion:
                    _animator.SetBool(IsMoving, true);
                    break;
            }
        }

        private void ClearStateParams()
        {
            for (int i = 0; i < StateParams.Length; i++)
            {
                _animator.SetBool(StateParams[i], false);
            }
            _animator.SetBool(UseRootMotion, false);
        }

        #endregion

        private void EvaluateFootPosition()
        {
            _animator.SetFloat(Foot, GetFootByTime());
        }

        private float GetFootByTime()
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = stateInfo.normalizedTime;
            float cycleCount = Mathf.Ceil(normalizedTime / _footNormalizedTimeCycle);
            return GetFootFromCycle(cycleCount);
        }

        /// <summary>
        /// Returns foot id from cycle count.
        /// Left foot is 0, right foot is 1.
        /// </summary>
        /// <param name="cycleCount"></param>
        /// <returns></returns>
        private float GetFootFromCycle(float cycleCount)
        {
            if (_isLeftFootOddCycle) return cycleCount % 2f > 0 ? 0f : 1f;
            else return cycleCount % 2f > 0 ? 1f : 0f;
        }
    }
}
