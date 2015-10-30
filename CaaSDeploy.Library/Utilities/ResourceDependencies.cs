using System;
using System.Collections.Generic;
using System.Linq;

using DD.CBU.CaasDeploy.Library.Models;
using QuickGraph;
using QuickGraph.Algorithms;

namespace DD.CBU.CaasDeploy.Library.Utilities
{
	/// <summary>
	///	Helper methods for working with inter-resource dependencies.
	/// </summary>
	internal static class ResourceDependencies
	{
        /// <summary>
        ///	Sort the specified resources in dependency order (most-dependent-first).
        /// </summary>
        /// <param name="resources">The resources to examine.</param>
        /// <returns>
        ///	A read-only list of <see cref="Resource"/>s in dependency order.
        /// </returns>
		public static IReadOnlyList<Resource> DependencySort(IReadOnlyCollection<Resource> resources)
		{
			if (resources == null)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            try
            {
                Dictionary<string, Resource> resourcesById = resources.ToDictionary(resource => resource.ResourceId);

                AdjacencyGraph<Resource, Edge<Resource>> resourceDependencies = new AdjacencyGraph<Resource, Edge<Resource>>();

                foreach (string resourceId in resourcesById.Keys)
                {
                    Resource resource = resourcesById[resourceId];
                    if (!resourceDependencies.AddVertex(resource))
                        continue; // Already processed.

                    if (resource.DependsOn != null)
                    {
                        foreach (string dependsOnResourceId in resource.DependsOn)
                        {
                            Resource dependsOnResource;
                            if (!resourcesById.TryGetValue(dependsOnResourceId, out dependsOnResource))
                            {
                                throw new TemplateParserException($"Resource '{resourceId}' depends on non-existent resource '{dependsOnResourceId}'.");
                            }

                            resourceDependencies.AddEdge(new Edge<Resource>(resource, dependsOnResource));
                        }
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
