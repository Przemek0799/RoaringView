using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RoaringView.Service
{
    public class SortingService
    {
        public List<T> SortData<T>(List<T> data, string columnName, bool ascending)
        {
            PropertyInfo property = typeof(T).GetProperty(columnName);

            if (property == null) throw new ArgumentException("Invalid column name.");

            if (ascending)
            {
                return data.OrderBy(x => property.GetValue(x, null)).ToList();
            }
            else
            {
                return data.OrderByDescending(x => property.GetValue(x, null)).ToList();
            }
        }
    }
}
