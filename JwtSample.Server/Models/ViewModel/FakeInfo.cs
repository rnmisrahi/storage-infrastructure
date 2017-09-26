using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtSample.Server.Models.ViewModel
{
    public static class FakeInfo
    {
        private static Random rnd = new Random(Guid.NewGuid().GetHashCode());

        public static List<string> getGenericList(string prefix)
        {
            string alfa = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

                List<string> list = new List<string>();
                foreach (char c in alfa)
                {
                    if (rnd.Next(100) > 80)
                        list.Add(prefix + c);
                }
                return list;
        }

        public static List<string> InfoList
        {
            get
            {
                return getGenericList("Info ");
            }
        }

        public static List<string> QuestionList
        {
            get
            {
                return getGenericList("Question ");
            }
        }

        public static List<string> ActivitiesList
        {
            get
            {
                return getGenericList("Activity ");
            }
        }

        public static List<string> OurFaveList
        {
            get
            {
                return getGenericList("Our Favorite ");
            }
        }


    }
}
