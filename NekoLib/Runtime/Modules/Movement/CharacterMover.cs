using System;
using System.Collections.Generic;
using UnityEngine;

namespace NekoLib.Movement
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class CharacterMover : MonoBehaviour, ICharacterMover
    {
        public const float kMaxSpeedChange = 300f;

        public enum VelocityPhysicsMode
        {
            None,
            Simple,
            Advanced,
        }

        #region Exposed Fields

        [Header("Collider Dimensions")]
        [SerializeField][Min(0f)] private float _height = 2f;
        [SerializeField][Min(0f)] private float _thickness = 1f;
        [SerializeField] private Vector3 _colliderOffset = Vector3.zero;

        [Header("Velocity Physics")]
        [Tooltip("Defines how we handle acceleration, deceleration, friction for active velocity.\n" +
            "None -- Bypass velocity physics calculations.\n" + "Simple -- Use Speed Change.\n" + "Advanced -- Use Velocity Config.")]
        [SerializeField] private VelocityPhysicsMode _velocityMode = VelocityPhysicsMode.Simple;
        [SerializeField][Range(0f, kMaxSpeedChange)] private float _speedChange = 50f;
        [SerializeField] private VelocityConfig _velocityConfig = new VelocityConfig();

        [Header("Gravity")]
        [SerializeField] private bool _enableGravity = true;
        [SerializeField][Min(0f)] private float _gravityAccel = 20f;
        [SerializeField][Min(0f)] private float _maxFallSpeed = 20f;

        [Header("Ground Detection")]
        [SerializeField] private LayerMask _groundLayer;
        [Tooltip("Snap to ground when grounded. Useful for cases like moving down stairs.")]
        [SerializeField] private bool _shouldSnapToGround = true;
        [Tooltip("Surfaces with normal below this angle is considered as ground.")]
        [SerializeField][Range(0f, 90f)] private float _groundAngleLimit = 90f;
        [Tooltip("Surfaces with normal below this angle is considered as flat ground.")]
        [SerializeField][Range(0f, 90f)] private float _flatGroundAngleLimit = 60f;
        [SerializeField][Min(0f)] private float _groundProbeRange = 10f;
        [SerializeField][Min(0f)] private float _groundProbeThickness = 0.1f;
        [Tooltip("Minimum extra ground threshold distance for snapping to ground. The greater value between step height and this value will be used.")]
        [SerializeField][Min(0f)] private float _minExtraGroundThreshold = 0.25f;
        [Tooltip("Factor to multiply by desired ground distance to account for floating point errors.")]
        [SerializeField][Min(0)] private float _groundCheckToleranceFactor = 0.01f;
        [Tooltip("If true, performs predictive ground probing and stops horizontal velocity if the final velocity will cause the character to leave ground.")]
        [SerializeField] private bool _restrictToGround = false;

        [Header("Step Traversal")]
        [SerializeField][Min(0f)] private float _stepHeight = 0.3f;
        [SerializeField][Min(1f)] private float _stepUpSmooth = 10f;
        [SerializeField][Min(1f)] private float _stepDownSmooth = 10f;
        [SerializeField] private bool _useRealGroundNormal = true;
        [Tooltip("If true, performs predictive ground probing to adjust floating step velocity more responsively.")]
        [SerializeField] private bool _predictGroundWhenFalling = true;

        [Header("Slope Detection")]
        [SerializeField] private bool _useSlopeProbing = true;
        [SerializeField][Min(0f)] private float _slopeProbeFrontOffset = 1f;
        [SerializeField][Range(1, 5)] private int _slopeProbeFrontCount = 2;
        [SerializeField][Min(0f)] private float _slopeProbeBackOffset = 1f;
        [SerializeField][Range(1, 5)] private int _slopeProbeBackCount = 2;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showVelocityDebug = false;
        [SerializeField] private bool _showGroundProbingDebug = false;
        [SerializeField] private bool _showSlopeDebug = false;
        [SerializeField] private bool _showContactDebug = false;
#endif

        #endregion

        #region Properties

        public bool IsOnGround { get; set; }
        public bool IsOnFlatGround { get; set; }
        public Vector3 GroundNormal { get; set; }
        public Vector3 GroundPoint { get; set; }
        public Collider GroundCollider { get; set; }
        public Vector3 SlopeNormal { get; set; }
        public float ActualSpeed {
            get {
                if (_actualSpeedIsDirty)
                {
                    _actualSpeed = Rigidbody.velocity.magnitude;
                    _actualSpeedIsDirty = false;
                }
                return _actualSpeed;
            }
        }
        public Vector3 ActualDirection {
            get {
                if (_actualDirectionIsDirty)
                {
                    _actualDirection = Rigidbody.velocity.normalized;
                    _actualDirectionIsDirty = false;
                }
                return _actualDirection;
            }
        }

        public Rigidbody Rigidbody { get; private set; }
        public Collider Collider { get; private set; }
        public Vector3 ColliderCenter { get => _capsuleCollider.bounds.center; }
        public GroundSensor GroundSensor { get; private set; }

        /// <summary>
        /// Capsule collider height.
        /// </summary>
        public float Height { get => _height; set { _height = value; UpdateColliderHeight(); } }
        /// <summary>
        /// Capsule collider thickness is equal to 2 times its radius.
        /// </summary>
        public float Thickness { get => _thickness; set { _thickness = value; UpdateColliderRadius(); } }
        public Vector3 Velocity { get => Rigidbody.velocity; }
        public VelocityConfig VelocityConfig { get => _velocityConfig; set => _velocityConfig = value; }

        public bool ShouldSnapToGround { get => _shouldSnapToGround; set => _shouldSnapToGround = value; }
        public float GroundAngleLimit {
            get => _groundAngleLimit;
            set {
                _groundAngleLimit = value;
                _minGroundAngleDot = Mathf.Cos(value * Mathf.Deg2Rad);
            }
        }
        public float FlatGroundAngleLimit {
            get => _flatGroundAngleLimit;
            set {
                _flatGroundAngleLimit = value;
                _minFlatGroundAngleDot = Mathf.Cos(value * Mathf.Deg2Rad);
            }
        }
        public float GroundProbeRange { get => _groundProbeRange; set => _groundProbeRange = value; }

        /// <summary>
        /// If true, extend desired ground distance by step height to snap to ground when moving downstairs or slopes.
        /// </summary>
        public bool UseExtraGroundThresholdDistance {
            get => _useExtraGroundThresholdDistance;
            set {
                _useExtraGroundThresholdDistance = value;
                UpdateGroundSensorThresholdDistance();
            }
        }
        /// <summary>
        /// Extra ground threshold distance used to help snap to ground when moving downstairs or slopes.
        /// </summary>
        public float ExtraGroundThresholdDistance {
            get => _extraGroundThresholdDistance;
            set {
                _extraGroundThresholdDistance = value;
                UpdateGroundSensorThresholdDistance();
            }
        }
        /// <summary>
        /// Desired ground distance to maintain based on step height.
        /// </summary>
        public float DesiredGroundDistance {
            get => _desiredGroundDistance;
            set {
                _desiredGroundDistance = value;
                UpdateGroundSensorThresholdDistance();
            }
        }

        #endregion

        #region Fields

        #region Cache Fields

        // Convenience cache for collider.
        private CapsuleCollider _capsuleCollider;
        private float _capsuleHalfHeight;
        // Points with ground normal dot larger than this value is considered as ground.
        private float _minGroundAngleDot;
        // Points with ground normal dot larger than this value is considered as flat ground.
        private float _minFlatGroundAngleDot;

        /// <summary>
        /// Ongoing collisions. Should be cleared at the end of each fixed update.
        /// </summary>
        private List<Collision> _collisions = new List<Collision>();
        private List<IVelocitySource> _velocitySources = new List<IVelocitySource>();
        private List<Impulse> _impulses = new List<Impulse>();

        private float _actualSpeed;
        private bool _actualSpeedIsDirty = true;
        private Vector3 _actualDirection;
        private bool _actualDirectionIsDirty = true;

        #endregion

        #region Velocity Fields

        #region Active Velocity Cache

        private Vector3 _activeVelocityTarget = Vector3.zero;
        // Active velocity, driven by input and affected by velocity physics.
        private Vector3 _activeVelocity = Vector3.zero;
        // Previous nonzero active velocity direction.
        private Vector3 _lastNonZeroActiveDirection = Vector3.zero;

        #endregion

        #region Passive, Extra, Override Velocity Cache

        // Passive velocity.
        private Vector3 _passiveVel = Vector3.zero;

        /// <summary>
        /// Extra velocity.
        /// <para>Cleared at the end of every fixed update.</para>
        /// </summary>
        private Vector3 _extraVel = Vector3.zero;

        /// <summary>
        /// Override velocity, overrides other velocities, bypassing all velocity physics.
        /// <para>Cleared at the end of every fixed update.</para>
        /// </summary>
        private Vector3 _overrideVel = Vector3.zero;
        private bool _hasOverrideVel = false;

        #endregion

        // Gravity speed;
        private float _gravitySpeed;

        // Velocity contribution from connected body.
        private Vector3 _connectedBodyVel = Vector3.zero;
        private Vector3 _connectedBodyPosDelta = Vector3.zero;

        // Ground step height velocity;
        private Vector3 _groundStepVel = Vector3.zero;

        #endregion

        #region Ground Fields

        // Ground state.
        private bool _wasOnGround = false;
        private bool _hasGroundStateChanged = false;
        private bool _isTouchingGround = false;
        private bool _isTouchingCeiling = false;

        // Ground contact.
        private int _groundCollisionCount = 0;
        private Vector3 _prevConnectionPos;

        // Ground probing.
        private bool _useExtraGroundThresholdDistance = false;
        // Extra ground threshold distance for maintaining grounded when going down stairs or slopes.
        private float _extraGroundThresholdDistance;
        // Desired ground distance from collider center.
        private float _desiredGroundDistance;
        // Ground probe range.
        private float _totalGroundProbeRange;

        // Slope probing.
        private List<Vector3> _slopePoints = new List<Vector3>();

        #endregion

        #endregion

        #region Events
        public event Action GainedGroundContact = delegate { };
        public event Action LostGroundContact = delegate { };
        #endregion

        #region MonoBehaviour

        private void OnValidate()
        {
            AddComponents();
            UpdateColliderDimensions();
            _minFlatGroundAngleDot = Mathf.Cos(_flatGroundAngleLimit * Mathf.Deg2Rad);
        }

        private void Awake()
        {
            OnValidate();
            UseExtraGroundThresholdDistance = true;
            GroundNormal = SlopeNormal = Vector3.up;
        }

        private void FixedUpdate()
        {
#if UNITY_EDITOR
            DrawContactNormals();
#endif

            // Update collisions and detect ground.
            UpdateCollisions();
            // Update ground state and invoke events.
            UpdateGroundState(IsOnGround);
            // Update movement.
            UpdateMovement(Time.deltaTime);

            // Mark properties for lazy update.
            _actualSpeedIsDirty = true;
            _actualDirectionIsDirty = true;

            // Clean up frame.
            _collisions.Clear();
            _extraVel = Vector3.zero;
            _overrideVel = Vector3.zero;
            _hasOverrideVel = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            _collisions.Add(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            _collisions.Add(collision);
        }

        #endregion

        #region Initialization

        private void AddComponents()
        {
            TryGetComponent(out Rigidbody rb);
            Rigidbody = rb;
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.freezeRotation = true;

            TryGetComponent(out CapsuleCollider capsuleCollider);
            Collider = capsuleCollider;
            _capsuleCollider = (CapsuleCollider)Collider;

            if (GroundSensor == null) GroundSensor = new GroundSensor();
        }

        #endregion

        #region Movement Methods

        // Internal method for moving. Sets rigidbody velocity.
        private void Move(Vector3 velocity)
        {
            if (ShouldSnapToGround) Rigidbody.velocity = velocity + _groundStepVel;
            else Rigidbody.velocity = velocity;
        }

        /// <summary>
        /// Directly move by the specified delta position.
        /// </summary>
        /// <param name="deltaPosition"></param>
        /// <param name="alignToGround"></param>
        /// <param name="restictToGround"></param>
        public void MoveDeltaPosition(Vector3 deltaPosition,
            bool alignToGround = true, bool restictToGround = false)
        {
            if (alignToGround)
            {
                deltaPosition = IsOnGround ? AlignVelocityToNormal(deltaPosition, SlopeNormal) : deltaPosition;
            }

            if (restictToGround)
            {
                GroundInfo predictedGroundInfo;
                bool willBeOnGround = PredictProbeGround(out predictedGroundInfo, deltaPosition);
                if (!willBeOnGround) return;
            }

            Rigidbody.MovePosition(Rigidbody.position + deltaPosition);
        }

        #region Input Move

        /// <summary>
        /// Move by setting the active velocity target using the specified input speed and direction.
        /// <para>Active velocity undergoes velocity physics calculations
        /// to reach the active velocity target.</para>
        /// </summary>
        /// <param name="inputSpeed"></param>
        /// <param name="inputDirection"></param>
        public void InputMove(float inputSpeed, Vector3 inputDirection)
        {
            InputMove(inputSpeed * inputDirection);
        }

        /// <summary>
        /// Move by setting the active velocity target.
        /// <para>Active velocity undergoes velocity physics calculations
        /// to reach the active velocity target.</para>
        /// </summary>
        /// <param name="velocity"></param>
        public void InputMove(Vector3 velocity)
        {
            _activeVelocityTarget = velocity;
        }

        /// <summary>
        /// Set the active velocity.
        /// <para>Active velocity will persist across fixed updates,
        /// and undergoes velocity physics calculations.</para>
        /// </summary>
        /// <param name="velocity"></param>
        public void SetActiveVelocity(Vector3 velocity)
        {
            _activeVelocity = velocity;
            _activeVelocityTarget = velocity;
        }

        public void ClearActiveVelocity()
        {
            _activeVelocity = Vector3.zero;
        }

        #endregion

        #region Velocities

        /// <summary>
        /// Set the passive velocity.
        /// <para>Passive velocity will persist across fixed updates.</para>
        /// </summary>
        /// <param name="velocity"></param>
        public void SetPassiveVelocity(Vector3 velocity)
        {
            _passiveVel = velocity;
        }

        public void ClearPassiveVelocity()
        {
            _passiveVel = Vector3.zero;
        }

        /// <summary>
        /// Set the extra velocity for one physics update.
        /// <para>Extra velocity will be cleared at the end of the physics update.</para>
        /// </summary>
        /// <param name="velocity"></param>
        public void AddExtraVelocity(Vector3 velocity)
        {
            _extraVel += velocity;
        }

        public void ClearExtraVelocity()
        {
            _extraVel = Vector3.zero;
        }

        /// <summary>
        /// Move by setting the override velocity for one physics frame.
        /// <para>Overrides active velocity and velocity physics calculations.
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="clearActiveVelocity"></param>
        /// <param name="ignoreConnectedGround"></param>
        public void SetOverrideVelocity(Vector3 velocity,
            bool clearActiveVelocity = false, bool ignoreConnectedGround = false)
        {
            if (ignoreConnectedGround)
            {
                _overrideVel = velocity;
            }
            else
            {
                _overrideVel = velocity + _connectedBodyVel;
            }
            _hasOverrideVel = true;
            if (clearActiveVelocity) ClearActiveVelocity();
        }

        #endregion

        #region Velocity Sources

        public void AddVelocitySource(IVelocitySource velocitySource)
        {
            if (!_velocitySources.Contains(velocitySource))
            {
                _velocitySources.Add(velocitySource);
            }
        }

        public void RemoveVelocitySource(IVelocitySource velocitySource)
        {
            _velocitySources.Remove(velocitySource);
        }

        public Vector3 UpdateVelocitySources(float deltaTime)
        {
            Vector3 totalVelocity = Vector3.zero;
            for (int i = 0; i < _velocitySources.Count; i++)
            {
                IVelocitySource velocitySource = _velocitySources[i];
                Vector3 velocity = velocitySource.Evaluate(deltaTime);
                totalVelocity += velocity;
            }
            return totalVelocity;
        }

        #endregion

        #region Impulses

        public void AddImpulse(Impulse impulse)
        {
            if (!_impulses.Contains(impulse))
            {
                _impulses.Add(impulse);
            }
            else impulse.Start();
        }

        public void RemoveImpulse(Impulse impulse)
        {
            _impulses.Remove(impulse);
        }

        private Vector3 UpdateImpulses(float deltaTime)
        {
            Vector3 impulseVel = Vector3.zero;
            for (int i = _impulses.Count - 1; i >= 0; i--)
            {
                Impulse impulse = _impulses[i];
                Vector3 thisImpulseVel = impulse.Evaluate(deltaTime);

                // If flagged as use max speed,
                // limit this impulse speed so the final result not exceed its max speed when combined with the active velocity.
                if (impulse.SpeedModeFlag == Impulse.SpeedMode.Max)
                {
                    thisImpulseVel = Vector3.ClampMagnitude(thisImpulseVel, impulse.MaxSpeed - _activeVelocity.magnitude);
                }
                // If flagged as align to ground.
                if (impulse.AlignToGroundFlag == true)
                {
                    thisImpulseVel = AlignVelocityToNormal(thisImpulseVel, SlopeNormal);
                }
                // If flagged as leave ground.
                if (impulse.LeaveGroundFlag == true)
                {
                }

                impulseVel += thisImpulseVel;
                if (!impulse.IsActive) _impulses.RemoveAt(i);
            }

            return impulseVel;
        }

        #endregion

        #endregion

        #region Movement Processing

        private void UpdateMovement(float deltaTime)
        {
            // Calculate final velocity.
            Vector3 finalVel = CalculateFinalVelocity(deltaTime);

            if (!IsOnGround)
            {
                // Update buit-in gravity.
                _gravitySpeed += deltaTime * _gravityAccel;
                if (_gravitySpeed > _maxFallSpeed) _gravitySpeed = _maxFallSpeed;
                if (_enableGravity) finalVel.y -= _gravitySpeed;

                // Falling ground detection lag compenstation.
                if (_predictGroundWhenFalling && finalVel.y < 0f)
                {
                    GroundInfo predictedGroundInfo;
                    bool willBeOnGround = PredictProbeGround(out predictedGroundInfo, finalVel * deltaTime);
                    if (willBeOnGround)
                        _groundStepVel = CalcGroundStepVel(predictedGroundInfo.Distance, deltaTime, noSmoothing: true);
                }
            }
            else if (_restrictToGround)
            {
                GroundInfo predictedGroundInfo;
                bool willBeOnGround = PredictProbeGround(out predictedGroundInfo, finalVel * deltaTime);
                if (!willBeOnGround) finalVel.x = finalVel.z = 0f;
            }

            Move(finalVel);

            // Clean up.
            if (_activeVelocity != Vector3.zero) _lastNonZeroActiveDirection = _activeVelocity.normalized;
            _hasOverrideVel = false;
        }

        private Vector3 CalculateFinalVelocity(float deltaTime)
        {
            // Refresh active velocity. Apply velocity physics.
            _activeVelocity = UpdateVelocityPhysics(_activeVelocity, _activeVelocityTarget, deltaTime);

            // Refresh impulses.
            Vector3 impulseVelocity = UpdateImpulses(deltaTime);
                             
            // Calculate final velocity.
            Vector3 finalVelocity = Vector3.zero;
            SlopeNormal = CalculateSlopeNormal(GroundNormal);
            if (_hasOverrideVel)
            {
                finalVelocity = _overrideVel;

                // Rotate velocity to align to ground.
                if (IsOnGround) finalVelocity = AlignVelocityToNormal(finalVelocity, SlopeNormal);
            }
            else
            {
                Vector3 groundAlignedVelocity = Vector3.zero;

                // Velocities from velocity sources.
                Vector3 sourceVelocity = UpdateVelocitySources(deltaTime);

                // Rotate velocity to align to ground.
                if (IsOnGround) groundAlignedVelocity = AlignVelocityToNormal(_activeVelocity + sourceVelocity, SlopeNormal);

                finalVelocity = groundAlignedVelocity + _passiveVel + _extraVel + impulseVelocity;
            }

            finalVelocity += _connectedBodyVel;

#if UNITY_EDITOR
            // Slope normal.
            if (_showVelocityDebug) Debug.DrawLine(transform.position, transform.position + SlopeNormal, Color.cyan);
            // Desired velocity line.
            if (_showVelocityDebug) Debug.DrawLine(transform.position, transform.position + finalVelocity, Color.yellow);
#endif

            return finalVelocity;
        }

        private Vector3 UpdateVelocityPhysics(Vector3 currentVelocity, Vector3 targetVelocity, float deltaTime)
        {
            // Add any velocity from input, apply velocity physics to existing active velocity.

            bool isNotActive = (currentVelocity == Vector3.zero) && (targetVelocity == Vector3.zero);
            if (!isNotActive)
            {
                switch (_velocityMode)
                {
                    case VelocityPhysicsMode.None:
                        currentVelocity = targetVelocity;
                        break;
                    case VelocityPhysicsMode.Simple:
                        currentVelocity = CalculatePhysicalVelocitySimple(currentVelocity, targetVelocity, _speedChange);
                        break;
                    case VelocityPhysicsMode.Advanced:
                        currentVelocity = CalculatePhysicalVelocity(currentVelocity, targetVelocity,
                            deltaTime,
                            accel: _velocityConfig.Accel,
                            decel: _velocityConfig.Decel, brakingDecel: _velocityConfig.BrakingDecel,
                            friction: _velocityConfig.Friction, brakingFriction: _velocityConfig.BrakingFriction);
                        break;
                }
            }
            return currentVelocity;
        }

        /// <summary>
        /// Detect and calculate an approximated slope normal.
        /// </summary>
        /// <param name="groundNormal"></param>
        /// <returns></returns>
        private Vector3 CalculateSlopeNormal(Vector3 groundNormal)
        {
            bool foundSlope = false;
            Vector3 slopeNormal = groundNormal;
            if (IsOnGround && _useSlopeProbing)
            {

                foundSlope = (_slopeProbeFrontCount == 1 && _slopeProbeBackCount == 1) ?
                    ProbeSlope(out slopeNormal, basePoint: GroundPoint, offsetDirection: _lastNonZeroActiveDirection,
                        originY: ColliderCenter.y + _capsuleHalfHeight, range: GroundSensor.GroundThresholdDistance + _capsuleHalfHeight,
                        _slopeProbeFrontOffset, _slopeProbeBackOffset)
                    :
                    ProbeSlopeArray(out slopeNormal, basePoint: GroundPoint, offsetDirection: _lastNonZeroActiveDirection,
                        originY: ColliderCenter.y + _capsuleHalfHeight, range: GroundSensor.GroundThresholdDistance + _capsuleHalfHeight,
                        _slopeProbeFrontOffset, _slopeProbeFrontCount, _slopeProbeBackOffset, _slopeProbeBackCount);
            }
            return slopeNormal;
        }

        #endregion

        #region Ground Detection

        #region Collisions

        private void UpdateCollisions()
        {
            // Evaluate collisions.
            _isTouchingGround = false;
            _isTouchingCeiling = false;
            Vector3 collisionGroundNormal;
            float collisionGroundHeight;
            EvaluateContacts(out _isTouchingGround, out _isTouchingCeiling,
                out collisionGroundNormal, out collisionGroundHeight, _collisions);

            // Init frame ground info.
            IsOnGround = false;
            GroundNormal = Vector3.up;
            GroundPoint = transform.position;
            _groundStepVel = Vector3.zero;

            // Perform ground probing and update floating adjustment velocity.
            GroundInfo probedGroundInfo;
            IsOnGround = EvaluateProbeGround(out probedGroundInfo);
        }

        /// <summary>
        /// Iterate through every collision contact point and perform checks.
        /// </summary>
        private bool EvaluateContacts(out bool isTouchingGround, out bool isTouchingCeiling,
            out Vector3 groundNormal, out float groundHeight, List<Collision> collisions)
        {
            bool hasContact = false;
            isTouchingGround = false;
            isTouchingCeiling = false;

            int groundCollisionCount = 0;
            Vector3 accGroundNormal = Vector3.zero;
            groundHeight = 0f;

            // For each collision.
            for (int i = 0; i < collisions.Count; i++)
            {
                Collision collision = collisions[i];
                bool isGroundCollision = false;
                int groundContactCount = 0;
                Vector3 accGroundContactsNormal = Vector3.zero; // Average ground normal from a collision.

                // For each contact in a collision.
                for (int j = 0; j < collision.contactCount; j++)
                {
                    ContactPoint contact = collision.GetContact(j);
                    hasContact = true;

                    // If is ground contact.
                    if (contact.normal.y > _minGroundAngleDot + 0.001f)
                    {
                        isTouchingGround = true;
                        isGroundCollision = true;
                        accGroundContactsNormal = contact.normal;
                        groundContactCount++;
                        // Update average ground height.
                        groundHeight = groundHeight * (groundContactCount - 1) / groundContactCount
                            + (contact.point.y / groundContactCount);
                    }
                    // If is ceiling contact.
                    else if (contact.normal.y < -0.001f)
                    {
                        isTouchingCeiling = true;
                    }
                }
                // If this is a ground collision.
                if (isGroundCollision)
                {
                    // Accumulate ground normal from this collision.
                    accGroundNormal += (accGroundContactsNormal / (float)groundContactCount);
                    groundCollisionCount++;
                }
            }
            // Average ground normal from all ground collisions.
            groundNormal = isTouchingGround ? accGroundNormal / (float)groundCollisionCount : Vector3.up;

            return hasContact;
        }
        #endregion

        #region Ground Probing
        /// <summary>
        /// Perform ground probing, and update ground floating adjustment velocity.
        /// </summary>
        /// <param name="groundInfo"></param>
        /// <returns></returns>
        private bool EvaluateProbeGround(out GroundInfo groundInfo)
        {
            return EvaluateProbeGround(out groundInfo, Vector3.zero);
        }

        /// <summary>
        /// Perform ground probing at an offset, and update ground floating adjustment velocity.
        /// </summary>
        /// <param name="groundInfo"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private bool EvaluateProbeGround(out GroundInfo groundInfo, Vector3 offset)
        {
            bool isOnGround = PredictProbeGround(out groundInfo, offset, _useRealGroundNormal);
            if (isOnGround)
            {
                GroundNormal = groundInfo.Normal;
                GroundPoint = groundInfo.Point;
                // Update ground connection velocity.
                if (groundInfo.Collider != GroundCollider)
                {
                    // This is a new connection, initialize the connection position and velocity.
                    _prevConnectionPos = groundInfo.Collider.transform.position;
                    _connectedBodyVel = Vector3.zero;
                    GroundCollider = groundInfo.Collider;
                }
                else
                {
                    Vector3 groundPos = GroundCollider.transform.position;
                    if (_wasOnGround)
                    {
                        _connectedBodyPosDelta = groundPos - _prevConnectionPos;
                        _connectedBodyVel = _connectedBodyPosDelta / Time.deltaTime;
                    }
                    _prevConnectionPos = groundPos;
                }
                // Update ground floating adjustment velocity.
                _groundStepVel = CalcGroundStepVel(groundInfo.Distance, Time.deltaTime, _connectedBodyPosDelta.y);
            }

#if UNITY_EDITOR
            if (_showGroundProbingDebug)
            {
                Vector3 desiredGroundPoint = ColliderCenter + offset - new Vector3(0f, DesiredGroundDistance, 0f);
                // Desired ground distance.
                if (_showGroundProbingDebug) Debug.DrawLine(ColliderCenter + offset, desiredGroundPoint, Color.green);
            }
#endif

            return isOnGround;
        }

        /// <summary>
        /// Perform ground probing.
        /// </summary>
        /// <param name="groundInfo"></param>
        /// <returns></returns>
        public bool PredictProbeGround(out GroundInfo groundInfo, bool useRealGroundNormal = false)
        {
            return PredictProbeGround(out groundInfo, Vector3.zero, useRealGroundNormal);
        }

        /// <summary>
        /// Perform ground probing at an offset. 
        /// </summary>
        /// <param name="groundInfo"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public bool PredictProbeGround(out GroundInfo groundInfo, Vector3 offset, bool useRealGroundNormal = false)
        {
            // Ground probing.
            bool isOnGround = GroundSensor.ProbeGround(out groundInfo, _totalGroundProbeRange, _groundProbeThickness,
                ColliderCenter + offset, layerMask: _groundLayer, useRealGroundNormal);
            return isOnGround;
        }
        #endregion

        #region Slope Probing
        /// <summary>
        /// Probe slope in the specified direction.
        /// </summary>
        /// <param name="basePoint"></param>
        /// <param name="offset"></param>
        /// <param name="offsetDirection"></param>
        /// <param name="originY"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        private bool ProbeSlope(out Vector3 slopeNormal, Vector3 basePoint, Vector3 offsetDirection,
            float originY, float range,
            float frontOffset, float backOffset)
        {
            bool foundSlope = false;
            offsetDirection.y = 0f;
            slopeNormal = Vector3.up;

            Vector3 frontOrigin = basePoint + frontOffset * offsetDirection;
            frontOrigin.y = originY;
            RaycastHit frontHitInfo;
            bool frontHit = Physics.Raycast(new Ray(frontOrigin, Vector3.down), out frontHitInfo,
                maxDistance: range, layerMask: _groundLayer);

            Vector3 backOrigin = basePoint + backOffset * -offsetDirection;
            backOrigin.y = originY;
            RaycastHit backHitInfo;
            bool backHit = Physics.Raycast(new Ray(backOrigin, Vector3.down), out backHitInfo,
                maxDistance: range, layerMask: _groundLayer);

            Vector3 frontPoint = basePoint;
            Vector3 backPoint = basePoint;
            float stepY = basePoint.y + _stepHeight;
            if (frontHit && frontHitInfo.point.y <= stepY) frontPoint = frontHitInfo.point;
            if (backHit && backHitInfo.point.y <= stepY) backPoint = backHitInfo.point;

            if (frontPoint != backPoint)
            {
                Vector3 slope = frontPoint - backPoint;
                slopeNormal = Vector3.Cross(slope, Vector3.Cross(Vector3.up, slope)).normalized;
                foundSlope = true;
            }
            else
            {
                foundSlope = false;
            }

#if UNITY_EDITOR
            if (_showSlopeDebug) Debug.DrawLine(backPoint, frontPoint, Color.white);
#endif

            return foundSlope;
        }


        /// <summary>
        /// Probe slope in the specified direction.
        /// </summary>
        /// <param name="basePoint"></param>
        /// <param name="offset"></param>
        /// <param name="offsetDirection"></param>
        /// <param name="originY"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        private bool ProbeSlopeArray(out Vector3 slopeNormal, Vector3 basePoint, Vector3 offsetDirection,
            float originY, float range,
            float frontOffset = 0.2f, int frontCount = 2, float backOffset = 0.2f, int backCount = 2)
        {
            // Init variables.
            bool foundSlope = false;
            offsetDirection.y = 0f;
            slopeNormal = Vector3.zero;
            float frontOffsetStep = frontOffset / (float)frontCount;
            float backOffsetStep = backOffset / (float)backCount;
            float stepY = basePoint.y + _stepHeight;

            // Init front probing origin.
            Vector3 frontOrigin = basePoint + frontOffset * offsetDirection;
            frontOrigin.y = originY;

            // Init back probing origin.
            Vector3 backOrigin = basePoint + backOffsetStep * -offsetDirection;
            backOrigin.y = originY;

            _slopePoints.Clear();

            for (int i = 0; i < frontCount; i++)
            {
                RaycastHit frontHitInfo;
                bool frontHit = Physics.Raycast(new Ray(frontOrigin, Vector3.down), out frontHitInfo,
                    maxDistance: range, layerMask: _groundLayer);
                Vector3 frontPoint = basePoint;
                // Successful.
                if (frontHit && frontHitInfo.point.y <= stepY)
                {
                    frontPoint = frontHitInfo.point;
                    _slopePoints.Add(frontPoint);
#if UNITY_EDITOR
                    if (_showSlopeDebug) Debug.DrawLine(frontOrigin, frontPoint, Color.white);
#endif
                }
                else
                {
                    _slopePoints.Clear();
                }
                frontOrigin += frontOffsetStep * -offsetDirection;
            }

            _slopePoints.Add(basePoint);

            for (int i = 0; i < backCount; i++)
            {
                RaycastHit backHitInfo;
                bool backHit = Physics.Raycast(new Ray(backOrigin, Vector3.down), out backHitInfo,
                    maxDistance: range, layerMask: _groundLayer);
                Vector3 backPoint = basePoint;
                if (backHit && backHitInfo.point.y <= stepY)
                {
                    backPoint = backHitInfo.point;
                    _slopePoints.Add(backPoint);
#if UNITY_EDITOR
                    if (_showSlopeDebug) Debug.DrawLine(backOrigin, backPoint, Color.white);
#endif
                }
                else break;
                backOrigin += backOffsetStep * -offsetDirection;
            }

            Vector3 slope = Vector3.zero;
            // Connect points.
            int segmentCount = 0;
            for (int i = 0; i < _slopePoints.Count; i++)
            {
                if (i + 1 >= _slopePoints.Count) break;
                Vector3 point = _slopePoints[i];
                Vector3 nextPoint = _slopePoints[i + 1];
                Vector3 segmentSlope = nextPoint - point;
                Vector3 segmentNormal = Vector3.Cross(segmentSlope, Vector3.Cross(Vector3.up, segmentSlope)).normalized;
                slope += segmentSlope;
                slopeNormal += segmentNormal;
                segmentCount += 1;
                foundSlope = true;

#if UNITY_EDITOR
                if (_showSlopeDebug) Debug.DrawLine(point, nextPoint, Color.white);
#endif
            }
            if (foundSlope)
            {
                slope = slope.normalized;
                slopeNormal = slopeNormal.normalized;
#if UNITY_EDITOR
                if (_showSlopeDebug)
                {
                    Debug.DrawLine(_slopePoints[0], _slopePoints[0] + slope * (_slopeProbeFrontOffset + _slopeProbeBackOffset), Color.cyan);
                }
#endif
            }

            return foundSlope;
        }
        #endregion

        #region Ground State
        /// <summary>
        /// Calculate the adjustment floating velocity needed to maintain desired ground distance (step height).
        /// </summary>
        /// <param name="groundDistance"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        private Vector3 CalcGroundStepVel(float groundDistance, float deltaTime, float offsetY = 0f, bool noSmoothing = false)
        {
            Vector3 vel = _groundStepVel;
            float requiredDelta = (_capsuleHalfHeight + _stepHeight + offsetY) - groundDistance;
            bool shouldGoUp = requiredDelta > 0f;
            bool shouldGoDown = requiredDelta < 0f;
            if (_hasGroundStateChanged || _isTouchingCeiling || noSmoothing)
            {
                vel = Vector3.up * (requiredDelta / deltaTime);
            }
            else
            {
                float stepSmooth = shouldGoUp ? _stepUpSmooth : _stepDownSmooth;
                vel = Vector3.up * (requiredDelta / (deltaTime * stepSmooth));
            }
            return vel;
        }

        private void UpdateGroundState(bool isOnGround)
        {
            // Gained ground contact.
            if (!_wasOnGround && IsOnGround)
            {
                _gravitySpeed = 0f;
                _hasGroundStateChanged = true;
                GainedGroundContact.Invoke();
                UseExtraGroundThresholdDistance = true;
            }
            // Lost ground contact.
            else if (_wasOnGround && !IsOnGround)
            {
                _hasGroundStateChanged = true;
                LostGroundContact.Invoke();
                UseExtraGroundThresholdDistance = false;
            }
            else
            {
                _hasGroundStateChanged = false;
            }
            _wasOnGround = IsOnGround;
        }

        private void UpdateGroundSensorThresholdDistance()
        {
            GroundSensor.GroundThresholdDistance = _useExtraGroundThresholdDistance ?
                DesiredGroundDistance + ExtraGroundThresholdDistance
                : DesiredGroundDistance;
        }
        #endregion

        #endregion

        #region Velocity Physics

        private Vector3 CalculatePhysicalVelocitySimple(Vector3 currentVelocity, Vector3 targetVelocity, float accel = kMaxSpeedChange)
        {
            Vector3 finalVel = currentVelocity;
            float speedChange = accel * Time.deltaTime;

            finalVel = Vector3.MoveTowards(finalVel, targetVelocity, speedChange);

            return finalVel;
        }

        private Vector3 CalculatePhysicalVelocity(Vector3 currentVelocity, Vector3 targetVelocity,
            float deltaTime,
            float accel = VelocityConfig.kMaxAccel,
            float decel = VelocityConfig.kMaxDecel, float brakingDecel = VelocityConfig.kMaxDecel,
            float friction = VelocityConfig.kMaxFriction, float brakingFriction = VelocityConfig.kMaxFriction)
        {
            float currentSpeedSqr = currentVelocity.sqrMagnitude;
            float targetSpeedSqr = targetVelocity.sqrMagnitude;
            float targetSpeed = targetVelocity.magnitude;
            Vector3 currentDirection = currentVelocity.normalized;
            Vector3 targetDirection = targetVelocity.normalized;

            if (currentSpeedSqr > targetSpeedSqr * 1.01f)
            {
                if (targetSpeedSqr > 0f && targetDirection != Vector3.zero)
                {
                    currentVelocity = ApplyDeceleration(currentVelocity, currentDirection, targetSpeed, decel, deltaTime);
                }
                else currentVelocity = ApplyVelocityBraking(currentVelocity, currentDirection, brakingFriction, brakingDecel, deltaTime);
            }
            else
            {
                float currentSpeed = currentVelocity.magnitude;
                currentVelocity = ApplyFriction(currentVelocity, currentSpeed, targetDirection, friction, deltaTime);
                ;
                if (currentSpeed < targetSpeed)
                {
                    currentVelocity += deltaTime * accel * targetDirection;
                    currentVelocity = Vector3.ClampMagnitude(currentVelocity, targetSpeed);
                }
            }

#if UNITY_EDITOR
            if (_showVelocityDebug)
            {
                // Desired velocity line.
                Debug.DrawLine(transform.position, transform.position + targetVelocity, Color.gray);
                // Final velocity line.
                Debug.DrawLine(transform.position, transform.position + currentVelocity, Color.black);
            }
#endif

            return currentVelocity;
        }

        /// <summary>
        /// Decelerate velocity to zero.
        /// </summary>
        /// <param name="currentVelocity"></param>
        /// <param name="friction"></param>
        /// <param name="decel"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        private static Vector3 ApplyVelocityBraking(Vector3 currentVelocity, Vector3 currentDirection,
            float friction, float decel, float deltaTime)
        {
            bool isZeroFriction = friction == 0f;
            bool isZeroDecel = decel == 0f;
            if (isZeroFriction && isZeroDecel) return currentVelocity;

            // Calculate braking deceleration.
            Vector3 oldVel = currentVelocity;
            Vector3 decelVec = isZeroDecel ? Vector3.zero : -decel * currentDirection;

            // Apply friction and deceleration.
            currentVelocity += (-friction * currentVelocity + decelVec) * deltaTime;

            // Stop before we start to go backwards.
            if (Vector3.Dot(currentVelocity, oldVel) <= 0f) return Vector3.zero;

            // Snap to zero.
            if (currentVelocity.sqrMagnitude <= 0.1f) return Vector3.zero;

            return currentVelocity;
        }

        private static Vector3 ApplyDeceleration(Vector3 currentVelocity, Vector3 currentDirection, 
            float targetSpeed, float decel, float deltaTime)
        {
            bool isZeroDecel = decel == 0f;
            if (isZeroDecel) return currentVelocity;

            currentVelocity += deltaTime * -decel * currentDirection;
            currentVelocity = Vector3.ClampMagnitude(currentVelocity, targetSpeed);
            return currentVelocity;
        }

        private static Vector3 ApplyFriction(Vector3 currentVelocity, float currentSpeed, 
            Vector3 desiredDirection, float friction, float deltaTime)
        {
            currentVelocity -= (currentVelocity - desiredDirection * currentSpeed) * Mathf.Min(friction * deltaTime, 1f);
            return currentVelocity;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Align velocity to a plane defined by the specified plane normal.
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        private Vector3 AlignVelocityToNormal(Vector3 velocity, Vector3 normal)
        {
            float speed = velocity.magnitude;
            Vector3 alignedDirection = Quaternion.FromToRotation(Vector3.up, normal) * (velocity / speed);
            return speed * alignedDirection.normalized;
        }

        private void UpdateColliderDimensions()
        {
            UpdateColliderHeight();
            UpdateColliderRadius();
        }

        private void UpdateColliderHeight()
        {
            if (_stepHeight > _height) _stepHeight = _height;
            Vector3 center = _colliderOffset + new Vector3(0f, _height / 2f, 0f);
            center.y += _stepHeight / 2f;
            _capsuleCollider.center = center;
            _capsuleCollider.height = _height - _stepHeight;
            _capsuleHalfHeight = _capsuleCollider.height / 2f;
            LimitRadius();
            UpdateGroundCheckDimensions();
        }

        public void UpdateColliderRadius()
        {
            float radius = _thickness / 2f;
            _capsuleCollider.radius = radius;
            LimitRadius();
        }

        protected void UpdateGroundCheckDimensions()
        {
            // Update desired ground distance.
            DesiredGroundDistance = (_capsuleHalfHeight + _stepHeight) * (1 + _groundCheckToleranceFactor);
            // Update extra ground threshold distance used to snap to ground when going down stairs or slopes.
            ExtraGroundThresholdDistance = Mathf.Max(_stepHeight, _minExtraGroundThreshold);
            // Update total length for ground probing.
            _totalGroundProbeRange = DesiredGroundDistance + _groundProbeRange;
        }

        /// <summary>
        /// Restrict collider minimum thickness to collider height.
        /// </summary>
        private void LimitRadius()
        {
            if (_capsuleCollider.radius * 2f > _capsuleCollider.height) _capsuleCollider.radius = _capsuleCollider.height / 2f;
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        private void DrawContactNormals()
        {
            if (!_showContactDebug) return;
            for (int i = 0; i < _collisions.Count; i++)
            {
                Collision collision = _collisions[i];
                for (int j = 0; j < collision.contactCount; j++)
                {
                    ContactPoint contact = collision.GetContact(j);
                    Color color = Color.grey;
                    if (contact.normal.y > _minFlatGroundAngleDot)
                    {
                        color = Color.white;
                    }
                    Debug.DrawLine(contact.point, contact.point + contact.normal * 0.1f, color);
                }
            }
        }
#endif

        #endregion
    }
}