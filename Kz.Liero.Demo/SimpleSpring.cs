using Kz.Engine.DataStructures;

namespace Kz.Liero.Demo
{
    /// <summary>
    /// Represents a simple spring
    ///
    /// SpringForce = -k * x
    ///     :where k = constant and represent scale of the force
    ///     :where x = represents displacement of the spring (difference between rest length and current length)
    ///         = currentLength - restLength
    /// </summary>
    public class SimpleSpring
    {
        public Vector2f Anchor { get; set; }

        public float RestLength { get; init; }

        public float K { get; init; }

        public SimpleSpring(float restLength, float k)
        {
            RestLength = restLength;
            K = k;
        }

        public Vector2f GetForce(Vector2f location)
        {
            var force = location - Anchor;
            var currentLength = force.Magnitude();
            var x = currentLength - RestLength;
            var springForce = -1 * K * x;

            force = force.Normal() * springForce;
            return force;
        }
    }
}