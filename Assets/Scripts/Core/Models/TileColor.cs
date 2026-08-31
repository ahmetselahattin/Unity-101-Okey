using UnityEngine;

public enum TileColor
{
    Yellow,
    Blue,
    Black,
    Red
}

public static class TileColorExtensions
{
    // Kırmızı: Parlak, net ve tok kırmızı (#E02020)
    public static readonly Color RedColor = new Color(0.92f, 0.10f, 0.10f, 1f);

    // Sarı: Asla turuncuya kaçmayan, beyaz taş üzerinde belirgin parlayan altın sarısı (#E59E00)
    public static readonly Color YellowColor = new Color(0.94f, 0.65f, 0.02f, 1f);

    // Mavi: Canlı ve doygun kobalt mavisi (#0B66E4)
    public static readonly Color BlueColor = new Color(0.04f, 0.40f, 0.96f, 1f);

    // Siyah: Koyu ve net antrasit siyah (#141414)
    public static readonly Color BlackColor = new Color(0.08f, 0.08f, 0.08f, 1f);

    public static Color ToColor(this TileColor tileColor)
    {
        switch (tileColor)
        {
            case TileColor.Red:
                return RedColor;
            case TileColor.Yellow:
                return YellowColor;
            case TileColor.Blue:
                return BlueColor;
            case TileColor.Black:
            default:
                return BlackColor;
        }
    }
}
