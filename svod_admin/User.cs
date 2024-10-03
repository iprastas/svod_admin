namespace svod_admin
{
    public class User
    {
        public int id;
        public string login = string.Empty;
        public string? password;
        public DateTime passwordupto;
        public string territory = string.Empty;
        public string? department;
        public string? subjectname;
        public string? name;
        public string? note;
        public string? myformkinds;
        public string? myforms;
        public ulong icanflags;
        public string? changer;
        public string? username;
        public DateTime changedate;

        public string ToString(User us)
        {
            return $"{login} {password} {passwordupto} {territory} {department} {name} {note} {myformkinds} {myforms} {icanflags} {changer} {username} {changedate}";
        }
    }
}
