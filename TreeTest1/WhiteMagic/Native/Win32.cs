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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

namespace WhiteMagic.Native
{
    /// <summary>
    /// A simplistic Win32 API wrapper class.
    /// </summary>
    public class Win32 : IDisposable
    {
        private readonly IntPtr _processHandle;

        internal Win32(IntPtr procHandle)
        {
            _processHandle = procHandle != IntPtr.Zero
                                 ? procHandle 
                             // We are injected, so we share a process ID with whatever we're injected in.
                                 : OpenProcess(0x001F0FFF, true, Process.GetCurrentProcess().Id);
            // All access due to both laziness, and not caring.
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            CloseHandle(_processHandle);
        }

        #endregion

        [DllImport("kernel32.dll"), SuppressUnmanagedCodeSecurity]
        internal static extern bool FreeLibrary(IntPtr h);

        [DllImport("kernel32.dll", SetLastError = true), SuppressUnmanagedCodeSecurity]
        internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer,
                                                      int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll"), SuppressUnmanagedCodeSecurity]
        internal static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true), SuppressUnmanagedCodeSecurity]
        internal static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize,
                                                       out int lpNumberOfBytesWritten);

        [DllImport("kernel32"), SuppressUnmanagedCodeSecurity]
        internal static extern IntPtr LoadLibrary(string libraryName);

        [DllImport("kernel32", CharSet = CharSet.Ansi), SuppressUnmanagedCodeSecurity]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll", SetLastError = true), SuppressUnmanagedCodeSecurity]
        internal static extern bool CloseHandle(IntPtr hHandle);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public int WriteBytes(IntPtr address, byte[] val)
        {
            if (_processHandle == IntPtr.Zero)
            {
                throw new Exception("There's no current process handle... are you sure you did everything right?");
            }
            int written;
            if (WriteProcessMemory(_processHandle, address, val, (uint) val.Length, out written))
            {
                return written;
            }
            throw new AccessViolationException(string.Format("Could not write the specified bytes! {0} [{1}]", address.ToString("X8"),
                                                             Marshal.GetLastWin32Error()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] ReadBytes(IntPtr address, int count)
        {
            if (_processHandle == IntPtr.Zero)
            {
                throw new Exception("There's no current process handle... are you sure you did everything right?");
            }
            var ret = new byte[count];
            int numRead;
            if (ReadProcessMemory(_processHandle, address, ret, count, out numRead) && numRead == count)
            {
                return ret;
            }
            return null;
        }
    }
}