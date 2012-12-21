namespace ClyngMobile
{
    public class NoSuchParameterException : System.Exception
    {
        public NoSuchParameterException(string message) : base( message )
        { 
        }
    }

    public class NoSuchUserException : System.Exception
    {
        public NoSuchUserException(string message)
            : base(message)
        {
        }
    }

    public class UserAlreadyExistException : System.Exception
    {
        public UserAlreadyExistException(string message)
            : base(message)
        {
        }
    }

    public class WrongUserNameOrPasswordException : System.Exception
    {
        public WrongUserNameOrPasswordException(string message)
            : base(message)
        {
        }
    }
}