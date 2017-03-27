// --------------------------------------------------------------------------------
// <copyright file="ISchedule.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the ISchedule type.
// </summary>
// --------------------------------------------------------------------------------

namespace SimulationCore.Model
{
    using System;

    public interface ISchedule
    {
        #region Public Methods and Operators

        bool IsArrived(DateTime date);

        #endregion
    }
}