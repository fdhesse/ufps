using System;
using UnityEngine;

public static class MiscUtils
{
    //目前先用本地的,以后会在服务端统一
    public static float GetCurBattleTime()
    {
        if( BattleTimeUtil.Instance != null )
        {
            return BattleTimeUtil.Instance.CurTime;
        }

        return 0.0f;
    }

    public static float GetPercentAbsClamped( float Numerator, float Denominator )
    {
        if( Denominator <= 0.0f )
        {
            return 1.0f;
        }

        if( Numerator >= Denominator )
        {
            return 1.0f;
        }

        return Mathf.Abs( Numerator / Denominator );
    }

    public static string GetTimeStrBySec_M_S( float sec )
    {

        string result = "00:00";

        if( sec <= 0.0f )
        {
            return result;
        }

        float leftTime = sec;
        string strM = ((int)(leftTime / 60.0f)).ToString();
        string strS = (((int)leftTime) % 60).ToString();

        if (strM.Length == 1)
        {
            strM = "0" + strM;
        }

        if (strS.Length == 1)
        {
            strS = "0" + strS;
        }

        result = strM + ":" + strS;
        return result;
    }
}
