using System.Collections.Generic;

namespace DataRelay
{
    public class AccountSessionManager
    {
        private readonly Dictionary<LoginToken, string> _loggedUsers;

        public AccountSessionManager()
        {
            _loggedUsers = new Dictionary<LoginToken, string>();
        }

        public LoginToken Add(string userAccountId)
        {
            var token = new LoginToken();
            _loggedUsers.Add(token, userAccountId);
            return token;
        }

        public string GetAccountIdFromToken(LoginToken token)
        {
            string userAccountId;
            if (_loggedUsers.TryGetValue(token, out userAccountId))
            {
                return userAccountId;
            }
            throw new KeyNotFoundException();
        }
    }
}