using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Makes sure TextMeshPro can draw Cyrillic (Ukrainian). If the project font has no Cyrillic glyphs,
// a dynamic font made from an operating-system font (Arial, Segoe UI ...) is added as a global fallback.
public static class LocFont
{
    static bool done;
    static TMP_FontAsset fallback;

    public static void Ensure()
    {
        if (done) return;
        done = true;
        try
        {
            TMP_FontAsset main = UIKit.Font;
            if (main == null && TMP_Settings.defaultFontAsset != null) main = TMP_Settings.defaultFontAsset;
            if (main != null && main.HasCharacter('Ж', true, true) && main.HasCharacter('ї', true, true) && main.HasCharacter('Ґ', true, true)) return;

            Font os = Font.CreateDynamicFontFromOSFont(new[] { "Arial", "Segoe UI", "Tahoma", "Verdana", "DejaVu Sans", "Liberation Sans", "Noto Sans" }, 36);
            if (os == null) return;
            fallback = TMP_FontAsset.CreateFontAsset(os);
            if (fallback == null) return;
            fallback.name = "UkrainianFallback";
            List<TMP_FontAsset> list = TMP_Settings.fallbackFontAssets;
            if (list != null && !list.Contains(fallback)) list.Add(fallback);
            if (main != null)
            {
                if (main.fallbackFontAssetTable == null) main.fallbackFontAssetTable = new List<TMP_FontAsset>();
                if (!main.fallbackFontAssetTable.Contains(fallback)) main.fallbackFontAssetTable.Add(fallback);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Loc] Could not prepare a Cyrillic fallback font: " + e.Message);
        }
    }
}
