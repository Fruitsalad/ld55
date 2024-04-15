using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Godot;


public static class Util {
    public const int NONE = -1;
    public const float EPSILON = 0.00001f;
    
    
    // Checked only in dev builds
    [System.Diagnostics.Conditional("DEBUG")]
    public static void assuming(bool assumption) {
        if (!assumption) {
            GD.PrintErr($"ASSUMPTION FAILED!\n{System.Environment.StackTrace}");
            throw new ZException("Sorry but something went wrong :(");
        }
    }

    // `verify` is like `assuming` but we also check in release builds
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void verify(bool assumption) {
        if (!assumption) {
            GD.PrintErr($"VERIFICATION FAILED!\n"
                        + $"{System.Environment.StackTrace}");
            throw new ZException("Sorry but something went wrong :(");
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void verify(bool assumption, string msg) {
        if (!assumption) {
            GD.PrintErr($"VERIFICATION FAILED!\n"
                        + $"{System.Environment.StackTrace}:\n"
                        + msg);
            throw new ZException(msg);
        }
    }
    
    
    // Arithmetic
    
    public static int modulo(int a, int b) {
        return ((a % b) + b) % b; 
    }
    
    public static int div_up(int a, int b) =>
        a / b + ((a > 0 && a % b != 0) ? 1 : 0);
    
    public static int div_down(int a, int b) =>
        a / b - ((a < 0 && a % b != 0) ? 1 : 0);

    // a1 & b1 are inclusive min, a2 & b2 are exclusive max
    public static bool do_ranges_overlap(int a1, int a2, int b1, int b2) {
        if (b1 < a1)
            return b2 >= a1;
        return b1 < a2 || b2 < a2;
    }
    
    
    // Random...

    public static T pick<T>(params T[] args) =>
        args[GD.RandRange(0, args.Length - 1)];
}