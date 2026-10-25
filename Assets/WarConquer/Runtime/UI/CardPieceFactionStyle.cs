namespace WarConquer
{
    public static partial class CardPieceModels
    {
        sealed partial class Sculptor
        {
            // Applied after each card's anatomy and equipment have been seated. The style
            // remains entirely in the presentation hierarchy, before idle poses are captured.
            void ApplyFactionStyle()
            {
                switch(card.leader)
                {
                    case "SAHRIA":ApplySahriaStyle();break;
                    case "FAUNAR":ApplyFaunarStyle();break;
                    case "ZUKGROK":ApplyZukgrokStyle();break;
                    case "XIL'THAR":ApplyVoidStyle();break;
                }
            }
        }
    }
}
