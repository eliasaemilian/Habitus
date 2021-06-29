using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils {
    public static byte ConvertBoolArrayToByte( bool[] src ) {
        byte res = 0;
        int index = ( 8 - src.Length >= 0 ) ? 8 - src.Length : 0;

        for ( int i = 0; i < src.Length; i++ ) {
            if ( src[i] ) res |= (byte)( 1 << ( 7 - index ) );
            index++;
        }

        return res;
    }
}
