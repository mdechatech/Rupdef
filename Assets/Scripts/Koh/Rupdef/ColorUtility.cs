using JetBrains.Annotations;
using UnityEngine;

namespace Koh.Rupdef
{
    /// <summary>
    /// Helpers related to <see cref="Color"/> and <see cref="Color32"/>.
    /// </summary>
    public static class ColorUtility
    {
        /// <summary>
        /// Converts the given uint into a color.
        /// </summary>
        /// <see cref="color">The color uint in the format 0xRRGGBBAA.</see>
        [Pure]
        public static Color32 FromUint(uint color)
        {
            return new Color32(
                (byte)((color >> 24) & 0xff),
                (byte)((color >> 16) & 0xff),
                (byte)((color >> 8) & 0xff),
                (byte)(color & 0xff));
        }

        [Pure]
        public static Color WithRed(this Color color, float r)
        {
            return new Color(r, color.g, color.b, color.a);
        }

        [Pure]
        public static Color WithGreen(this Color color, float g)
        {
            return new Color(color.r, g, color.b, color.a);
        }

        [Pure]
        public static Color WithBlue(this Color color, float b)
        {
            return new Color(color.r, color.g, b, color.a);
        }

        [Pure]
        public static Color WithAlpha(this Color color, float a)
        {
            return new Color(color.r, color.g, color.b, a);
        }

        [Pure]
        public static Color32 WithRed(this Color32 color, byte r)
        {
            return new Color32(r, color.g, color.b, color.a);
        }

        [Pure]
        public static Color32 WithGreen(this Color32 color, byte g)
        {
            return new Color32(color.r, g, color.b, color.a);
        }

        [Pure]
        public static Color32 WithBlue(this Color32 color, byte b)
        {
            return new Color32(color.r, color.g, b, color.a);
        }

        [Pure]
        public static Color32 WithAlpha(this Color32 color, byte a)
        {
            return new Color32(color.r, color.g, color.b, a);
        }

        /// <summary>
        /// Encloses the given message with rich text color tags.
        /// </summary>
        [Pure]
        public static string Format(this Color32 color, string message)
        {
            return string.Format("<color={0}>{1}</color>", color.ToRichText(), message);
        }

        /// <summary>
        /// Encloses the given message with rich text color tags.
        /// </summary>
        [Pure]
        public static string Format(this Color color, string message)
        {
            return string.Format("<color={0}>{1}</color>", color.ToRichText(), message);
        }

        /// <summary>
        /// Encloses the given message with rich text color tags.
        /// </summary>
        [Pure]
        public static string WithColor(this string message, Color32 color)
        {
            return string.Format("<color={0}>{1}</color>", color.ToRichText(), message);
        }

        /// <summary>
        /// Encloses the given message with rich text color tags.
        /// </summary>
        [Pure]
        public static string WithColor(this string message, Color color)
        {
            return string.Format("<color={0}>{1}</color>", color.ToRichText(), message);
        }

        /// <summary>
        /// Formats the color for use in rich text color tags.
        /// </summary>
        [Pure]
        public static string ToRichText(this Color color)
        {
            return ((Color32) color).ToRichText();
        }


        /// <summary>
        /// Formats the color for use in rich text color tags.
        /// </summary>
        [Pure]
        public static string ToRichText(this Color32 color)
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color.r, color.g, color.b, color.a);
        }

    } // Class
} // Namespace
