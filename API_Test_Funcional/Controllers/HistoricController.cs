﻿using API_Test.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace API_Test.Controllers
{
    public class HistoricController : ApiController
    {
        public static List<ListVehicle> ListVehicle = new List<ListVehicle>();
        public static List<Historic> Historic = new List<Historic>();
        
        public int Get()
        {
            if (ListVehicle.Count == 0) return -1;
            DateTime[] dates = new DateTime[ListVehicle.Count];
            for (int i = 0; i < ListVehicle.Count; i++)
            {
                 dates[i] = ListVehicle[i].dates;
            }
            return GetTollFee(dates);
        }

        public void Post(int vehicleId, string vehicleType, DateTime dates)
        {
            if (Convert.ToInt32(vehicleId)!= 0)
                Historic.Add(new Historic(vehicleId, vehicleType, dates));
        }

        public void Post(int vehicleId)
        {
            Historic result = Historic.Find(delegate (Historic h) { return h.vehicleId == vehicleId; });
              
            if (result != null)
            {
                for (int i = 0; i< Historic.Count; i++)
                {
                    if (Historic[i].vehicleId == vehicleId)
                    {
                        if (IsTollFreeVehicle(Historic[i].vehicleType)) { }
                            if (IsTollFreeDate(Historic[i].dates)){} 
                                else
                                {
                                    DateTime dates = Historic[i].dates;
                                    ListVehicle.Add(new ListVehicle(dates));
                                }
                    }
                }
            }
        }

        private int GetTollFee(DateTime[] dates)
        {
            DateTime intervalStart = dates[0];
            int totalFee = 0;
            foreach (DateTime date in dates)
            {
                int nextFee = GetTollFee(date);
                int tempFee = GetTollFee(intervalStart);

                long diffInMillies = date.Millisecond - intervalStart.Millisecond;
                long minutes = diffInMillies / 1000 / 60;

                if (minutes <= 60)
                {
                    if (totalFee > 0) totalFee -= tempFee;
                    if (nextFee >= tempFee) tempFee = nextFee;
                    totalFee += tempFee;
                }
                else
                {
                    totalFee += nextFee;
                }
            }
                if (totalFee > 60) totalFee = 60;
            return totalFee;
        }

        private int GetTollFee(DateTime date)
        {

            int hour = date.Hour;
            int minute = date.Minute;

            if (hour == 6 && minute >= 0 && minute <= 29) return 8;
            else if (hour == 6 && minute >= 30 && minute <= 59) return 13;
            else if (hour == 7 && minute >= 0 && minute <= 59) return 18;
            else if (hour == 8 && minute >= 0 && minute <= 29) return 13;
            else if (hour >= 8 && hour <= 14 && minute >= 30 && minute <= 59) return 8;
            else if (hour == 15 && minute >= 0 && minute <= 29) return 13;
            else if (hour == 15 && minute >= 0 || hour == 16 && minute <= 59) return 18;
            else if (hour == 17 && minute >= 0 && minute <= 59) return 13;
            else if (hour == 18 && minute >= 0 && minute <= 29) return 8;
            else return 0;
        }

        private Boolean IsTollFreeDate(DateTime date)
        {
            int year = date.Year;
            int month = date.Month;
            int day = date.Day;

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) return true;

            if (year == 2013)
            {
                if (month == 1 && day == 1 ||
                    month == 3 && (day == 28 || day == 29) ||
                    month == 4 && (day == 1 || day == 30) ||
                    month == 5 && (day == 1 || day == 8 || day == 9) ||
                    month == 6 && (day == 5 || day == 6 || day == 21) ||
                    month == 7 ||
                    month == 11 && day == 1 ||
                    month == 12 && (day == 24 || day == 25 || day == 26 || day == 31))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsTollFreeVehicle(string vehicleType)
        {
            return vehicleType.Equals(TollFreeVehicles.Motorbike.ToString()) ||
                   vehicleType.Equals(TollFreeVehicles.Tractor.ToString()) ||
                   vehicleType.Equals(TollFreeVehicles.Emergency.ToString()) ||
                   vehicleType.Equals(TollFreeVehicles.Diplomat.ToString()) ||
                   vehicleType.Equals(TollFreeVehicles.Foreign.ToString()) ||
                   vehicleType.Equals(TollFreeVehicles.Military.ToString());
        }

        private enum TollFreeVehicles
        {
            Motorbike = 0,
            Tractor = 1,
            Emergency = 2,
            Diplomat = 3,
            Foreign = 4,
            Military = 5
        }

    }
}

