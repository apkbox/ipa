// --------------------------------------------------------------------------------
// <copyright file="DailySchedule.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the DailySchedule type.
// </summary>
// --------------------------------------------------------------------------------

namespace SimulationCore.Model
{
    using System;

    public class DailySchedule : ISchedule
    {
        #region Fields

        private DateTime? nextDate;

        #endregion

        #region Public Methods and Operators

        public bool IsArrived(DateTime date)
        {
            if (this.nextDate == null)
            {
                this.nextDate = date;
            }

            if (this.nextDate == date)
            {
                this.nextDate = date.AddDays(1);
                return true;
            }

            return false;
        }

        #endregion
    }
}