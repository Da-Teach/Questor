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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

using WhiteMagic.Native;


#if X64
using ADDR = System.UInt64;
#else
using ADDR = System.UInt32;
#endif

namespace WhiteMagic.Internals
{
    /// <summary>
    /// Credits to Dominik, Patrick, Bobbysing, and whoever else I forgot, for most of the ripped code here!
    /// </summary>
    public class PatternManager
    {
        private readonly Dictionary<string, IntPtr> _patterns = new Dictionary<string, IntPtr>();
        private readonly Win32 _win32;

        internal PatternManager(Win32 win32)
        {
            _win32 = win32;
        }

        /// <summary>
        /// Retrieves an address from the found patterns stash.
        /// </summary>
        /// <param name="name">The name of the pattern, as per the XML file provided in the constructor of this class instance.</param>
        /// <returns></returns>
        public IntPtr this[string name] { get { return _patterns[name]; } }

        /// <summary>
        /// Loads a pattern file.
        /// </summary>
        /// <param name="file">The full path to the file to be loaded. (XML files only!)</param>
        /// <param name="start">The start address to begin scanning from.</param>
        /// <param name="length">The length of data to scan.</param>
        public void LoadFile(string file, ADDR start, ADDR length)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException("file");
            }
            if (start == 0)
            {
                throw new ArgumentOutOfRangeException("start", "Start address cannot be 0!");
            }
            if (length == 0)
            {
                throw new ArgumentOutOfRangeException("length", "Length cannot be 0!");
            }

            LoadFile(XElement.Load(file), _win32.ReadBytes((IntPtr) start, (int) length), start);
        }

        /// <summary>
        /// Loads an XML pattern file, and scans a specific module.
        /// </summary>
        /// <param name="file">The full path to the file to be loaded. (XML files only!)</param>
        /// <param name="hModule">The base address, or handle, to a module to scan. (Length/start will be calculated automatically)</param>
        public void LoadFile(string file, IntPtr hModule)
        {
            if (hModule == IntPtr.Zero)
            {
                throw new ArgumentException("hModule cannot be 0!", "hModule");
            }

            var pe = new PeHeaderParser(hModule);

            var start = (ADDR) (pe.ModulePtr.ToInt32() + pe.NtHeader.OptionalHeader.BaseOfCode);
            ADDR length = pe.NtHeader.OptionalHeader.BaseOfData - 2 - pe.NtHeader.OptionalHeader.BaseOfCode;

            LoadFile(file, start, length);
        }

        /// <summary>
        /// Loads an XML pattern file, and scans the entry assembly. (The first module in the modules list)
        /// </summary>
        /// <param name="file">The full path to the file to be loaded. (XML files only!)</param>
        public void LoadFile(string file)
        {
            LoadFile(file, Process.GetCurrentProcess().Modules[0].BaseAddress);
        }

        private void LoadFile(XContainer file, byte[] data, ADDR start)
        {
            // Grab all the <Pattern /> elements from the XML.
            IEnumerable<XElement> pats = from p in file.Descendants("Pattern")
                                         select p;

            // Each Pattern element needs to be handled seperately.
            // The enumeration we're goinv over, is in document order, so attributes such as 'start'
            // should work perfectly fine.
            foreach (XElement pat in pats)
            {
                ADDR tmpStart = 0;

                string name = pat.Attribute("desc").Value;
                string mask = pat.Attribute("mask").Value;
                byte[] patternBytes = GetBytesFromPattern(pat.Attribute("pattern").Value);

                // Make sure we're not getting some sort of screwy XML data.
                if (mask.Length != patternBytes.Length)
                {
                    throw new Exception("Pattern and mask lengths do not match!");
                }

                // If we run into a 'start' attribute, we need to remember that we're working from a 0
                // based 'memory pool'. So we just remove the 'start' from the address we found earlier.
                if (pat.Attribute("start") != null)
                {
                    tmpStart = (ADDR) (this[pat.Attribute("start").Value].ToInt32() - start + 1);
                }

                // Actually search for the pattern match...
                ADDR found = Find(data, mask, patternBytes, tmpStart);

                if (found == 0)
                {
                    throw new Exception("FindPattern failed... figure it out ****tard!");
                }

                // Handle specific child elements for the pattern.
                // <Lea> <Rel> <Add> <Sub> etc
                foreach (XElement e in pat.Elements())
                {
                    switch (e.Name.LocalName)
                    {
                        case "Lea":
                            found = BitConverter.ToUInt32(data, (int) found);
                            break;
                        case "Rel":
                            int instructionSize = int.Parse(e.Attribute("size").Value, NumberStyles.HexNumber);
                            int operandOffset = int.Parse(e.Attribute("offset").Value, NumberStyles.HexNumber);
                            found = (ADDR) (BitConverter.ToUInt32(data, (int) found) + found + instructionSize - operandOffset);
                            break;
                        case "Add":
                            found += ADDR.Parse(e.Attribute("value").Value, NumberStyles.HexNumber);
                            break;
                        case "Sub":
                            found -= ADDR.Parse(e.Attribute("value").Value, NumberStyles.HexNumber);
                            break;
                    }
                }

                _patterns.Add(name, (IntPtr) (found + start));
            }
        }

        private static byte[] GetBytesFromPattern(string pattern)
        {
            // Because I'm lazy, and this just makes life easier.
            string[] split = pattern.Split(new[] {'\\', 'x'}, StringSplitOptions.RemoveEmptyEntries);
            var ret = new byte[split.Length];
            for (int i = 0; i < split.Length; i++)
            {
                ret[i] = byte.Parse(split[i], NumberStyles.HexNumber);
            }
            return ret;
        }

        private static ADDR Find(byte[] data, string mask, byte[] byteMask, ADDR start)
        {
            // There *has* to be a better way to do this stuff,
            // but for now, we'll deal with it.
            for (ADDR i = start; i < data.Length; i++)
            {
                if (DataCompare(data, (int) i, byteMask, mask))
                {
                    return i;
                }
            }
            return 0;
        }

        private static bool DataCompare(byte[] data, int offset, byte[] byteMask, string mask)
        {
            // Only check for 'x' mismatches. As we'll assume anything else is a wildcard.
            for (int i = 0; i < mask.Length; i++)
            {
                if (mask[i] == 'x' && byteMask[i] != data[i + offset])
                {
                    return false;
                }
            }
            return true;
        }
    }
}