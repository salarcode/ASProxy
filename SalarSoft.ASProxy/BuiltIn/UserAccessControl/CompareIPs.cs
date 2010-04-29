//**************************************************************************
// Product Name: ASProxy
// Description:  SalarSoft ASProxy is a free and open-source web proxy
// which allows the user to surf the net anonymously.
//
// MPL 1.1 License agreement.
// The contents of this file are subject to the Mozilla Public License
// Version 1.1 (the "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of the License at
// http://www.mozilla.org/MPL/
// 
// Software distributed under the License is distributed on an "AS IS"
// basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
// License for the specific language governing rights and limitations
// under the License.
// 
// The Original Code is ASProxy (an ASP.NET Proxy).
// 
// The Initial Developer of the Original Code is Salar.Kh (SalarSoft).
// Portions created by Salar.Kh are Copyright (C) 2010 All Rights Reserved.
// 
// Contributor(s): 
// Salar.Kh < salarsoftwares [@] gmail[.]com > (original author)
//
//**************************************************************************

// Note: I got this class from an article to compare IPs

using System;
using System.Net;

namespace SalarSoft.ASProxy.BuiltIn
{
    /// <summary>
    /// Used to convert and compare IP addresses.
    /// 
    /// By Alexander Lowe, Lowe*Software.
    /// http://www.lowesoftware.com
    /// 
    /// Free for use and modification. No warranty express or implied.
    /// </summary>
    public class CompareIPs
    {
        #region Public methods

        /// <summary>
        /// Compares two IP addresses for equality. 
        /// </summary>
        /// <param name="IPAddress1">The first IP to compare</param>
        /// <param name="IPAddress2">The second IP to compare</param>
        /// <returns>True if equal, false if not.</returns>
        static public bool AreEqual(string IPAddress1, string IPAddress2)
        {
            // convert to long in case there is any zero padding in the strings
            return IPAddressToLongBackwards(IPAddress1) == IPAddressToLongBackwards(IPAddress2);
        }

        /// <summary>
        /// Compares two string representations of an Ip address to see if one
        /// is greater than the other
        /// </summary>
        /// <param name="ToCompare">The IP address on the left hand side of the greater 
        /// than operator</param>
        /// <param name="CompareAgainst">The Ip address on the right hand side of the 
        /// greater than operator</param>
        /// <returns>True if ToCompare is greater than CompareAgainst, else false</returns>
        static public bool IsGreater(string ToCompare, string CompareAgainst)
        {
            // convert to long in case there is any zero padding in the strings
            return IPAddressToLongBackwards(ToCompare) > IPAddressToLongBackwards(CompareAgainst);
        }

        /// <summary>
        /// Compares two string representations of an Ip address to see if one
        /// is less than the other
        /// </summary>
        /// <param name="ToCompare">The IP address on the left hand side of the less 
        /// than operator</param>
        /// <param name="CompareAgainst">The Ip address on the right hand side of the 
        /// less than operator</param>
        /// <returns>True if ToCompare is greater than CompareAgainst, else false</returns>
        static public bool IsLess(string ToCompare, string CompareAgainst)
        {
            // convert to long in case there is any zero padding in the strings
            return IPAddressToLongBackwards(ToCompare) < IPAddressToLongBackwards(CompareAgainst);
        }


        /// <summary>
        /// Compares two string representations of an Ip address to see if one
        /// is greater than or equal to the other.
        /// </summary>
        /// <param name="ToCompare">The IP address on the left hand side of the greater 
        /// than or equal operator</param>
        /// <param name="CompareAgainst">The Ip address on the right hand side of the 
        /// greater than or equal operator</param>
        /// <returns>True if ToCompare is greater than or equal to CompareAgainst, else false</returns>
        static public bool IsGreaterOrEqual(string ToCompare, string CompareAgainst)
        {
            // convert to long in case there is any zero padding in the strings
            return IPAddressToLongBackwards(ToCompare) >= IPAddressToLongBackwards(CompareAgainst);
        }

        /// <summary>
        /// Compares two string representations of an Ip address to see if one
        /// is less than or equal to the other.
        /// </summary>
        /// <param name="ToCompare">The IP address on the left hand side of the less 
        /// than or equal operator</param>
        /// <param name="CompareAgainst">The Ip address on the right hand side of the 
        /// less than or equal operator</param>
        /// <returns>True if ToCompare is greater than or equal to CompareAgainst, else false</returns>
        static public bool IsLessOrEqual(string ToCompare, string CompareAgainst)
        {
            // convert to long in case there is any zero padding in the strings
            return IPAddressToLongBackwards(ToCompare) <= IPAddressToLongBackwards(CompareAgainst);
        }



        /// <summary>
        /// Converts a uint representation of an Ip address to a string.
        /// </summary>
        /// <param name="IPAddress">The IP address to convert</param>
        /// <returns>A string representation of the IP address.</returns>
        static public string LongToIPAddress(uint IPAddress)
        {
            return new System.Net.IPAddress(IPAddress).ToString();
        }

        /// <summary>
        /// Converts a string representation of an IP address to a uint. This
        /// encoding is proper and can be used with other networking functions such
        /// as the System.Net.IPAddress class.
        /// </summary>
        /// <param name="IPAddress">The Ip address to convert.</param>
        /// <returns>Returns a uint representation of the IP address.</returns>
        static public uint IPAddressToLong(string IPAddress)
        {
            System.Net.IPAddress oIP = System.Net.IPAddress.Parse(IPAddress);
            byte[] byteIP = oIP.GetAddressBytes();


            uint ip = (uint)byteIP[3] << 24;
            ip += (uint)byteIP[2] << 16;
            ip += (uint)byteIP[1] << 8;
            ip += (uint)byteIP[0];

            return ip;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// This encodes the string representation of an IP address to a uint, but
        /// backwards so that it can be used to compare addresses. This function is
        /// used internally for comparison and is not valid for valid encoding of
        /// IP address information.
        /// </summary>
        /// <param name="IPAddress">A string representation of the IP address to convert</param>
        /// <returns>Returns a backwards uint representation of the string.</returns>
        static private uint IPAddressToLongBackwards(string IPAddress)
        {
            System.Net.IPAddress oIP = System.Net.IPAddress.Parse(IPAddress);
            byte[] byteIP = oIP.GetAddressBytes();


            uint ip = (uint)byteIP[0] << 24;
            ip += (uint)byteIP[1] << 16;
            ip += (uint)byteIP[2] << 8;
            ip += (uint)byteIP[3];

            return ip;
        }

        #endregion
    }
}
