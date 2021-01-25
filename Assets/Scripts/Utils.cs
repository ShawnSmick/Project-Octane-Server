using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class Utils
{
    public static uint ConvertBoolArrayToUInt(bool[] source)
    {
        uint result = 0;
        int index = 0;
        // Loop through the array
        foreach (bool b in source)
        {
            // if the element is 'true' set the bit at that position
            if (b)
            {
                result |= (uint) (1 << index);
            }


            index++;
        }

        return result;
    }
    public static bool[] ConvertUIntToBoolArray(uint b)
    {
        // prepare the return result
        bool[] result = new bool[32];

        // check each bit in the byte. if 1 set to true, if 0 set to false
        for (int i = 0; i < 32; i++)
        {
            result[i] = (b & (1 << i)) == 0 ? false : true;
        }
    ;


        return result;
    }
}
