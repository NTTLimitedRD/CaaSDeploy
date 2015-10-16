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
		/// <param name="existingResources">
		///		The existing resources to examine.
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
		public static IReadOnlyList<Resource> DependencySort([NotNull] IReadOnlyCollection<Resource> resources, [NotNull] IReadOnlyCollection<ExistingResource> existingResources)
		{
			if (resources == null)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            try
            {
                Dictionary<string, Resource> resourcesById = resources.ToDictionary(resource => resource.resourceId);
                Dictionary<string, ExistingResource> existingResourcesById = existingResources.ToDictionary(resource => resource.resourceId);

                AdjacencyGraph<Resource, Edge<Resource>> resourceDependencies = new AdjacencyGraph<Resource, Edge<Resource>>();

                foreach (string resourceId in resourcesById.Keys)
                {
                    Resource resource = resourcesById[resourceId];
                    if (!resourceDependencies.AddVertex(resource))
                        continue; // Already processed.

                    foreach (string dependsOnResourceId in resource.dependsOn)
                    {
                        if (existingResourcesById.ContainsKey(dependsOnResourceId))
                        {
                            continue;
                        }

                        Resource dependsOnResource;
                        if (!resourcesById.TryGetValue(dependsOnResourceId, out dependsOnResource))
                        {
                            throw new TemplateParserException(
                                $"Resource '{resourceId}' depends on non-existent resource '{dependsOnResourceId}'."
                            );
                        }

                        resourceDependencies.AddEdge(
                            new Edge<Resource>(resource, dependsOnResource)
                        );
                    }
                }

                return resourceDependencies.TopologicalSort().ToList();
            }
            catch (NonAcyclicGraphException ex)
            {
                throw new TemplateParserException("The template contains a circular dependency.", ex);
            }
		} 
	}
}
