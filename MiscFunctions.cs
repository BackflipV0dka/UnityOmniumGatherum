using UnityEngine;

namespace UnityOmniumGatherum
{
    public static class HelperFunctions
    {
        public static Vector2 Joint2DForceSum(this Rigidbody2D rigid)
        {
            Vector2 sum = Vector2.zero;
            if (rigid.bodyType != RigidbodyType2D.Dynamic) return sum;

            foreach (Joint2D j in rigid.GetComponents<Joint2D>())
                if (j.enabled) sum += j.reactionForce;

            return sum;
        }

        public static void CopyValues(this Rigidbody2D receiver, Rigidbody2D donor)
        {
            if (RigidbodyType2D.Static != (receiver.bodyType = donor.bodyType))
            {
                if (!(receiver.useAutoMass = donor.useAutoMass))
                    receiver.mass = donor.mass;
                //receiver.centerOfMass = donor.centerOfMass;
                //receiver.drag = donor.drag;
                //receiver.angularDrag = donor.angularDrag;
                //receiver.gravityScale = donor.gravityScale;

                //receiver.sleepMode = donor.sleepMode;
                receiver.interpolation = donor.interpolation;
                receiver.collisionDetectionMode = donor.collisionDetectionMode;
                receiver.constraints = donor.constraints;
                if (donor.bodyType == RigidbodyType2D.Kinematic) receiver.useFullKinematicContacts = donor.useFullKinematicContacts;
            }
            receiver.simulated = donor.simulated;
            receiver.sharedMaterial = donor.sharedMaterial;
        }

        private static void CopyValues(this Behaviour receiver, Behaviour donor)
        {
            receiver.enabled = donor.enabled;
            //receiver.name = donor.name;
            //receiver.tag = donor.tag;
            //receiver.hideFlags = donor.hideFlags;
        }

        private static void CopyValues(this Joint2D receiver, Joint2D donor)
        {
            (receiver as Behaviour).CopyValues(donor);
            receiver.enableCollision = donor.enableCollision;
            receiver.connectedBody = donor.connectedBody;
            receiver.breakForce = donor.breakForce;
            receiver.breakTorque = donor.breakTorque;
        }

        private static void CopyValues(this AnchoredJoint2D receiver, AnchoredJoint2D donor)
        {
            (receiver as Joint2D).CopyValues(donor);
            receiver.anchor = donor.anchor;
            if (!(receiver.autoConfigureConnectedAnchor = donor.autoConfigureConnectedAnchor))
                receiver.connectedAnchor = donor.connectedAnchor;
        }

        public static void CopyValues(this HingeJoint2D receiver, HingeJoint2D donor)
        {
            (receiver as AnchoredJoint2D).CopyValues(donor);
            if (receiver.useMotor = donor.useMotor)
                receiver.motor = donor.motor;
            if (receiver.useLimits = donor.useLimits)
                receiver.limits = donor.limits;
        }

        public static void CopyValues(this DistanceJoint2D receiver, DistanceJoint2D donor)
        {
            (receiver as AnchoredJoint2D).CopyValues(donor);
            if (!(receiver.autoConfigureDistance = donor.autoConfigureDistance))
                receiver.distance = donor.distance;
            receiver.maxDistanceOnly = donor.maxDistanceOnly;
        }


        public static T AddComponent<T>(this GameObject obj, T donor) where T : Component
        {
            if (!donor) return null;
            T c = obj.AddComponent(donor.GetType()) as T;

            if (donor is Behaviour)
            {
                if (donor is MonoBehaviour)
                {
                    if (donor is PHY_Draggable) (c as PHY_Draggable).CopyValues(donor as PHY_Draggable);
                }
                else if (donor is Joint2D)
                {
                    if (donor is AnchoredJoint2D)
                    {
                        if (donor is HingeJoint2D) (c as HingeJoint2D).CopyValues(donor as HingeJoint2D);
                        else if (donor is DistanceJoint2D) (c as DistanceJoint2D).CopyValues(donor as DistanceJoint2D);
                        else if (donor is FixedJoint2D) (c as FixedJoint2D).CopyValues(donor as FixedJoint2D);
                        else if (donor is FrictionJoint2D) (c as FrictionJoint2D).CopyValues(donor as FrictionJoint2D);
                        else if (donor is SliderJoint2D) (c as SliderJoint2D).CopyValues(donor as SliderJoint2D);
                        else if (donor is SpringJoint2D) (c as SpringJoint2D).CopyValues(donor as SpringJoint2D);
                        else if (donor is WheelJoint2D) (c as WheelJoint2D).CopyValues(donor as WheelJoint2D);

                    }
                    else if (donor is RelativeJoint2D) (c as RelativeJoint2D).CopyValues(donor as RelativeJoint2D);
                    else if (donor is TargetJoint2D) (c as TargetJoint2D).CopyValues(donor as TargetJoint2D);
                }
            }
            else if (donor is Rigidbody2D) (c as Rigidbody2D).CopyValues(donor as Rigidbody2D);

            return c;
        }
    }
}