using System.ComponentModel.DataAnnotations;
namespace FirstAPI.Misc
{
    public class NameValidation : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            string name = value as string ?? "";
            if(string.IsNullOrEmpty(name))
              return false;
            foreach(char c in name)
            {
                if(!char.IsLetter(c) || !char.IsWhiteSpace(c))
                    return false;
            }
            return true;
        }
    }
}