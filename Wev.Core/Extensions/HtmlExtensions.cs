using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Wev.Core
{
	public static class HtmlExtensions
	{
		/// <summary>
		/// Creates a drop down list from an <c>IList</c> and selects the item.
		/// </summary>
		public static MvcHtmlString DropDownFromList(this HtmlHelper helper, string name, IList<string> items, string selectedValue)
		{
			List<SelectListItem> selectList = new List<SelectListItem>();
			
			foreach (string item in items)
			{
				SelectListItem selectListItem = new SelectListItem
				{
					Text = item,
					Value = item
				};

				if (item == selectedValue)
					selectListItem.Selected = true;

				selectList.Add(selectListItem);
			}

			return helper.DropDownList(name, selectList);
		}

		/// <summary>
		/// Creates a drop down list from an <c>IDictionary</c> and selects the item.
		/// </summary>
		public static MvcHtmlString DropDownBox(this HtmlHelper helper, string name, IDictionary<string, string> items, string selectedValue)
		{
			List<SelectListItem> selectList = new List<SelectListItem>();
			
			foreach (string key in items.Keys)
			{
				SelectListItem selectListItem = new SelectListItem
				{
					Text = items[key],
					Value = key
				};

				if (key == selectedValue)
					selectListItem.Selected = true;

				selectList.Add(selectListItem);
			}

			return helper.DropDownList(name, selectList);
		}
	}
}