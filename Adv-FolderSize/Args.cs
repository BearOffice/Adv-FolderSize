using System;
using System.Collections.Generic;
using System.Linq;

namespace Adv_FolderSize
{
    public record ArgSet
    {
        // empty -> arg without key
        public string Key { get; init; } = "";

        // null -> value is required
        // not null -> value is optional
        // empty -> arg without value
        public string Default { get; init; } = "";
    }

    public static class Args
    {
        public static string[] Interprete(ArgSet[] argsets, string args, bool remainder = false)
        {
            var values = new List<string>();
            var leftargs = new List<string>();

            if (remainder)
                leftargs.Add(args);
            else if (args != "")
                leftargs.AddRange(args.Split(' '));

            foreach (var argset in argsets)
            {
                var key = argset.Key.ToUpper();
                var defaultvalue = argset.Default;

                var matchedarg = leftargs.Find(arg => arg.ToUpper().StartsWith(key));

                if (matchedarg != null)
                {
                    var matchedvalue = matchedarg[key.Length..];
                    if ((defaultvalue == "" && matchedvalue != "")  // empty -> arg without value
                        || (defaultvalue == null && matchedvalue == ""))  // null -> value is required
                        return null;
                    values.Add(matchedvalue);
                    leftargs.Remove(matchedarg);
                }
                else if (defaultvalue == null)  // null -> value is required
                {
                    return null;
                }
                else
                {
                    values.Add(defaultvalue);
                }
            }

            // Check the unregistered case
            if (leftargs.Count == 0)
                return values.ToArray();
            else
                return null;
        }
    }
}
