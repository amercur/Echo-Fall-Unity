using UnityEngine;

namespace EchoFall.Movement
{
    [DefaultExecutionOrder(100)] // Follow after the motor's fixed simulation step.
    [RequireComponent(typeof(Camera))]
    public sealed class RoomCamera : MonoBehaviour
    {
        public PlayerMotor target;
        public Rect bounds = new Rect(0, -.88f, 32, 12);
        public float smoothing = 7f;
        public float lookAheadSeconds = .13f;
        Camera view;
        int resetCount = -1;

        void Awake() => view = GetComponent<Camera>();
        void Start() => SnapToTarget();
        void FixedUpdate() => Simulate(Time.fixedDeltaTime);

        public void SnapToTarget()
        {
            if (view == null) view = GetComponent<Camera>();
            if (target == null || view == null) return;
            // Room entry/hazard return uses a 300px vertical anchor and no look-ahead.
            Vector2 desired = DesiredPosition(target.Position, Vector2.zero, view.orthographicSize, view.aspect, 3f);
            transform.position = new Vector3(desired.x, desired.y, -10);
            resetCount = target.ResetCount;
        }

        public Vector2 DesiredPosition(Vector2 feet, Vector2 velocity, float halfHeight, float aspect, float verticalAnchor = 2.85f)
        {
            // Source uses player left/top and a 340px X, 285px Y follow anchor.
            float halfWidth = halfHeight * aspect;
            return ClampCenter(new Vector2(feet.x - .11f + halfWidth - 3.4f + velocity.x * lookAheadSeconds,
                feet.y + .4f + verticalAnchor - halfHeight), halfHeight, aspect);
        }

        public Vector2 ClampCenter(Vector2 center, float halfHeight, float aspect)
        {
            float halfWidth = halfHeight * aspect;
            return new Vector2(bounds.width <= halfWidth * 2 ? bounds.center.x : Mathf.Clamp(center.x, bounds.xMin + halfWidth, bounds.xMax - halfWidth),
                bounds.height <= halfHeight * 2 ? bounds.center.y : Mathf.Clamp(center.y, bounds.yMin + halfHeight, bounds.yMax - halfHeight));
        }

        public void Simulate(float dt)
        {
            if (view == null) view = GetComponent<Camera>();
            if (target == null || view == null) return;
            if (target.ResetCount != resetCount) { SnapToTarget(); return; }
            Vector2 desired = DesiredPosition(target.Position, target.Velocity, view.orthographicSize, view.aspect);
            Vector2 position = Vector2.Lerp(transform.position, desired, 1 - Mathf.Exp(-smoothing * dt));
            position = ClampCenter(position, view.orthographicSize, view.aspect);
            transform.position = new Vector3(position.x, position.y, -10);
            resetCount = target.ResetCount;
        }
    }
}
