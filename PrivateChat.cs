using System.ComponentModel.DataAnnotations.Schema;

namespace WalrusMessenger
{
    public class PrivateChat : Chat
    {
        public override List<long> Admins => new List<long>();

        internal PrivateChat(long id1, long id2) : base(new List<long> { id1, id2 }, "", null, null) { }
        public PrivateChat(List<long> users) : base(users, "", null, null) { }
    }
}
