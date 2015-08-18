using JetBrains.Annotations;
using QuickGraph;
using System;
using System.Collections.Generic;

namespace CaasDeploy.Library.Dependencies
{
	/// <summary>
	///		Represents a node in a <see cref="Resource"/> dependency graph.
	/// </summary>
	/// <remarks>
	///		Enables sorting of resources in dependency order.
	/// </remarks>
	sealed class ResourceDependencyNode
		: IEdge<Resource>
	{
		/// <summary>
		///		Create a new resource dependency.
		/// </summary>
		/// <param name="resource">
		///		The target resource whose dependency are expressed by the <see cref="ResourceDependencyNode"/>.
		/// </param>
		/// <param name="dependsOnResource">
		///		The resource on which the target resource depends.
		/// </param>
		public ResourceDependencyNode(Resource resource, Resource dependsOnResource)
		{
			if (resource == null)
				throw new ArgumentNullException(nameof(resource));

			if (dependsOnResource == null)
				throw new ArgumentNullException(nameof(dependsOnResource));

			Resource = resource;
			DependsOnResource = dependsOnResource;
        }

		/// <summary>
		///		The target resource whose dependency are expressed by the <see cref="ResourceDependencyNode"/>.
		/// </summary>
		public Resource Resource
		{
			get;
		}

		/// <summary>
		///		The resource on which the target resource depends.
		/// </summary>
		public Resource DependsOnResource
		{
			get;
		}

		#region IEdge<Resource>

		/// <summary>
		///		The edge's source vertex (when considering the dependencies as a directed graph).
		/// </summary>
		[NotNull]
		Resource IEdge<Resource>.Source => Resource;

		/// <summary>
		///		The edge's target vertex (when considering the dependencies as a directed graph).
		/// </summary>
		[NotNull]
		Resource IEdge<Resource>.Target => DependsOnResource;

		#endregion // IEdge<Resource>

		/// <summary>
		///		Build a dependency graph from the specified resources, preserving resource / dependency (vertex) identity.
		/// </summary>
		/// <param name="resources">
		///		The resources to examine.
		/// </param>
		/// <returns>
		///		A sequence of 0 or more <see cref="ResourceDependencyNode"/>s representing the dependencies.
		/// </returns>
		public static IEnumerable<ResourceDependencyNode> BuildDependencyGraph(IEnumerable<Resource> resources)
		{
			if (resources == null)
				throw new ArgumentNullException(nameof(resources));

			// TODO: Break up resources into dependency graph, preserving resource / vertex identity.

			yield break;
		} 
	}
}
