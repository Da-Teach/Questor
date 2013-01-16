#region License/Copyright

// WhiteMagic - Injected .NET Helper Library
//     Copyright (C) 2009 Apoc
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Runtime.Serialization;

namespace WhiteMagic
{
    ///<summary>
    /// An exception that is thrown when a struct, class, or delegate is missing proper attributes.
    ///</summary>
    [Serializable]
    public class MissingAttributeException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        /// <summary>
        /// 
        /// </summary>
        public MissingAttributeException()
        {
            
        }

        ///<summary>
        ///</summary>
        ///<param name="message"></param>
        public MissingAttributeException(string message) : base(message)
        {
        }

        ///<summary>
        ///</summary>
        ///<param name="message"></param>
        ///<param name="inner"></param>
        public MissingAttributeException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected MissingAttributeException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}