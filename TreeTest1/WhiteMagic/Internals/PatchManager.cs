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

using WhiteMagic.Native;

namespace WhiteMagic.Internals
{
    /// <summary>
    /// A manager class to handle memory patches.
    /// </summary>
    public class PatchManager : Manager<Patch>
    {
        internal PatchManager(Win32 win32) : base(win32)
        {
        }

        /// <summary>
        /// Creates a new <see cref="Patch"/> at the specified address.
        /// </summary>
        /// <param name="address">The address to begin the patch.</param>
        /// <param name="patchWith">The bytes to be written as the patch.</param>
        /// <param name="name">The name of the patch.</param>
        /// <returns>A patch object that exposes the required methods to apply and remove the patch.</returns>
        public Patch Create(IntPtr address, byte[] patchWith, string name)
        {
#if !NOEXCEPTIONS
            if (address == IntPtr.Zero)
            {
                throw new ArgumentException("Address cannot be 0!", "address");
            }
            if (patchWith == null || patchWith.Length == 0)
            {
                throw new ArgumentNullException("patchWith", "Patch bytes cannot be null, or 0 bytes long!");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
#endif

            if (!Applications.ContainsKey(name))
            {
                return new Patch(address, patchWith, name, Win32);
            }
            return null;
        }

        /// <summary>
        /// Creates a new <see cref="Patch"/> at the specified address, and applies it.
        /// </summary>
        /// <param name="address">The address to begin the patch.</param>
        /// <param name="patchWith">The bytes to be written as the patch.</param>
        /// <param name="name">The name of the patch.</param>
        /// <returns>A patch object that exposes the required methods to apply and remove the patch.</returns>
        public Patch CreateAndApply(IntPtr address, byte[] patchWith, string name)
        {
            Patch p = Create(address, patchWith, name);
            if (p != null)
            {
                p.Apply();
            }
            return p;
        }
    }

    /// <summary>
    /// Contains methods, and information for a memory patch.
    /// </summary>
    public class Patch : IMemoryOperation
    {
        private readonly IntPtr _address;
        private readonly byte[] _originalBytes;
        private readonly byte[] _patchBytes;
        private readonly Win32 _win32;

        internal Patch(IntPtr address, byte[] patchWith, string name, Win32 win)
        {
            Name = name;
            _win32 = win;
            _address = address;
            _patchBytes = patchWith;
            _originalBytes = _win32.ReadBytes(address, patchWith.Length);
        }

        #region IMemoryOperation Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (IsApplied)
            {
                Remove();
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Removes this Patch from memory. (Reverts the bytes back to their originals.)
        /// </summary>
        /// <returns></returns>
        public bool Remove()
        {
            if (_win32.WriteBytes(_address, _originalBytes) == _originalBytes.Length)
            {
                IsApplied = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Applies this Patch to memory. (Writes new bytes to memory)
        /// </summary>
        /// <returns></returns>
        public bool Apply()
        {
            if (_win32.WriteBytes(_address, _patchBytes) == _patchBytes.Length)
            {
                IsApplied = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if this Patch is currently applied.
        /// </summary>
        public bool IsApplied { get; private set; }

        /// <summary>
        /// Returns the name for this Patch.
        /// </summary>
        public string Name { get; private set; }

        #endregion

        /// <summary>
        /// Allows an <see cref="T:System.Object"/> to attempt to free resources and perform other cleanup operations before the <see cref="T:System.Object"/> is reclaimed by garbage collection.
        /// </summary>
        ~Patch()
        {
            Dispose();
        }
    }
}