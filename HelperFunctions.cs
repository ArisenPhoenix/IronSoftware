class Helpers
{
    public class AreAll
        {
            
            public bool IsRealNumber(string value){

            foreach (char c in value)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
            }
        }
}

