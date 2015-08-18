using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CaasDeploy.Library.Tests
{
	/// <summary>
	///		Assertion methods for working with exceptions.
	/// </summary>
	public static class ExceptionAssert
	{
		/// <summary>
		///		Assert that the specified test action throws an exception of type <typeparamref name="TExpectedException"/>.
		/// </summary>
		/// <typeparam name="TExpectedException">
		///		The expected exception type.
		/// </typeparam>
		/// <param name="testAction">
		///		A delegate that implements the test action.
		/// </param>
		/// <returns>
		///		The exception under test.
		/// </returns>
		[NotNull]
		public static TExpectedException Throws<TExpectedException>(Action testAction)
			where TExpectedException : Exception
		{
			return Throws<TExpectedException>(testAction, "Test action failed to throw expected exception type '{0}'.", typeof(TExpectedException).FullName);
		}

		/// <summary>
		///		Assert that the specified test action throws an exception of type <typeparamref name="TExpectedException"/>.
		/// </summary>
		/// <typeparam name="TExpectedException">
		///		The expected exception type.
		/// </typeparam>
		/// <param name="testAction">
		///		A delegate that implements the test action.
		/// </param>
		/// <param name="messageOrFormat">
		///		The assertion message or message-format specifier.
		/// </param>
		/// <param name="formatArguments">
		///		Optional message format arguments.
		/// </param>
		/// <returns>
		///		The exception under test.
		/// </returns>
		[NotNull]
		[StringFormatMethod("messageOrFormat")]
		public static TExpectedException Throws<TExpectedException>(Action testAction, string messageOrFormat, params object[] formatArguments)
			where TExpectedException : Exception
		{
			if (testAction == null)
				throw new ArgumentNullException(nameof(testAction));

			try
			{
				testAction();

				Assert.Fail(messageOrFormat, formatArguments);

				// ReSharper disable once HeuristicUnreachableCode
				return null; // Never reached.
			}
			catch (TExpectedException expectedException)
			{
				return expectedException;
			}
		}
	}
}
