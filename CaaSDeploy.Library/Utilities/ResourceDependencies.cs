using CaasDeploy.Library.Models;
using JetBrains.Annotations;
using QuickGraph;
using QuickGraph.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CaasDeploy.Library.Utilities
{
	/// <summary>
	///		Helper methods for working with inter-resource dependencies.
	/// </summary>
	public static class ResourceDependencies
	{
		/// <summary>
		///		Sort the specified resources in dependency order (most-dependent-first).
		/// </summary>
		/// <param name="resources">
		///		The resources to examine.
		/// </param>
		/// <returns>
		///		A read-only list of <see cref="Resource"/>s in dependency order.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		///		One of the supplied resources declares a dependency on a resource that is not amongst the supplied resources.
		///		OR
		///		One of the supplied resources has a cyclic (recursive) dependency.
		/// </exception>
		[NotNull]
		public static IReadOnlyList<Resource> DependencySort([NotNull] this IReadOnlyCollection<Resource> resources)
		{
			if (resources == null)
				throw new ArgumentNullException(nameof(resources));

			AdjacencyGraph<Resource, Edge<Resource>> resourceDependencies = new AdjacencyGraph<Resource, Edge<Resource>>();
			
			Dictionary<string, Resource> resourcesById = resources.ToDictionary(resource => resource.resourceId);
			foreach (string resourceId in resourcesById.Keys)
			{
				Resource resource = resourcesById[resourceId];
				if (!resourceDependencies.AddVertex(resource))
					continue; // Already processed.

				foreach (string dependsOnResourceId in resource.dependsOn)
				{
					Resource dependsOnResource;
					if (!resourcesById.TryGetValue(dependsOnResourceId, out dependsOnResource))
					{
						throw new InvalidOperationException(
							$"Resource '{resourceId}' depends on non-existent resource '{dependsOnResourceId}'."
						);
					}

					resourceDependencies.AddEdge(
						new Edge<Resource>(resource, dependsOnResource)
					);
				}
			}

			Resource[] sortedResources =
				resourceDependencies
					.TopologicalSort()
					.ToArray();
			
			return sortedResources;
		} 
	}
}
