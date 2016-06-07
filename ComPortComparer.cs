using System.Text.RegularExpressions;

namespace RN42HIDKeyboardTest
{
    class ComPortComparer : System.Collections.Generic.IComparer<string>
    {
        Regex reg = new Regex("COM(?<num>\\d+)");

        public int Compare(string a, string b)
        {
            int a_num = int.Parse(reg.Match(a).Groups["num"].Value);
            int b_num = int.Parse(reg.Match(b).Groups["num"].Value);

            if (a_num == b_num)
            {
                return 0;
            }
            if (a_num < b_num)
            {
                return -1;
            }

            return 1;
        }
    }
}