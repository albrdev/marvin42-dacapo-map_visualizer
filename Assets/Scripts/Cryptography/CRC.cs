using System;

namespace Assets.Scripts.Cryptography.CRC
{
    public static class CRC16
    {
        private unsafe static UInt16 CRC16Update(UInt16 crc, byte a)
        {
            byte i;

            crc = (UInt16)(crc ^ a);
            for(i = 0; i < 8; i++)
            {
                crc = (crc & 1) != 0 ? (UInt16)((crc >> 1) ^ 0xA001) : (UInt16)(crc >> 1);
            }

            return crc;
        }

        public static unsafe UInt16 Generate(byte* data, Int32 size)
        {
            UInt16 result = 0;
            for(Int32 i = 0; i < size; i++)
            {
                result = CRC16Update(result, data[i]);
            }

            return result;
        }
    }
}
