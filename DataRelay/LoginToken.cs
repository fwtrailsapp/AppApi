using System;
using System.Runtime.Serialization;

namespace DataRelay
{
    [DataContract]
    public class LoginToken
    {
        [DataMember]
        private readonly Guid token;

        public LoginToken() : this(Guid.NewGuid())
        {
        }

        public LoginToken(Guid token)
        {
            this.token = token;
        }

        protected bool Equals(LoginToken other)
        {
            return token.Equals(other.token);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LoginToken)obj);
        }

        public override int GetHashCode()
        {
            return token.GetHashCode();
        }
    }
}