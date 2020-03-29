using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Kuinox.ARP.Tests
{
	public class Tests
	{

		[Test]
		public void running_arp_does_not_throw_if_english_language_throws_otherwise()
		{

            Func<ICollection<ArpInterface>> action = ARP.GetInterfaces;
			if( CultureInfo.InstalledUICulture.EnglishName.StartsWith("English") )
			{
				action.Should().NotThrow();
			} 
			else
			{
				action.Should().Throw<FormatException>();
			}

		}
	}
}
