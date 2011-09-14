using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Wev.Core
{
	/// <summary>
	/// Represents a collection of <see cref="ServerElement"/> items.
	/// </summary>
	public class ServerElementCollection : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new ServerElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((ServerElement)element).Url;
		}
	}
}