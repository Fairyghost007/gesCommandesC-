namespace gesCommandes.Enums
{
    public enum TypePaiement
    {
        OM,
        Cheque,
        Wave
    }

    public static class TypePaiementHelper
    {
        public static string GetLabel(this TypePaiement typePaiement)
        {
            switch (typePaiement)
            {
                case TypePaiement.OM:
                    return "Virement";
                case TypePaiement.Cheque:
                    return "Chèque";
                case TypePaiement.Wave:
                    return "Wave";
                default:
                    return "";
            }
        }
    }
}
