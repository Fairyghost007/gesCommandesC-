namespace gesCommandes.Enums
{
    public enum Etat
    {
        ENCOURS,
        VALIDE,
        PAYE
    }
    public static class EtatHelper
    {
        
        public static string GetEtatString(Etat etat)
        {
            switch (etat)
            {
                case Etat.ENCOURS:
                    return "En cours";
                case Etat.VALIDE:
                    return "Valide";
                case Etat.PAYE:
                    return "Paye";
            }

            return "";
        }
    }
}

