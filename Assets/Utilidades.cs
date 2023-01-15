using System;
using System.Collections.Generic;
using UnityEngine;

public static class Utilidades{
    public static string FormatVector(Vector3 v){
        return v.x.ToString().Replace(",", ".")+"|"+v.y.ToString().Replace(",", ".")+"|"+v.z.ToString().Replace(",", ".");
    }

    public static Vector3 FormatString(string s){
        string[] nums = s.Split("|");
        return new Vector3(float.Parse(nums[0].Replace(".", ",")),float.Parse(nums[1].Replace(".", ",")),float.Parse(nums[2].Replace(".", ",")));
    }

    public static Dictionary<int,Jugador> Jugadores = new Dictionary<int, Jugador>();
    public static int nJugadores = 0;
    // public static Dictionary<int,string> nombres = new Dictionary<int, string>();
}