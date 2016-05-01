using System;
using System.Runtime.Serialization;

namespace DataRelay
{
    /// <summary>
    ///     A logintoken represents a session for the clients. Its underlying type is a Guid.
    /// </summary>
    [DataContract]
    public class LoginToken
    {
        [DataMember] private readonly Guid token;

        /// <summary>
        /// Instantiates a new LoginToken. It is guaranteed to be unique.
        /// </summary>
        public LoginToken() : this(Guid.NewGuid())
        {
        }

        /// <summary>
        /// Instantiates a new LoginToken with the given Guid.
        /// </summary>
        /// <param name="token">the existing Guid to create a LoginToken of</param>
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
            return Equals((LoginToken) obj);
        }

        public override int GetHashCode()
        {
            return token.GetHashCode();
        }
    }
}