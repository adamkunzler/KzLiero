using Kz.Engine.DataStructures;

namespace Kz.POC.GrappleHook
{
    public class Spring
    {
        public Vector2f Anchor { get; set; }        
        public float RestLength { get; init; }

        public float K { get; init; }

        public Spring(float restLength, float k)
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