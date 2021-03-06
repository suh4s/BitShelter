﻿using CronExpressionDescriptor;
using BitShelter.Models;
using System;
using System.Linq;
using BitShelter.Data;

namespace BitShelter.Agent.Helpers
{
  static class ScheduleUtils
  {
    public static string GetHumanizedSchedule(SnapshotRule rule)
    {
      string descriptorCron = GenerateCron(rule, true);

      string expr = ExpressionDescriptor.GetDescription(descriptorCron);

      if (rule.IsExcludingDayRange())
      {
        rule.GetExcludingDayRange(out DateTime from, out DateTime to);

        expr += String.Format(", Excluding from {0} to {1}",
          from.ToShortTimeString(),
          to.ToShortTimeString()
        );
      }

      return expr;
    }

    public static string GenerateCron(SnapshotRule sched, bool descriptorFix = false)
    {
      if (sched.Freq == Freq.Cron)
        return sched.FreqCron;

      string cron = "0 ";

      //
      // Minute Hour
      switch (sched.DailyFreq)
      {
        case DailyFreq.Once:
          cron += String.Format("{0} {1} ", sched.DailyFreqOnce.Minute, sched.DailyFreqOnce.Hour);
          break;

        case DailyFreq.Every:
          // Minutes
          if (sched.DailyFreqEvery % 60 != 0)
            cron += String.Format("*/{0} * ", sched.DailyFreqEvery);

          // Hours
          else
            cron += String.Format("0 */{0} ", sched.DailyFreqEvery / 60);

          break;

        default:
          //MessageBox.Show("Invalid daily frequency selection.");
          return string.Empty;
      }


      //
      // DayOfMonth Month DayOfWeek
      switch (sched.Freq)
      {
        case Freq.Daily:
          cron += String.Format("{0} * ?", (sched.FreqDailyEvery == 1 ? "*" : "*/" + sched.FreqDailyEvery));
          break;

        case Freq.Weekly:
          switch (sched.FreqWeekly)
          {
            case FreqWeekly.Weekly:
              cron += String.Format("{0} * ?", "*/" + sched.FreqWeeklyEvery * 7);
              break;

            case FreqWeekly.OnDays:
              cron += String.Format("? * {0}",
                String.Join(",", sched.FreqWeeklyDays.Select(i => (descriptorFix ? i : (i + 1)).ToString())));
              break;

            default:
              //MessageBox.Show("Invalid weekly schedule selection.");
              return string.Empty;
          }
          break;

        case Freq.Monthly:
          cron += String.Format("{0} {1} ?",
            String.Join(",", sched.FreqMonthlyDays.Select(i => (i + 1).ToString())),
            String.Join(",", sched.FreqMonthlyMonths.Select(i => (i + 1).ToString()))
          );
          break;

        default:
          //MessageBox.Show("Invalid frequency selection.");
          return string.Empty;
      }

      return cron;
    }
  }
}
