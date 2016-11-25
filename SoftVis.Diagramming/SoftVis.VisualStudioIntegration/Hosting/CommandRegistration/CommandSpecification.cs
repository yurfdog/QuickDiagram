﻿using System;
using Codartis.SoftVis.VisualStudioIntegration.App.Commands;

namespace Codartis.SoftVis.VisualStudioIntegration.Hosting.CommandRegistration
{
    /// <summary>
    /// Describes a command's ID and type.
    /// </summary>
    /// <typeparam name="T">The type of the command.</typeparam>
    internal struct CommandSpecification<T> : ICommandSpecification
        where T : ParameterlessCommandBase
    {
        public int CommandId { get; }

        public CommandSpecification(int commandId)
        {
            CommandId = commandId;
        }

        public Type CommandType => typeof(T);
    }
}