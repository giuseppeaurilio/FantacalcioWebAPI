using System.Collections.Generic;
using System.Linq;

namespace Membership
{
    public class User
    {
        public bool Authorized { get; set; }
        public string Username { get; set; }
        public List<PageAction> PageActionsList { get; set; }

        private User()
        {
            this.Authorized = false;
            this.Username = string.Empty;
            this.PageActionsList = new List<PageAction>();
        }

        public User(string username, string password)
        {
            if (username == "Admin" && password == "Password")
            {
                User user = CreateMock();
                this.Authorized = user.Authorized;
                this.Username = user.Username;
                this.PageActionsList = user.PageActionsList;
            }
        }

        public User(string username)
        {
            User user = CreateMock();
            this.Authorized = user.Authorized;
            this.Username = user.Username;
            this.PageActionsList = user.PageActionsList;
        }

        public static bool VerifyCredentials(string username, string password)
        {
            //Vediamo se le credenziali fornite sono valide
            User currUser = new User(username, password);
            return currUser.Authorized;
        }

        public static bool canExecute(string username, string controller, string action)
        {
            bool ret = false;
            User curreUser = new User(username);
            if (curreUser.PageActionsList.Any(p => p.Controller == controller && p.Action == action))
                ret = true;
            return ret;
        }

        private static User CreateMock()
        {
            return new User()
            {
                Authorized = true,
                Username = "Admin",
                PageActionsList = new List<PageAction>() { new PageAction() { Controller = "UserData", Action = "Get" } }
            };
        }
    }

    public class PageAction
    {
        public string Controller { get; set; }
        public string Action { get; set; }
    }
}
