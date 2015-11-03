using System;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Models;
using DD.CBU.CaasDeploy.Library.Tasks;

namespace DD.CBU.CaasDeploy.Library.Utilities
{
    /// <summary>
    /// Provides commonly used extension methods for <see cref="ITask"/> implementations.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Gets the role membership required to execute the task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>The role membership required.</returns>
        public static string GetRequiredRole(this ITask task)
        {
            var resourceType = ResourceType.Unknown;

            var deployResourceTask = task as DeployResourceTask;
            if (deployResourceTask != null)
            {
                resourceType = deployResourceTask.Resource.ResourceType;
            }

            var deleteResourceTask = task as DeleteResourceTask;
            if (deleteResourceTask != null)
            {
                resourceType = deleteResourceTask.ResourceLog.ResourceType;
            }

            switch (resourceType)
            {
                case ResourceType.Unknown:
                    return null;
                case ResourceType.Server:
                    return "server";
                case ResourceType.NetworkDomain:
                case ResourceType.Vlan:
                case ResourceType.FirewallRule:
                case ResourceType.PublicIpBlock:
                case ResourceType.NatRule:
                case ResourceType.Node:
                case ResourceType.Pool:
                case ResourceType.PoolMember:
                case ResourceType.VirtualListener:
                    return "network";
                default:
                    throw new NotSupportedException($"Unknown resource type '{resourceType}'.");
            }
        }
    }
}
