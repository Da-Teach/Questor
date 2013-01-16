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
using System.Collections.Generic;

using WhiteMagic.Native;

namespace WhiteMagic.Internals
{
    /// <summary>
    /// Represents an operation in memory, be it a patch, detour, or anything else.
    /// </summary>
    public interface IMemoryOperation : IDisposable
    {
        /// <summary>
        /// Returns true if this IMemoryOperation is currently applied.
        /// </summary>
        bool IsApplied { get; }
        /// <summary>
        /// Returns the name for this IMemoryOperation.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Removes this IMemoryOperation from memory. (Reverts the bytes back to their originals.)
        /// </summary>
        /// <returns></returns>
        bool Remove();
        /// <summary>
        /// Applies this IMemoryOperation to memory. (Writes new bytes to memory)
        /// </summary>
        /// <returns></returns>
        bool Apply();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Manager<T> where T : IMemoryOperation
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal Dictionary<string, T> Applications = new Dictionary<string, T>();

        internal Manager(Win32 win32)
        {
            Win32 = win32;
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal Win32 Win32 { get; set; }

        /// <summary>
        /// Retrieves an IMemoryOperation by name.
        /// </summary>
        /// <param name="name">The name given to the IMemoryOperation</param>
        /// <returns></returns>
        public virtual T this[string name] { get { return Applications[name]; } }

        /// <summary>
        /// Applies all the IMemoryOperations contained in this manager via their Apply() method.
        /// </summary>
        public virtual void ApplyAll()
        {
            foreach (var dictionary in Applications)
            {
                dictionary.Value.Apply();
            }
        }

        /// <summary>
        /// Removes all the IMemoryOperations contained in this manager via their Remove() method.
        /// </summary>
        public virtual void RemoveAll()
        {
            foreach (var dictionary in Applications)
            {
                dictionary.Value.Remove();
            }
        }

        /// <summary>
        /// Deletes all the IMemoryOperations contained in this manager.
        /// </summary>
        public virtual void DeleteAll()
        {
            foreach (var dictionary in Applications)
            {
                dictionary.Value.Dispose();
            }
            Applications.Clear();
        }

        /// <summary>
        /// Deletes a specific IMemoryOperation contained in this manager, by name.
        /// </summary>
        /// <param name="name"></param>
        public virtual void Delete(string name)
        {
            if (Applications.ContainsKey(name))
            {
                Applications[name].Dispose();
                Applications.Remove(name);
            }
        }
    }
}