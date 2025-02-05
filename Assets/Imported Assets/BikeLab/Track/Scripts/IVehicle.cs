using UnityEngine;

namespace VK.BikeLab
{
    public interface IVehicle
    {
        /// <summary>
        /// Gets the Rigidbody component of the vehicle.
        /// </summary>
        /// <returns>The Rigidbody of the vehicle.</returns>
        public Rigidbody getRigidbody();

        /// <summary>
        /// Gets the maximum forward acceleration of the vehicle.
        /// </summary>
        /// <returns>The maximum forward acceleration.</returns>
        public float getMaxForwardAcceleration();

        /// <summary>
        /// Gets the maximum brake acceleration of the vehicle.
        /// </summary>
        /// <returns>The maximum brake acceleration.</returns>
        public float getMaxBrakeAcceleration();

        /// <summary>
        /// Gets the maximum sideways acceleration of the vehicle at a given velocity.
        /// </summary>
        /// <param name="velocity">The current velocity of the vehicle.</param>
        /// <returns>The maximum sideways acceleration.</returns>
        public float getMaxSidewaysAcceleration(float velocity);

        /// <summary>
        /// Gets the allowed speed for the given turning radius.
        /// </summary>
        /// <param name="radius">The turning radius.</param>
        /// <returns>The allowed speed.</returns>
        public float getMaxVelocity(float radius);

        /// <summary>
        /// Gets the radius steer value for the given turning radius.
        /// </summary>
        /// <param name="radius">The turning radius.</param>
        /// <returns>The radius steer value.</returns>
        public float getRadiusSteer(float radius);

        /// <summary>
        /// Gets the current turning radius of the vehicle.
        /// </summary>
        /// <returns>The current turning radius.</returns>
        public float getTurnRadius();

        /// <summary>
        /// Gets the current turning direction of the vehicle.
        /// </summary>
        /// <returns>The current turning direction.</returns>
        public float getTurnDir();

        /// <summary>
        /// Resets the vehicle to its initial state.
        /// </summary>
        public void reset();

        /// <summary>
        /// Sets the vehicle to a vertical position.
        /// </summary>
        /// <param name="h">The height to which the vehicle should be lifted.</param>
        /// <param name="turn">The turn value to set.</param>
        public void getup(float h = 0.1f, float turn = 0);
    }
}
