using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace RoomSpeach
{
    interface Skill
    {
        bool MakeCommand(string phrase);
    }

    static class SkillMethods
    {
        public static bool Include(string data, string val)
        {
            Regex r = new Regex(val);
            if (r.Match(data).Value != "")
                return true;
            else return false;
        }

        public static bool AnyWord(string data, params string[] vals)
        {
            for(int i = 0;i < vals.Length;i++)
            {
                if (Include(data, vals[i]))
                    return true;
            }
            return false;
        }

        public static string GetPage(string uri)
        {
            return (new StreamReader(WebRequest.Create(uri).GetResponse().GetResponseStream())).ReadToEnd();
        }
    }
}
