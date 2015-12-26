//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Elf
{   
    using System;
    using System.Text;


    public class StringTable : ElfSection
    {
        internal StringTable(ElfObject parent) : base(parent, new byte[1]{0})
        {
            m_header.sh_type = sh_type.SHT_STRTAB;
        }

        internal StringTable(ElfObject parent, Elf32_Shdr hdr, 
            UInt16 index, byte[] raw) : base(parent, hdr, index, raw, 0)
        {
        }

        public string GetString(uint index)
        {
            if (m_raw == null || index >= m_raw.Length)
            {
                return "";
            }

            var sb = new StringBuilder();

            while (m_raw[index] != 0)
            {
                sb.Append((char)m_raw[index]);
                index++;
            }

            return sb.ToString();
        }

        public uint AddString(string val)
        {
            uint index = 0;

            // First see if we already have this string in the table and reuse it
            if (m_raw != null && m_raw.Length > 0)
            {
                var sb    = new StringBuilder();

                while (true)
                {
                    while (m_raw[index] != 0)
                    {
                        sb.Append((char)m_raw[index]);
                        index++;
                    }

                    var str = sb.ToString();

                    // We need to check for matching suffix strings and use
                    // one if we find one.
                    for (int i = 0; i < str.Length; i++)
                    {
                        if (str.Substring(i) == val)
                        {
                            return index - (uint)(str.Length - i);
                        }
                    }

                    //if (sb.ToString() == val)
                    //{
                    //    return index - (uint)sb.Length;
                    //}

                    if (++index >= m_raw.Length)
                    {
                        break;
                    }

                    sb = new StringBuilder();
                }
            }

            // We don't already have the string in the table. Resize the table, and add it.
            index = (uint)m_raw.Length;

            Array.Resize(ref m_raw, m_raw.Length + val.Length + 1);

            var enc = new ASCIIEncoding();

            Buffer.BlockCopy(enc.GetBytes(val), 0, m_raw, (int)index, val.Length);

            m_raw[m_raw.Length - 1] = 0;

            return index;
        }
    }
}