﻿/* 
  	* XmlNamespaces.cs
	* [ part of MVP-XML library: http://sourceforge.net/projects/mvp-xml ]
	* Author: Daniel Cazzulino, kzu@aspnet2.com
	* License: BSD-License (see below)
    
	Copyright (c) 2003, 2004 Daniel Cazzulino, kzu@aspnet2.com
    All rights reserved.

    Redistribution and use in source and binary forms, with or without
    modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice,
    * this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
    * notice, this list of conditions and the following disclaimer in the
    * documentation and/or other materials provided with the distribution.
    * Neither the name of the copyright owner nor the names of its
    * contributors may be used to endorse or promote products derived from
    * this software without specific prior written permission.

    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
    AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
    IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
    ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
    LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
    CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
    SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
    INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
    CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
    ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
    POSSIBILITY OF SUCH DAMAGE.
*/
#region using



#endregion using

namespace XBLMS.Core.Utils.Serialization.MvpXml
{
    /// <summary>
    /// Provides public constants for wellknown XML namespaces.
    /// </summary>
    /// <remarks>Author: Daniel Cazzulino, kzu@aspnet2.com</remarks>
    internal sealed class XmlNamespaces
	{
		#region Ctor

		private XmlNamespaces() { }

		#endregion Ctor

		#region Public Constants

		/// <summary>
		/// The public XML 1.0 namespace. 
		/// </summary>
		/// <remarks>See http://www.w3.org/TR/2004/REC-xml-20040204/</remarks>
		public const string Xml = "http://www.w3.org/XML/1998/namespace";

		/// <summary>
		/// Public Xml Namespaces specification namespace. 
		/// </summary>
		/// <remarks>See http://www.w3.org/TR/REC-xml-names/</remarks>
		public const string XmlNs = "http://www.w3.org/2000/xmlns/";

		/// <summary>
		/// Public Xml Namespaces prefix. 
		/// </summary>
		/// <remarks>See http://www.w3.org/TR/REC-xml-names/</remarks>
		public const string XmlNsPrefix = "xmlns";

		/// <summary>
		/// XML Schema instance namespace.
		/// </summary>
		/// <remarks>See http://www.w3.org/TR/xmlschema-1/</remarks>
		public const string Xsi = "http://www.w3.org/2001/XMLSchema-instance";

		/// <summary>
		/// XML 1.0 Schema namespace.
		/// </summary>
		/// <remarks>See http://www.w3.org/TR/xmlschema-1/</remarks>
		public const string Xsd = "http://www.w3.org/2001/XMLSchema";

		#endregion Public Constants
	}
}
