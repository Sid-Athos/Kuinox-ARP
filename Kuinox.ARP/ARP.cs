using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;

namespace Kuinox.ARP
{
    public static class ARP
    {
        /// <summary>
        /// Parse the ARP output of 'arp -a' on windows.
        /// </summary>
        /// <param name="reader">The <see cref="StreamReader"/> containing the Standard Output of 'arp -a'.</param>
        /// <returns>An object representing the ARP interfaces.</returns>
        public static ICollection<ArpInterface> GetInterfaces()
        {
            if( Environment.OSVersion.Platform != PlatformID.Win32NT ) throw new NotImplementedException( "TODO." );
      
            if ( !CultureInfo.InstalledUICulture.EnglishName.StartsWith( "English" ) ) throw new FormatException( "ARP library only supports english language so far." );

            using ( Process process = Process.Start( new ProcessStartInfo( "arp" , "-a" )
            {
                RedirectStandardOutput = true
            } ) )
            {
                process.WaitForExit();
                using( var reader = process.StandardOutput )
                {
                    return WinParseArp( reader );
                }
            }
        }

        
        static ICollection<ArpInterface> WinParseArp( StreamReader reader )
        {
            reader.ReadLine();//useless line.
            List<ArpInterface> entries = new List<ArpInterface>();
            while( !reader.EndOfStream )
            {
                entries.Add( WinReadArpEntry( reader ) );
            }
            return entries;
        }

        
        static ArpInterface WinReadArpEntry( StreamReader reader )
        {
            string headerLine = reader.ReadLine();
            string[] headerEntries = headerLine.Split( ' ' );
            IPAddress address = IPAddress.Parse( headerEntries[1] );
            string maskHexNumber = headerEntries[3].Split( 'x' )[1];
            byte mask = byte.Parse( maskHexNumber, NumberStyles.HexNumber );
            reader.ReadLine();//useless line.
            var entries = new List<InterfaceArpEntry>();
            while( true )
            {
                string line = reader.ReadLine();
                if( line == null ) break;//We rechead EndOfStream.

                string[] lineEntries = line.Split( ' ', StringSplitOptions.RemoveEmptyEntries );
                if( lineEntries.Length == 0 ) break;//Arp Interfaces are separated by a blank line.

                IPAddress ipAddress = IPAddress.Parse( lineEntries[0] );
                string physicalAddress = lineEntries[1].ToUpperInvariant();//PhysicalAddress.Parse only accept upper case addresses.
                bool isDynamic;
                if( lineEntries[2] == "dynamic" )
                {
                    isDynamic = true;
                }
                else if( lineEntries[2] == "static" )
                {
                    isDynamic = false;
                }
                else
                {
                    throw new InvalidDataException( "IP is neither dynamic or static." );
                }
                InterfaceArpEntry entry = new InterfaceArpEntry( ipAddress, PhysicalAddress.Parse( physicalAddress ), isDynamic );
                entries.Add( entry );
            }
            return new ArpInterface( address, mask, entries );
        }
    }

    /// <summary>
    /// Represent and arp entry of an interface in the ARP table.
    /// </summary>
    public class InterfaceArpEntry
    {
        internal InterfaceArpEntry( IPAddress ipAddress, PhysicalAddress physicalAddress, bool isDynamic )
        {
            IPAddress = ipAddress;
            PhysicalAddress = physicalAddress;
            IsDynamic = isDynamic;
        }

        /// <summary>
        /// IP Address of the entry.
        /// </summary>
        public IPAddress IPAddress { get; }

        /// <summary>
        /// MAC Address of the entry.
        /// </summary>
        public PhysicalAddress PhysicalAddress { get; }

        /// <summary>
        /// True if IP is dynamic, else IP is static.
        /// </summary>
        public bool IsDynamic { get; }
    }

    /// <summary>
    /// Represent an interface in the ARP table.
    /// </summary>
    public class ArpInterface
    {
        internal ArpInterface( IPAddress interfaceIPAddress, byte mask, ICollection<InterfaceArpEntry> arpEntries )
        {
            InterfaceIPAddress = interfaceIPAddress;
            Mask = mask;
            ArpEntries = arpEntries;
        }

        /// <summary>
        /// IP Address of the Interface.
        /// </summary>
        public IPAddress InterfaceIPAddress { get; }

        /// <summary>
        /// Mask of the Interface.
        /// </summary>
        public byte Mask { get; }

        /// <summary>
        /// Arp Entries of this interface.
        /// </summary>
        public ICollection<InterfaceArpEntry> ArpEntries { get; }
    }
}
