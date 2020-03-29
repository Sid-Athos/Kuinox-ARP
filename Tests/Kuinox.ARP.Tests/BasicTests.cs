using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Kuinox.ARP.Tests
{
	public class Tests
	{
		[Test]
		public void running_arp_does_not_throw()
		{
            Func<ICollection<ArpInterface>> action = ARP.GetInterfaces;
            action.Should().NotThrow();
		}
	}
}
