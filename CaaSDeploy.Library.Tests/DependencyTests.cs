using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using CaasDeploy.Library.Models;

namespace CaasDeploy.Library.Tests
{
	using System;
	using Utilities;

	/// <summary>
	///		Test suite for resource dependency calculations.
	/// </summary>
	[TestClass]
	public class DependencyTests
	{
		/// <summary>
		///		The current test execution context.
		/// </summary>
		public TestContext TestContext
		{
			get;
			set;
		}

		/// <summary>
		///		Verify that a single resource can be sorted in dependency order.
		/// </summary>
		[TestMethod]
		[TestCategory("Resources")]
		[TestCategory("Dependencies")]
		public void Can_Sort_Resource_Single()
		{
			Resource[] resources =
			{
				new Resource
				{
					resourceId = "resource-1",
					dependsOn = new List<string>()
				}
			};

			IReadOnlyList<Resource> sortedResources = resources.DependencySort();
			Assert.IsNotNull(sortedResources);
			Assert.AreEqual(1, sortedResources.Count);
			Assert.AreSame(resources[0], sortedResources[0]);
		}

		/// <summary>
		///		Verify that a resource cannot be sorted in dependency order if it depends on a non-existent resource.
		/// </summary>
		[TestMethod]
		[TestCategory("Resources")]
		[TestCategory("Dependencies")]
		public void Cannot_Sort_Resource_Dependency_NonExistent()
		{
			Resource[] resources =
			{
				new Resource
				{
					resourceId = "resource-1",
					dependsOn = new List<string>
					{
						"resource-2"
					}
				}
			};

			// AF: Actually, this is a pretty lazy exception type to throw.
			ExceptionAssert.Throws<InvalidOperationException>(
				() => resources.DependencySort()
			);
		}
	}
}
