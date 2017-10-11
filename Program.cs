using System;
using System.Windows.Forms;

namespace ConvertExecuteSQL
{
    internal class Program
    {
        // STAThread Attribute required for Clipboard interaction
        [STAThread]
        private static void Main(string[] args)
        {
            const string origSql = "";

            try
            {
                string newSql;

                // Take commandline parameter if it exists
                if (args != null && args.Length == 1)            
                {
                    newSql = ConvertSql(origSql);
                    Console.WriteLine(newSql);
                }
                // Use clipboard
                else
                {
                    if (Clipboard.ContainsText())
                    {
                        newSql = ConvertSql(Clipboard.GetText());
                        Clipboard.SetText(newSql);
                    }
                    else
                    {
                        throw new Exception("No Source SQL specified. Source SQL can either be specified as the first command line parameter, or taken from the clipboard");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static string ConvertSql(string origSql)
        {
            // Temporary replacement to simplify matching isolated single quotes
            var tmp = origSql.Replace("''", "~~");       
            string baseSql;
            var paramData = "";
            var i0 = tmp.IndexOf("'", StringComparison.Ordinal) + 1;
            var i1 = tmp.IndexOf("'", i0, StringComparison.Ordinal);
            if (i1 > 0)
            {
                // Main SQL statement is first parameter in single quotes
                baseSql = tmp.Substring(i0, i1 - i0);
                i0 = tmp.IndexOf("'", i1 + 1, StringComparison.Ordinal);
                i1 = tmp.IndexOf("'", i0 + 1, StringComparison.Ordinal);
                if (i0 > 0 && i1 > 0)
                {
                    paramData = tmp.Substring(i1 + 1);
                }
            }
            else
            {
                throw new Exception("Cannot identify SQL statement in first parameter");
            }

            // Undo initial temp replacement, and convert to single instance of single quote
            baseSql = baseSql.Replace("~~", "'");           
            if (string.IsNullOrEmpty(paramData))
            {
                return baseSql;
            }

            var paramList = paramData.Split(",".ToCharArray());
            foreach (var paramValue in paramList)
            {
                var iEq = paramValue.IndexOf("=", StringComparison.Ordinal);

                if (iEq < 0)
                {
                    continue;
                }

                var pName = paramValue.Substring(0, iEq).Trim();
                var pVal = paramValue.Substring(iEq + 1).Trim();

                baseSql = baseSql.Replace(pName, pVal);
            }

            return baseSql;
        }
    }
}
