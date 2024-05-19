using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDB.ApiConstants;
using VNDBMetadata.Models;

namespace VNDBMetadata.Filters
{
	public class Vn
	{
		public class VnPredicate : SimpleFilterBase
		{
			internal VnPredicate(string filterName, string filterOperator, object value) : base(filterName, filterOperator, value)
			{

			}
		}

		public static class Id
		{
			public static string FilterName = VnConstants.Filters.Id;
			public static bool CanBeNull { get; } = false;
			public static VnPredicate EqualTo(params uint[] value) =>  new VnPredicate(
				FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null(value) : value);
			public static VnPredicate NotEqualTo(params uint[] value) => new VnPredicate(
				FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null(value) : value);
		}
	}

}