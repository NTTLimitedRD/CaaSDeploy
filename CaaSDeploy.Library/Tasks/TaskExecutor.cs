﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CaasDeploy.Library.Models;

namespace CaasDeploy.Library.Tasks
{
    /// <summary>
    /// Contains the tasks and the context to execute them.
    /// </summary>
    public sealed class TaskExecutor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskExecutor"/> class.
        /// </summary>
        /// <param name="tasks">The tasks to execute.</param>
        /// <param name="context">The task execution context.</param>
        public TaskExecutor(IList<ITask> tasks, TaskContext context)
        {
            if (tasks == null)
            {
                throw new ArgumentNullException(nameof(tasks));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Tasks = tasks;
            Context = context;
        }

        /// <summary>
        /// Gets the tasks to execute.
        /// </summary>
        public IList<ITask> Tasks { get; private set; }

        /// <summary>
        /// Gets the task execution context.
        /// </summary>
        public TaskContext Context { get; private set; }

        /// <summary>
        /// Executes the tasks.
        /// </summary>
        /// <returns>The async <see cref="Task"/> with the deployment log.</returns>
        public async Task<DeploymentLog> Execute()
        {
            foreach (var task in Tasks)
            {
                try
                {
                    await task.Execute(Context);

                    if (Context.Log.status == DeploymentLogStatus.Failed)
                    {
                        return Context.Log;
                    }
                }
                catch (Exception)
                {
                    Context.Log.status = DeploymentLogStatus.Failed;
                    throw;
                }
            }

            Context.Log.status = DeploymentLogStatus.Success;
            return Context.Log;
        }
    }
}