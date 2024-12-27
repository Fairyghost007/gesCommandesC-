namespace gesCommandes.Enums

{
    using gesCommandes.Models;
    public enum Role
    {
        CLIENT,
        RS,
        COMPTABLE
    }

    public static class RoleHelper
    {
        public static string GetRoleString(Role role)
        {
            switch (role)
            {
                case Role.CLIENT:
                    return "Client";
                case Role.RS:
                    return "RS";
                case Role.COMPTABLE:
                    return "Comptable";
            }

            return "";
        }
    }
}


