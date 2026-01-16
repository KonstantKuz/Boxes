using UnityEngine;

namespace Gameplay.Interactable.BoxesInteraction.Components
{
    public class AttachableObject : MonoBehaviour
    {
        [SerializeField]
        private new Rigidbody rigidbody;

        [SerializeField]
        private new Collider collider;

        [Header("Joint Settings")]
        [SerializeField, Tooltip("Break force")]
        private float breakForce = 500000f;

        [SerializeField, Tooltip("Break torque")]
        private float breakTorque = 500000f;

        [SerializeField, Tooltip("Slerp drive spring strength")]
        private float slerpDriveSpring = 500000f;

        [SerializeField, Tooltip("Slerp drive damper")]
        private float slerpDriveDamper = 50000f;

        [SerializeField, Tooltip("Slerp drive max force")]
        private float slerpDriveMaxForce = 500000f;

        [Header("Rigidbody Settings")]
        [SerializeField, Tooltip("Количество итераций физического солвера при присоединении")]
        private int attachedSolverIterations = 100;

        [SerializeField, Tooltip("Количество итераций скорости физического солвера при присоединении")]
        private int attachedSolverVelocityIterations = 100;

        [SerializeField, Tooltip("Максимальная скорость выталкивания при проникновении")]
        private float attachedMaxDepenetrationVelocity = 2f;

        [SerializeField, Tooltip("Линейное сопротивление при присоединении")]
        private float attachedDrag = 2f;

        [SerializeField, Tooltip("Угловое сопротивление при присоединении")]
        private float attachedAngularDrag = 5f;

        private ConfigurableJoint joint;
        private Rigidbody attachedTo;

        private int initialSolverIterations;
        private int initialSolverVelocityIterations;
        private float initialMaxAngularVelocity;
        private float initialMaxDepenetrationVelocity;
        private float initialDrag;
        private float initialAngularDrag;

        public bool IsAttached => attachedTo != null;
        public Rigidbody AttachedTo => attachedTo;

        private void Awake()
        {
            initialSolverIterations = rigidbody.solverIterations;
            initialSolverVelocityIterations = rigidbody.solverVelocityIterations;
            initialMaxAngularVelocity = rigidbody.maxAngularVelocity;
            initialMaxDepenetrationVelocity = rigidbody.maxDepenetrationVelocity;
            initialDrag = rigidbody.drag;
            initialAngularDrag = rigidbody.angularDrag;
        }

        public void Attach(Rigidbody target, Vector3? anchorPoint = null)
        {
            if (target != null)
            {
                if (joint == null)
                {
                    joint = gameObject.GetComponent<ConfigurableJoint>();
                }

                if (joint == null)
                {
                    joint = gameObject.AddComponent<ConfigurableJoint>();
                }

                if (joint == null)
                {
                    Debug.LogError($"Failed to create ConfigurableJoint on {gameObject.name}");
                    return;
                }

                joint.connectedBody = target;
                joint.enablePreprocessing = true;
                joint.enableCollision = true;

                joint.breakForce = breakForce;
                joint.breakTorque = breakTorque;

                joint.anchor = Vector3.zero;
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = target.transform.InverseTransformPoint(anchorPoint ?? transform.position);

                joint.xMotion = ConfigurableJointMotion.Locked;
                joint.yMotion = ConfigurableJointMotion.Locked;
                joint.zMotion = ConfigurableJointMotion.Locked;

                joint.angularXMotion = ConfigurableJointMotion.Free;
                joint.angularYMotion = ConfigurableJointMotion.Free;
                joint.angularZMotion = ConfigurableJointMotion.Free;

                joint.rotationDriveMode = RotationDriveMode.Slerp;

                JointDrive slerpDrive = new JointDrive();
                slerpDrive.positionSpring = slerpDriveSpring;
                slerpDrive.positionDamper = slerpDriveDamper;
                slerpDrive.maximumForce = slerpDriveMaxForce;
                joint.slerpDrive = slerpDrive;

                rigidbody.solverIterations = attachedSolverIterations;
                rigidbody.solverVelocityIterations = attachedSolverVelocityIterations;
                rigidbody.maxDepenetrationVelocity = attachedMaxDepenetrationVelocity;
                rigidbody.drag = attachedDrag;
                rigidbody.angularDrag = attachedAngularDrag;

                attachedTo = target;
            }
            else
            {
                rigidbody.solverIterations = initialSolverIterations;
                rigidbody.solverVelocityIterations = initialSolverVelocityIterations;
                rigidbody.maxAngularVelocity = initialMaxAngularVelocity;
                rigidbody.maxDepenetrationVelocity = initialMaxDepenetrationVelocity;
                rigidbody.drag = initialDrag;
                rigidbody.angularDrag = initialAngularDrag;

                if (joint != null)
                {
                    Destroy(joint);
                    joint = null;
                }

                attachedTo = null;
            }
        }

        public void Detach()
        {
            Attach(null);
        }
    }
}
